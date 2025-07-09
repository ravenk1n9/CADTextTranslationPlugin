// CuiUtils.cs
using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.IO;

namespace TextTranslationPlugin
{
    public static class CuiUtils
    {
        /// <summary>
        /// 加载指定的CUIX文件，并妥善处理系统变量。
        /// </summary>
        /// <param name="cuiFilePath">CUIX文件的完整路径。</param>
        public static void LoadCuiFile(string cuiFilePath)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            // 检查文件是否存在，避免不必要的错误
            if (!File.Exists(cuiFilePath))
            {
                doc.Editor.WriteMessage($"\n错误：未找到CUIX文件: {cuiFilePath}");
                return;
            }

            // 保存系统变量的当前状态
            object oldCmdEcho = Application.GetSystemVariable("CMDECHO");
            object oldFileDia = Application.GetSystemVariable("FILEDIA");

            try
            {
                // 设置系统变量以实现静默加载
                Application.SetSystemVariable("CMDECHO", 0);
                Application.SetSystemVariable("FILEDIA", 0);

                // 构建并执行CUILOAD命令
                // 关键：在文件路径两边加上引号，以正确处理路径中可能存在的空格
                string command = $"_.cuiload \"{cuiFilePath}\" ";
                doc.SendStringToExecute(command, false, false, false);
            }
            catch (Exception ex)
            {
                doc.Editor.WriteMessage($"\n加载CUIX文件时发生错误: {ex.Message}");
            }
            finally
            {
                // 使用LISP表达式可靠地恢复系统变量，即使发生错误也能执行
                string restoreFileDia = $"(setvar \"FILEDIA\" {oldFileDia.ToString()})(princ) ";
                string restoreCmdEcho = $"(setvar \"CMDECHO\" {oldCmdEcho.ToString()})(princ) ";
                doc.SendStringToExecute(restoreFileDia, false, false, false);
                doc.SendStringToExecute(restoreCmdEcho, false, false, false);
            }
        }
    }
}