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
        private readonly OpenAIConfig _config;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _translationCache;
        private readonly SemaphoreSlim _semaphore; // 用于限制并发请求

        public OpenAIService(OpenAIConfig config)
        {
            _config = config;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30); // 添加超时设置
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");
            _translationCache = new Dictionary<string, string>();
            _semaphore = new SemaphoreSlim(3); // 最多3个并发请求
        }

        public async Task<string> TranslateTextAsync(string text)
        {
            try
            {
                // Skip empty text
                if (string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }

                // Check if we already translated this text
                lock (_translationCache)
                {
                    if (_translationCache.ContainsKey(text))
                    {
                        return _translationCache[text];
                    }
                }

                await _semaphore.WaitAsync(); // 获取信号量，控制并发

                try
                {
                    // Create the request payload
                    var requestData = new
                    {
                        model = _config.Model,
                        messages = new[]
                        {
                            new { role = "system", content = "你是一位建筑幕墙词汇翻译专家。将用户提供的中文翻译成英文，注意使用专业词汇。只输出翻译结果，不需要解释。" },
                            new { role = "user", content = text }
                        },
                        temperature = 0.3
                    };

                    string jsonRequest = JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    // 使用异步API调用
                    HttpResponseMessage response = await _httpClient.PostAsync(_config.BaseUrl, content);

                    // Parse the response
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        var responseObject = JsonConvert.DeserializeObject<OpenAIResponse>(responseContent);

                        if (responseObject?.choices?.Length > 0 && responseObject.choices[0]?.message?.content != null)
                        {
                            string translatedText = responseObject.choices[0].message.content.Trim();

                            // 线程安全地缓存翻译结果
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
                    _semaphore.Release(); // 释放信号量
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Translation exception: {ex.Message}");
                return null;
            }
        }

        // 批量翻译文本的方法
        public async Task<Dictionary<string, string>> BatchTranslateAsync(List<string> texts)
        {
            Dictionary<string, string> results = new Dictionary<string, string>();

            // 先检查缓存
            List<string> textsToTranslate = new List<string>();
            lock (_translationCache)
            {
                foreach (string text in texts)
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

            // 对未缓存的文本进行分批处理
            var batches = SplitIntoBatches(textsToTranslate, 5); // 每批5个文本
            foreach (var batch in batches)
            {
                var tasks = batch.Select(text => TranslateTextAsync(text)).ToList();
                var translatedTexts = await Task.WhenAll(tasks);

                for (int i = 0; i < batch.Count; i++)
                {
                    string originalText = batch[i];
                    string translatedText = translatedTexts[i] ?? originalText; // 如果翻译失败，使用原文
                    results[originalText] = translatedText;
                }
            }

            return results;
        }

        // 将列表分成批次
        private List<List<T>> SplitIntoBatches<T>(List<T> source, int batchSize)
        {
            List<List<T>> batches = new List<List<T>>();
            for (int i = 0; i < source.Count; i += batchSize)
            {
                batches.Add(source.Skip(i).Take(batchSize).ToList());
            }
            return batches;
        }

        // 实现IDisposable接口，确保资源被正确释放
        public void Dispose()
        {
            try
            {
                // 释放HTTP客户端
                _httpClient?.Dispose();

                // 释放信号量
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
    }

    public class OpenAIResponse
    {
        public Choice[] choices { get; set; }

        public class Choice
        {
            public Message message { get; set; }
        }

        public class Message
        {
            public string content { get; set; }
        }
    }
}