// TextTranslationApp.cs
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.IO;
using System.Reflection;

// Register plugin with AutoCAD
[assembly: ExtensionApplication(typeof(TextTranslationPlugin.TextTranslationApp))]
// Register custom commands
[assembly: CommandClass(typeof(TextTranslationPlugin.TextTranslationCommands))]
// Register new management commands
[assembly: CommandClass(typeof(TextTranslationPlugin.PluginManagementCommands))]

namespace TextTranslationPlugin
{
    public class TextTranslationApp : IExtensionApplication
    {
        public static OpenAIService OpenAIService { get; private set; }

        public void Initialize()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            try
            {
                ConfigReader.Initialize();

                ReinitializeOpenAIService();

                if (doc != null)
                {
                    string lispExpression = "(if (not (menugroup \"AITRANSLATION\")) (command \"._AITRANSLATION_MENU\"))(princ) ";
                    doc.SendStringToExecute(lispExpression, false, false, false);
                }

                if (OpenAIService == null)
                {
                    if (doc != null)
                    {
                        doc.Editor.WriteMessage("\n警告: API配置加载失败或未配置API Key。插件可能无法工作。请使用 AITRANSLATE_SETTINGS 命令进行设置。");
                    }
                }

                if (doc != null)
                {
                    doc.Editor.WriteMessage("\n文字翻译插件加载成功  作者:姚京天");
                    doc.Editor.WriteMessage("\n文字翻译命令: TRANSLATETEXT");
                    doc.Editor.WriteMessage("\n插件设置命令: AITRANSLATE_SETTINGS");
                    doc.Editor.WriteMessage("\n关于插件命令: AITRANSLATE_ABOUT");
                }
            }
            catch (System.Exception ex)
            {
                var errorDoc = Application.DocumentManager.MdiActiveDocument;
                if (errorDoc != null)
                {
                    errorDoc.Editor.WriteMessage($"\nError loading Text Translation Plugin: {ex.Message}");
                }
            }
        }

        public static void ReinitializeOpenAIService()
        {
            OpenAIConfig config = ConfigReader.ReadOpenAIConfig();
            
            // 场景1：新配置无效（例如API Key为空）
            if (config == null)
            {
                // 销毁现有服务（如果存在），并将引用设为null
                (OpenAIService as IDisposable)?.Dispose();
                OpenAIService = null;
                return;
            }

            // 场景2：服务实例尚未创建（插件首次加载）
            if (OpenAIService == null)
            {
                // 创建一个全新的服务实例
                OpenAIService = new OpenAIService(config);
            }
            // 场景3：服务实例已存在
            else
            {
                // 调用其UpdateConfig方法，传入新配置，以保留内部状态（如缓存）
                OpenAIService.UpdateConfig(config);
            }
        }

        public void Terminate()
        {
            try
            {
                if (OpenAIService != null)
                {
                    (OpenAIService as IDisposable)?.Dispose();
                    OpenAIService = null;
                }
            }
            catch
            {
                // 忽略终止时的错误
            }
        }
    }
}