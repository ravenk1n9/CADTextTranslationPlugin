// TextTranslationCommands.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System.IO;

namespace TextTranslationPlugin
{
    public class TextTranslationCommands
    {
        [CommandMethod("TRANSLATETEXT")]
        public async void TranslateText() 
        {
            ConfigReader.ReloadConfig();

            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc?.Editor; 
            if (doc == null || ed == null)
            {
                return;
            }

            OpenAIService openAIService = TextTranslationApp.OpenAIService;

            if (openAIService == null)
            {
                ed.WriteMessage("\n错误：OpenAI 服务未初始化。请检查插件配置并重新加载插件。");
                return;
            }

            string sourceLayer = ConfigReader.GetSourceLayer();
            ed.WriteMessage($"\n选择位于 '{sourceLayer}' 图层上的文字和多行文字对象进行翻译... (按 Enter 键完成选择)");

            PromptSelectionResult selRes = ed.GetSelection();
            if (selRes.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\n操作取消：未选择任何对象。");
                return;
            }

            SelectionSet ss = selRes.Value;
            if (ss == null || ss.Count == 0)
            {
                ed.WriteMessage("\n操作终止：选择集中没有对象。");
                return;
            }

            try
            {
                await ProcessTextObjectsAsync(doc, openAIService, ss);
                ed.WriteMessage("\n文字和多行文字翻译完成。");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\n翻译过程中发生严重错误: {ex.Message}");
                ed.WriteMessage($"\n详细信息: {ex.StackTrace}");
            }
        }

        private async Task ProcessTextObjectsAsync(Document doc, OpenAIService openAIService, SelectionSet ss)
        {
            Editor ed = doc.Editor;
            Database db = doc.Database;

            string sourceLayer = ConfigReader.GetSourceLayer();
            string targetLayer = ConfigReader.GetTargetLayer();

            try
            {
                List<string> dbTextsToTranslate = new List<string>();
                Dictionary<ObjectId, string> dbTextContents = new Dictionary<ObjectId, string>();

                List<string> mTextsToTranslate = new List<string>();
                Dictionary<ObjectId, string> mTextContents = new Dictionary<ObjectId, string>();

                using (doc.LockDocument())
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                    ObjectId targetLayerId;

                    if (!lt.Has(targetLayer))
                    {
                        lt.UpgradeOpen();
                        LayerTableRecord ltr = new LayerTableRecord();
                        ltr.Name = targetLayer;
                        targetLayerId = lt.Add(ltr);
                        trans.AddNewlyCreatedDBObject(ltr, true);
                    }
                    else
                    {
                        targetLayerId = lt[targetLayer];
                    }

                    foreach (SelectedObject selObj in ss)
                    {
                        Entity ent = trans.GetObject(selObj.ObjectId, OpenMode.ForRead) as Entity;
                        if (ent is DBText text && text.Layer == sourceLayer)
                        {
                            string content = text.TextString;
                            dbTextContents[selObj.ObjectId] = content;
                            dbTextsToTranslate.Add(content);
                        }
                        else if (ent is MText mtext && mtext.Layer == sourceLayer)
                        {
                            string content = mtext.Contents;
                            mTextContents[selObj.ObjectId] = content;
                            mTextsToTranslate.Add(content);
                        }
                    }
                    trans.Commit();
                }

                if (dbTextsToTranslate.Count > 0)
                {
                    ed.WriteMessage("\n开始翻译单行文字...");
                    Dictionary<string, string> translatedDBTexts = await openAIService.BatchTranslateAsync(dbTextsToTranslate);

                    using (doc.LockDocument())
                    using (Transaction trans = db.TransactionManager.StartTransaction())
                    {
                        try
                        {
                            LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                            ObjectId targetLayerId = lt[targetLayer];

                            BlockTableRecord btr = (BlockTableRecord)trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);

                            int processedDBTextCount = 0;
                            foreach (var pair in dbTextContents)
                            {
                                if (translatedDBTexts.TryGetValue(pair.Value, out string translatedText))
                                {
                                    DBText text = trans.GetObject(pair.Key, OpenMode.ForRead) as DBText;
                                    if (text != null)
                                    {
                                        processedDBTextCount += ProcessDBText(trans, text, targetLayerId, btr, translatedText);
                                    }
                                }
                            }
                            ed.WriteMessage($"\n处理并翻译了 {processedDBTextCount} 个单行文字对象。");
                            trans.Commit();
                        }
                        catch (System.Exception innerEx)
                        {
                            ed.WriteMessage($"\n处理单行文字事务时发生错误: {innerEx.Message}");
                            trans.Abort();
                            throw;
                        }
                    }
                }
                else
                {
                    ed.WriteMessage("\n在源图层上没有找到单行文字对象。");
                }

                if (mTextsToTranslate.Count > 0)
                {
                    ed.WriteMessage("\n开始翻译多行文字...");
                    Dictionary<string, string> translatedMTexts = await openAIService.BatchTranslateAsync(mTextsToTranslate);

                    using (doc.LockDocument())
                    using (Transaction trans = db.TransactionManager.StartTransaction())
                    {
                        try
                        {
                            LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                            ObjectId targetLayerId = lt[targetLayer];

                            BlockTableRecord btr = (BlockTableRecord)trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);

                            int processedMTextCount = 0;
                            foreach (var pair in mTextContents)
                            {
                                if (translatedMTexts.TryGetValue(pair.Value, out string translatedText))
                                {
                                    MText mtext = trans.GetObject(pair.Key, OpenMode.ForRead) as MText;
                                    if (mtext != null)
                                    {
                                        processedMTextCount += ProcessMText(trans, mtext, targetLayerId, btr, translatedText);
                                    }
                                }
                            }
                            ed.WriteMessage($"\n处理并翻译了 {processedMTextCount} 个多行文字对象。");
                            trans.Commit();
                        }
                        catch (System.Exception innerEx)
                        {
                            ed.WriteMessage($"\n处理多行文字事务时发生错误: {innerEx.Message}");
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

                ed.WriteMessage($"\n文字处理过程中发生错误: {ex.Message}");
            }
        }

        private int ProcessDBText(
            Transaction trans,
            DBText text,
            ObjectId targetLayerId,
            BlockTableRecord btr,
            string translatedText)
        {
            try
            {
                DBText newText = text.Clone() as DBText;
                double offsetInUnits = text.Height;
                Vector3d moveVector = new Vector3d(0, -offsetInUnits, 0);
                newText.TransformBy(Matrix3d.Displacement(moveVector));
                newText.LayerId = targetLayerId;
                newText.TextString = translatedText;
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

        private int ProcessMText(
            Transaction trans,
            MText mtext,
            ObjectId targetLayerId,
            BlockTableRecord btr,
            string translatedText)
        {
            try
            {
                MText newMText = mtext.Clone() as MText;
                Extents3d extents = mtext.GeometricExtents;
                double offsetInUnits = extents.MaxPoint.Y - extents.MinPoint.Y;
                Vector3d moveVector = new Vector3d(0, -offsetInUnits, 0);
                newMText.TransformBy(Matrix3d.Displacement(moveVector));
                newMText.LayerId = targetLayerId;
                newMText.Contents = translatedText;
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