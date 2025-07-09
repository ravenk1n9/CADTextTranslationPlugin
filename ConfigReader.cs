// ConfigReader.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TextTranslationPlugin
{
    public static class ConfigReader
    {
        private static Dictionary<string, string> _configValues;
        private static readonly string ConfigPath;

        static ConfigReader()
        {
            ConfigPath = Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "api.env"
            );
        }

        public static void Initialize()
        {
            _configValues = LoadConfigValues();
        }

        private static Dictionary<string, string> LoadConfigValues()
        {
            var values = new Dictionary<string, string>();

            if (!File.Exists(ConfigPath))
            {
                return values;
            }

            try
            {
                string[] lines = File.ReadAllLines(ConfigPath);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("//") || line.Trim().StartsWith("#"))
                        continue;

                    string[] parts = line.Split(new[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();

                        if (value.StartsWith("\"") && value.EndsWith("\""))
                        {
                            value = value.Substring(1, value.Length - 2);
                        }
                        values[key] = value;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading configuration file: {ex.Message}", "配置错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return values;
        }

        public static void SaveConfig(Dictionary<string, string> settings)
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    var initialLines = settings.Select(kvp =>
                    {
                        if (kvp.Key == "SYSTEMPROMPT")
                            return $"{kvp.Key}=\"{kvp.Value}\"";
                        return $"{kvp.Key}={kvp.Value}";
                    }).ToList();
                    File.WriteAllLines(ConfigPath, initialLines, Encoding.UTF8);
                    return;
                }

                var lines = File.ReadAllLines(ConfigPath).ToList();
                var settingsToUpdate = new Dictionary<string, string>(settings);

                for (int i = 0; i < lines.Count; i++)
                {
                    var trimmedLine = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("//") || trimmedLine.StartsWith("#"))
                    {
                        continue;
                    }

                    var parts = lines[i].Split(new[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        if (settingsToUpdate.ContainsKey(key))
                        {
                            string newValue = settingsToUpdate[key];
                            if (key == "SYSTEMPROMPT")
                            {
                                lines[i] = $"{key}=\"{newValue}\"";
                            }
                            else
                            {
                                lines[i] = $"{key}={newValue}";
                            }
                            settingsToUpdate.Remove(key);
                        }
                    }
                }

                if (settingsToUpdate.Any())
                {
                    foreach (var newSetting in settingsToUpdate)
                    {
                        if (newSetting.Key == "SYSTEMPROMPT")
                        {
                            lines.Add($"{newSetting.Key}=\"{newSetting.Value}\"");
                        }
                        else
                        {
                            lines.Add($"{newSetting.Key}={newSetting.Value}");
                        }
                    }
                }

                File.WriteAllLines(ConfigPath, lines, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing configuration file: {ex.Message}", "配置错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        /// <summary>
        /// 重新加载配置。这会更新内部缓存并重新初始化所有依赖的服务。
        /// 这是确保配置同步的唯一真实来源。
        /// </summary>
        public static void ReloadConfig()
        {
            // 步骤 1: 重新从文件加载配置到内存缓存
            Initialize();

            // 步骤 2: 重新初始化依赖于此配置的服务
            TextTranslationApp.ReinitializeOpenAIService();
        }

        public static OpenAIConfig ReadOpenAIConfig()
        {
            if (_configValues == null)
            {
                Initialize();
            }
            
            OpenAIConfig config = new OpenAIConfig
            {
                ApiKey = GetConfigValue("APIKEY"),
                BaseUrl = GetConfigValue("BASEURL", "https://api.openai.com/v1/chat/completions"),
                Model = GetConfigValue("MODEL", "gpt-4"),
                SystemPrompt = GetConfigValue("SYSTEMPROMPT")
            };

            if (string.IsNullOrEmpty(config.ApiKey) || config.ApiKey == "your_api_key")
            {
                return null;
            }
            return config;
        }

        public static string GetSourceLayer()
        {
            if (_configValues == null) Initialize();
            return GetConfigValue("SOURCE_LAYER", "0文字标注");
        }

        public static string GetTargetLayer()
        {
            if (_configValues == null) Initialize();
            return GetConfigValue("TARGET_LAYER", "0TXT");
        }

        public static string GetConfigValue(string key, string defaultValue = null)
        {
            if (_configValues != null && _configValues.TryGetValue(key, out string value))
            {
                return value;
            }
            return defaultValue;
        }
    }
}