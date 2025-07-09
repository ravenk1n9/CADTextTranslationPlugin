// UI/SettingsForm.cs
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TextTranslationPlugin.UI
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                ConfigReader.ReloadConfig();

                txtApiKey.Text = ConfigReader.GetConfigValue("APIKEY");
                txtBaseUrl.Text = ConfigReader.GetConfigValue("BASEURL", "https://api.openai.com/v1/chat/completions");
                txtModel.Text = ConfigReader.GetConfigValue("MODEL", "gpt-4");
                txtSystemPrompt.Text = ConfigReader.GetConfigValue("SYSTEMPROMPT");
                txtSourceLayer.Text = ConfigReader.GetSourceLayer();
                txtTargetLayer.Text = ConfigReader.GetTargetLayer();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载配置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var settings = new Dictionary<string, string>
                {
                    { "APIKEY", txtApiKey.Text.Trim() },
                    { "BASEURL", txtBaseUrl.Text.Trim() },
                    { "MODEL", txtModel.Text.Trim() },
                    { "SYSTEMPROMPT", txtSystemPrompt.Text },
                    { "SOURCE_LAYER", txtSourceLayer.Text.Trim() },
                    { "TARGET_LAYER", txtTargetLayer.Text.Trim() }
                };

                ConfigReader.SaveConfig(settings);
                ConfigReader.ReloadConfig(); 
                TextTranslationApp.ReinitializeOpenAIService();

                MessageBox.Show("设置已成功保存并应用！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存配置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}