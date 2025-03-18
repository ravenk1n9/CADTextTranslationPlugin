// TextTranslationCommands.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System.Windows.Forms;
using System.IO;

namespace TextTranslationPlugin
{
    public class TextTranslationCommands
    {
        private static string SOURCE_LAYER; // 从配置文件读取
        private static string TARGET_LAYER; // 从配置文件读取

        // 静态构造函数，在类加载时读取配置文件
        static TextTranslationCommands()
        {
            SOURCE_LAYER = ConfigReader.GetSourceLayer(); // 加载源图层配置
            TARGET_LAYER = ConfigReader.GetTargetLayer(); // 加载目标图层配置
        }

        // Register the command to be called from AutoCAD
        [CommandMethod("TRANSLATETEXT")]
        public async void TranslateText() 
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
            ed.WriteMessage($"\n选择位于 '{SOURCE_LAYER}' 图层上的文字和多行文字对象进行翻译... (按 Enter 键完成选择)");

            // 获取用户选择，允许选择多种类型的对象
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

            try
            {
                // 直接 await 异步方法，避免线程阻塞
                await ProcessTextObjectsAsync(doc, openAIService, ss);
                ed.WriteMessage("\n文字和多行文字翻译完成。"); // 翻译完成后提示
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\n翻译过程中发生错误: {ex.Message}");
                ed.WriteMessage($"\n异常堆栈跟踪: {ex.StackTrace}");
                MessageBox.Show($"翻译失败: {ex.Message}\n\n堆栈跟踪:\n{ex.StackTrace}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ProcessTextObjectsAsync(Document doc, OpenAIService openAIService, SelectionSet ss)
        {
            Editor ed = doc.Editor;
            Database db = doc.Database;

            try
            {
                List<string> dbTextsToTranslate = new List<string>(); // 用于存储 DBText 的翻译文本
                Dictionary<ObjectId, string> dbTextContents = new Dictionary<ObjectId, string>(); // 存储 DBText 的 ObjectId 和内容

                List<string> mTextsToTranslate = new List<string>(); // 用于存储 MText 的翻译文本
                Dictionary<ObjectId, string> mTextContents = new Dictionary<ObjectId, string>(); // 存储 MText 的 ObjectId 和内容

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

                    // 分离 DBText 和 MText 的处理
                    foreach (SelectedObject selObj in ss)
                    {
                        Entity ent = trans.GetObject(selObj.ObjectId, OpenMode.ForRead) as Entity;
                        if (ent is DBText text && text.Layer == SOURCE_LAYER)
                        {
                            string content = text.TextString;
                            dbTextContents[selObj.ObjectId] = content;
                            dbTextsToTranslate.Add(content);
                        }
                        else if (ent is MText mtext && mtext.Layer == SOURCE_LAYER) // 处理 MText
                        {
                            string content = mtext.Contents; // 使用 Contents 属性
                            mTextContents[selObj.ObjectId] = content;
                            mTextsToTranslate.Add(content);
                        }
                    }
                    trans.Commit();
                }

                // 先处理 DBText
                if (dbTextsToTranslate.Count > 0)
                {
                    ed.WriteMessage("\n开始翻译单行文字...");
                    Dictionary<string, string> translatedDBTexts = await openAIService.BatchTranslateAsync(dbTextsToTranslate);

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

                            int processedDBTextCount = 0;
                            foreach (var pair in dbTextContents)
                            {
                                ObjectId objectId = pair.Key;
                                string originalText = pair.Value;
                                string translatedText;
                                if (!translatedDBTexts.TryGetValue(originalText, out translatedText))
                                {
                                    translatedText = originalText; // 翻译失败时使用原文替代
                                }

                                Entity ent = trans.GetObject(objectId, OpenMode.ForRead) as Entity;
                                if (ent is DBText)
                                {
                                    DBText text = ent as DBText;
                                    processedDBTextCount += ProcessDBText(trans, text, targetLayerId, btr, translatedText);
                                }
                            }
                            ed.WriteMessage($"\n处理并翻译了 {processedDBTextCount} 个单行文字对象。");
                            trans.Commit(); // 提交 DBText 的事务
                        }
                        catch (System.Exception innerEx)
                        {
                            ed.WriteMessage($"\n处理单行文字事务时发生错误: {innerEx.Message}");
                            ed.WriteMessage($"\n内部异常堆栈跟踪: {innerEx.StackTrace}");
                            trans.Abort();
                            throw;
                        }
                    }
                }
                else
                {
                    ed.WriteMessage("\n在源图层上没有找到单行文字对象。");
                }

                // 然后处理 MText
                if (mTextsToTranslate.Count > 0)
                {
                    ed.WriteMessage("\n开始翻译多行文字...");
                    Dictionary<string, string> translatedMTexts = await openAIService.BatchTranslateAsync(mTextsToTranslate);

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

                            int processedMTextCount = 0;
                            foreach (var pair in mTextContents)
                            {
                                ObjectId objectId = pair.Key;
                                string originalText = pair.Value;
                                string translatedText;
                                if (!translatedMTexts.TryGetValue(originalText, out translatedText))
                                {
                                    translatedText = originalText; // 翻译失败时使用原文替代
                                }

                                Entity ent = trans.GetObject(objectId, OpenMode.ForRead) as Entity;
                                if (ent is MText)
                                {
                                    MText mtext = ent as MText;
                                    processedMTextCount += ProcessMText(trans, mtext, targetLayerId, btr, translatedText); // 调用新的 ProcessMText 方法
                                }
                            }
                            ed.WriteMessage($"\n处理并翻译了 {processedMTextCount} 个多行文字对象。");
                            trans.Commit(); // 提交 MText 的事务
                        }
                        catch (System.Exception innerEx)
                        {
                            ed.WriteMessage($"\n处理多行文字事务时发生错误: {innerEx.Message}");
                            ed.WriteMessage($"\n内部异常堆栈跟踪: {innerEx.StackTrace}");
                            trans.Abort();
                            throw;
                        }
                    }
                }
                else
                {
                    ed.WriteMessage("\n在源图层上没有找到多行文字对象。");
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

        // Process a MText entity
        private int ProcessMText(
            Transaction trans,
            MText mtext,
            ObjectId targetLayerId,
            BlockTableRecord btr,
            string translatedText)
        {
            try
            {
                // 1. 获取 MText 的 Bounding Box
                Extents3d extents = mtext.GeometricExtents; // 使用 GeometricExtents 属性
                double mtextHeight = extents.MaxPoint.Y - extents.MinPoint.Y; // 计算高度，近似值

                // 2. 创建 MText 的副本
                MText newMText = mtext.Clone() as MText;

                // 3. 使用 MText 的高度作为位移距离
                double offsetInUnits = mtextHeight;

                // 4. 向下移动副本
                Vector3d moveVector = new Vector3d(0, -offsetInUnits, 0);
                Matrix3d moveMatrix = Matrix3d.Displacement(moveVector);
                newMText.TransformBy(moveMatrix);

                // 5. 设置图层为目标图层
                newMText.LayerId = targetLayerId;

                // 6. 更新文本内容为翻译后的文本
                newMText.Contents = translatedText; // 使用 Contents 属性

                // 7. 将新的 MText 添加到数据库
                btr.AppendEntity(newMText);
                trans.AddNewlyCreatedDBObject(newMText, true);

                return 1;
            }
            catch (System.Exception ex)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor
                    .WriteMessage($"\n处理 MText 时发生错误: {ex.Message}");
                return 0;
            }
        }
    }
}