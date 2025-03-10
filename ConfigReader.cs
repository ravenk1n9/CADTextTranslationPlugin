// ConfigReader.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace TextTranslationPlugin
{
    public static class ConfigReader
    {
        private static Dictionary<string, string> _configValues;

        static ConfigReader()
        {
            _configValues = LoadConfigValues();
        }

        private static Dictionary<string, string> LoadConfigValues()
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            string configPath = Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "api.env"
            );

            if (!File.Exists(configPath))
            {
                MessageBox.Show($"Configuration file not found at: {configPath}", "配置错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return values; // 返回空字典，后续代码会使用默认值
            }

            try
            {
                string[] lines = File.ReadAllLines(configPath);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    string[] parts = line.Split(new[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();

                        // Remove quotes if present
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

        public static OpenAIConfig ReadOpenAIConfig()
        {
            OpenAIConfig config = new OpenAIConfig
            {
                ApiKey = GetConfigValue("APIKEY"),
                BaseUrl = GetConfigValue("BASEURL", "https://api.openai.com/v1/chat/completions"), // 默认使用 OpenAI 官方 API 地址
                Model = GetConfigValue("MODEL", "gpt-4"),
                SystemPrompt = GetConfigValue("SYSTEMPROMPT")
            };

            // Validate the config
            if (string.IsNullOrEmpty(config.ApiKey))
            {
                MessageBox.Show("未正确配置API KEY.", "配置错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return config;
        }

        public static string GetSourceLayer()
        {
            return GetConfigValue("SOURCE_LAYER", "0文字标注"); // 默认值
        }

        public static string GetTargetLayer()
        {
            return GetConfigValue("TARGET_LAYER", "0TXT");     // 默认值
        }

        private static string GetConfigValue(string key, string defaultValue = null)
        {
            if (_configValues.TryGetValue(key, out string value))
            {
                return value;
            }
            return defaultValue;
        }
    }
}