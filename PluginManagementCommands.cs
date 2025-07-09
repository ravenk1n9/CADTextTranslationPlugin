// PluginManagementCommands.cs
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using System;
using System.IO;
using System.Reflection;
using TextTranslationPlugin.UI;

namespace TextTranslationPlugin
{
    public class PluginManagementCommands
    {
        [CommandMethod("AITRANSLATE_SETTINGS")]
        public void ShowSettingsDialog()
        {
            using (var settingsForm = new SettingsForm())
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(settingsForm);
            }
        }

        [CommandMethod("AITRANSLATE_ABOUT")]
        public void ShowAboutDialog()
        {
            using (var aboutForm = new AboutForm())
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(aboutForm);
            }
        }


        [CommandMethod("AITRANSLATION_MENU")]
        public void LoadMenuCommand()
        {
            if (Convert.ToInt32(Application.GetSystemVariable("MENUBAR")) != 1)
            {
                Application.SetSystemVariable("MENUBAR", 1);
            }

            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyLocation);
            string cuiPath = Path.Combine(assemblyDir, "AITRANSLATION.cuix");

            CuiUtils.LoadCuiFile(cuiPath);
            
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                doc.Editor.WriteMessage("\nAI翻译插件自定义界面(CUIX)已加载。");
            }
        }
    }
}