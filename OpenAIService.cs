// OpenAIService.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

namespace TextTranslationPlugin
{
    public class OpenAIService : IDisposable
    {
        private OpenAIConfig _config;
        private HttpClient _httpClient;
        private readonly Dictionary<string, string> _translationCache;
        private readonly SemaphoreSlim _semaphore;

        public OpenAIService(OpenAIConfig config)
        {
            _config = config;
            _translationCache = new Dictionary<string, string>();
            _semaphore = new SemaphoreSlim(3);
            // 将HttpClient的创建分离到一个方法中，以便复用
            InitializeHttpClient();
        }

        private void InitializeHttpClient()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");
        }

        /// <summary>
        /// 此方法会更新配置并重建HttpClient，但会保留翻译缓存。
        /// </summary>
        /// <param name="newConfig">新的配置对象</param>
        public void UpdateConfig(OpenAIConfig newConfig)
        {
            _config = newConfig;

            _httpClient?.Dispose();
            
            InitializeHttpClient();
        }

        public async Task<string> TranslateTextAsync(string text)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }

                lock (_translationCache)
                {
                    if (_translationCache.ContainsKey(text))
                    {
                        return _translationCache[text];
                    }
                }

                await _semaphore.WaitAsync();

                try
                {
                    var requestData = new
                    {
                        model = _config.Model,
                        messages = new[]
                        {
                            new { role = "system", content = _config.SystemPrompt },
                            new { role = "user", content = text }
                        },
                        temperature = 0.3
                    };

                    string jsonRequest = JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _httpClient.PostAsync(_config.BaseUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        var responseObject = JsonConvert.DeserializeObject<OpenAIResponse>(responseContent);

                        if (responseObject?.choices?.Length > 0 && responseObject.choices[0]?.message?.content != null)
                        {
                            string translatedText = responseObject.choices[0].message.content.Trim();
                            lock (_translationCache)
                            {
                                _translationCache[text] = translatedText;
                            }
                            return translatedText;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"API Response Error: Invalid response format.");
                            return null;
                        }
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode}\n{errorContent}");
                        return null;
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Translation exception: {ex.Message}");
                return null;
            }
        }

        public async Task<Dictionary<string, string>> BatchTranslateAsync(List<string> texts)
        {
            Dictionary<string, string> results = new Dictionary<string, string>();
            List<string> textsToTranslate = new List<string>();

            lock (_translationCache)
            {
                foreach (string text in texts.Distinct()) 
                {
                    if (string.IsNullOrWhiteSpace(text))
                        continue;

                    if (_translationCache.ContainsKey(text))
                    {
                        results[text] = _translationCache[text];
                    }
                    else
                    {
                        textsToTranslate.Add(text);
                    }
                }
            }

            if (textsToTranslate.Count > 0)
            {
                var tasks = textsToTranslate.Select(text => TranslateTextAsync(text)).ToList();
                await Task.WhenAll(tasks);

                foreach (var text in textsToTranslate)
                {
                    lock (_translationCache)
                    {
                        results[text] = _translationCache.ContainsKey(text) ? _translationCache[text] : text;
                    }
                }
            }

            var finalResults = new Dictionary<string, string>();
            foreach (var text in texts)
            {
                finalResults[text] = results.ContainsKey(text) ? results[text] : text;
            }

            return finalResults;
        }


        public void Dispose()
        {
            try
            {
                _httpClient?.Dispose();
                _semaphore?.Dispose();
            }
            catch
            {
                // 忽略释放时的错误
            }
        }
    }

    public class OpenAIConfig
    {
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public string Model { get; set; }
        public string SystemPrompt { get; set; }
    }

    public class OpenAIResponse
    {
        public Choice[] choices { get; set; }
        public class Choice { public Message message { get; set; } }
        public class Message { public string content { get; set; } }
    }
}