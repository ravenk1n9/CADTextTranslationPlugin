// TextTranslationApp.cs
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using System;

// Register plugin with AutoCAD
[assembly: ExtensionApplication(typeof(TextTranslationPlugin.TextTranslationApp))]
// Register custom commands
[assembly: CommandClass(typeof(TextTranslationPlugin.TextTranslationCommands))]

namespace TextTranslationPlugin
{
    public class TextTranslationApp : IExtensionApplication
    {
        // 静态引用OpenAI服务，允许多个命令共享同一个实例
        public static OpenAIService OpenAIService { get; private set; }

        public void Initialize()
        {
            var doc = Application.DocumentManager.MdiActiveDocument; // 在 try 块外部声明 doc 变量
            try
            {
                // 初始化配置
                OpenAIConfig config = ConfigReader.ReadConfig();
                if (config != null)
                {
                    // 创建单例服务
                    OpenAIService = new OpenAIService(config);
                }
                else
                {
                    // 如果配置读取失败，OpenAIService 将为 null，在命令执行时会进行检查
                    if (doc != null) // 使用外部作用域的 doc 变量
                    {
                        doc.Editor.WriteMessage("\nFailed to load API configuration. Text Translation Plugin may not function correctly.");
                    }
                    return; // 初始化失败，直接返回
                }

                // 使用命令行输出而不是弹窗，减少干扰
                if (doc != null) // 使用外部作用域的 doc 变量
                {
                    doc.Editor.WriteMessage("\nText Translation Plugin v0.0.3.0 loaded successfully.");
                    doc.Editor.WriteMessage("\nUse TRANSLATETEXT command to translate selected text objects.");
                }
            }
            catch (System.Exception ex) // 明确指定使用 System.Exception
            {
                // 出错时记录到命令行
                var errorDoc = Application.DocumentManager.MdiActiveDocument; // 修改 catch 块内的变量名为 errorDoc
                if (errorDoc != null) // 使用新的变量名 errorDoc
                {
                    errorDoc.Editor.WriteMessage($"\nError loading Text Translation Plugin: {ex.Message}");
                }
            }
        }

        public void Terminate()
        {
            try
            {
                // 释放资源
                if (OpenAIService != null)
                {
                    // OpenAIService可能包含HttpClient等需要显式释放的资源
                    // 在OpenAIService中添加Dispose方法来实现资源释放
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