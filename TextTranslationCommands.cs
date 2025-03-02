// TextTranslationCommands.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System.Windows.Forms;

namespace TextTranslationPlugin
{
    public class TextTranslationCommands
    {
        private const string SOURCE_LAYER = "0文字标注";
        private const string TARGET_LAYER = "0TXT";

        // Register the command to be called from AutoCAD
        [CommandMethod("TRANSLATETEXT")]
        public void TranslateText()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                MessageBox.Show("当前没有打开的文档。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Database db = doc.Database;
            Editor ed = doc.Editor;

            // 使用单例OpenAI服务
            OpenAIService openAIService = TextTranslationApp.OpenAIService;

            // 检查服务是否成功初始化
            if (openAIService == null)
            {
                ed.WriteMessage("\nOpenAI 服务未初始化。请检查插件配置。");
                MessageBox.Show("OpenAI 服务未初始化。请检查插件配置并重新加载插件。", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 显示选择提示，更明确的指示
            ed.WriteMessage($"\n选择位于 '{SOURCE_LAYER}' 图层上的文字对象进行翻译... (按 Enter 键完成选择)");

            // 获取用户选择
            PromptSelectionResult selRes = ed.GetSelection();
            if (selRes.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\n未选择任何对象或选择被取消。");
                return;
            }

            SelectionSet ss = selRes.Value;
            if (ss == null || ss.Count == 0)
            {
                ed.WriteMessage("\n选择集中没有对象。");
                return;
            }

            System.Threading.Thread translationThread = new System.Threading.Thread(() =>
            {
                try
                {
                    ProcessTextObjectsAsync(doc, openAIService, ss).Wait();
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage($"\n翻译线程中发生错误: {ex.Message}");
                    ed.WriteMessage($"\n异常堆栈跟踪: {ex.StackTrace}");
                    MessageBox.Show($"翻译失败: {ex.Message}\n\n堆栈跟踪:\n{ex.StackTrace}", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });

            translationThread.SetApartmentState(System.Threading.ApartmentState.STA);
            translationThread.Start();
        }

        private async Task ProcessTextObjectsAsync(Document doc, OpenAIService openAIService, SelectionSet ss)
        {
            Editor ed = doc.Editor;
            Database db = doc.Database;

            try
            {
                List<string> textsToTranslate = new List<string>();
                Dictionary<ObjectId, string> textContents = new Dictionary<ObjectId, string>();

                using (doc.LockDocument())
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    // 确保目标图层存在
                    LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                    ObjectId targetLayerId;

                    if (!lt.Has(TARGET_LAYER))
                    {
                        lt.UpgradeOpen();
                        LayerTableRecord ltr = new LayerTableRecord();
                        ltr.Name = TARGET_LAYER;
                        targetLayerId = lt.Add(ltr);
                        trans.AddNewlyCreatedDBObject(ltr, true);
                    }
                    else
                    {
                        targetLayerId = lt[TARGET_LAYER];
                    }

                    foreach (SelectedObject selObj in ss)
                    {
                        Entity ent = trans.GetObject(selObj.ObjectId, OpenMode.ForRead) as Entity;
                        if (ent is DBText text && text.Layer == SOURCE_LAYER)
                        {
                            string content = text.TextString;
                            textContents[selObj.ObjectId] = content;
                            textsToTranslate.Add(content);
                        }
                    }
                    trans.Commit();
                }

                if (textsToTranslate.Count == 0)
                {
                    ed.WriteMessage("\n在源图层上没有找到文字对象。");
                    return;
                }

                Dictionary<string, string> translatedTexts = await openAIService.BatchTranslateAsync(textsToTranslate);

                using (doc.LockDocument())
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    try
                    {
                        // 获取目标图层
                        LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                        ObjectId targetLayerId = lt[TARGET_LAYER];

                        // 获取当前空间记录
                        BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                        BlockTableRecord btr = (BlockTableRecord)trans.GetObject(
                            db.CurrentSpaceId,
                            OpenMode.ForWrite
                        );

                        int processedCount = 0;

                        foreach (var pair in textContents)
                        {
                            ObjectId objectId = pair.Key;
                            string originalText = pair.Value;
                            string translatedText;
                            if (!translatedTexts.TryGetValue(originalText, out translatedText))
                            {
                                translatedText = originalText; // 翻译失败时使用原文
                            }

                            Entity ent = trans.GetObject(objectId, OpenMode.ForRead) as Entity;
                            if (ent is DBText)
                            {
                                DBText text = ent as DBText;
                                processedCount += ProcessDBText(trans, text, targetLayerId, btr, translatedText);
                            }
                        }

                        trans.Commit();
                        ed.WriteMessage($"\n处理并翻译了 {processedCount} 个文字对象。");
                    }
                    catch (System.Exception innerEx)
                    {
                        ed.WriteMessage($"\n事务处理中发生错误: {innerEx.Message}");
                        ed.WriteMessage($"\n内部异常堆栈跟踪: {innerEx.StackTrace}");
                        trans.Abort();
                        throw;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\nProcessTextObjectsAsync 方法中发生错误: {ex.Message}");
                ed.WriteMessage($"\nProcessTextObjectsAsync 异常堆栈跟踪: {ex.StackTrace}");
                MessageBox.Show($"文字处理过程中发生错误: {ex.Message}\n\n堆栈跟踪:\n{ex.StackTrace}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Process a DBText entity
        private int ProcessDBText(
            Transaction trans,
            DBText text,
            ObjectId targetLayerId,
            BlockTableRecord btr,
            string translatedText)
        {
            try
            {
                // Get the height of the text
                double textHeight = text.Height;

                // Create a copy of the text
                DBText newText = text.Clone() as DBText;

                // 只使用textHeight作为位移距离
                double offsetInUnits = textHeight;

                // Move it down using displacement vector
                Vector3d moveVector = new Vector3d(0, -offsetInUnits, 0);
                Matrix3d moveMatrix = Matrix3d.Displacement(moveVector);
                newText.TransformBy(moveMatrix);

                // Change the layer
                newText.LayerId = targetLayerId;

                // Update the text content
                newText.TextString = translatedText;

                // Add it to the database
                btr.AppendEntity(newText);
                trans.AddNewlyCreatedDBObject(newText, true);

                return 1;
            }
            catch (System.Exception ex)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor
                    .WriteMessage($"\n处理 DBText 时发生错误: {ex.Message}");
                return 0;
            }
        }
    }
}