// ConfigReader.cs
using System;
using System.Collections.Generic;
using System.IO;

namespace TextTranslationPlugin
{
    public static class ConfigReader
    {
        public static OpenAIConfig ReadConfig()
        {
            try
            {
                string configPath = Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    "api.env"
                );

                if (!File.Exists(configPath))
                {
                    System.Windows.Forms.MessageBox.Show($"Configuration file not found at: {configPath}");
                    return null;
                }

                Dictionary<string, string> configValues = new Dictionary<string, string>();

                // Read the config file
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

                        configValues[key] = value;
                    }
                }

                // Create and populate the config object
                OpenAIConfig config = new OpenAIConfig
                {
                    ApiKey = GetConfigValue(configValues, "APIKEY"),
                    BaseUrl = GetConfigValue(configValues, "BASEURL", "https://api.openai.com/v1/chat/completions"), // 默认使用 OpenAI 官方 API 地址
                    Model = GetConfigValue(configValues, "MODEL", "gpt-4"),
                    SystemPrompt = GetConfigValue(configValues, "SYSTEMPROMPT")
                };

                // Validate the config
                if (string.IsNullOrEmpty(config.ApiKey))
                {
                    System.Windows.Forms.MessageBox.Show("未正确配置API KEY.");
                    return null;
                }

                return config;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error reading configuration: {ex.Message}");
                return null;
            }
        }

        private static string GetConfigValue(Dictionary<string, string> configValues, string key, string defaultValue = null)
        {
            if (configValues.TryGetValue(key, out string value))
            {
                return value;
            }
            return defaultValue;
        }
    }
}