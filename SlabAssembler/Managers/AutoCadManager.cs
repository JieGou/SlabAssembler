using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Urbbox.SlabAssembler.Managers
{
    public class AutoCadManager
    {
        public Document WorkingDocument { get; private set; }
        public Database Database => WorkingDocument.Database;
        public CoordinateSystem3d UCS => WorkingDocument.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d;

        public AutoCadManager()
        {
            WorkingDocument = AcApplication.DocumentManager.MdiActiveDocument;
        }

        public AutoCadManager(Document document)
        {
            WorkingDocument = document;
        }

        public Transaction StartOpenCloseTransaction()
        {
            return Database.TransactionManager.StartOpenCloseTransaction();
        }

        public Transaction StartTransaction()
        {
            return Database.TransactionManager.StartTransaction();
        }

        public List<string> GetLayers()
        {
            if (WorkingDocument == null)
            {
                return new List<string>();
            }

            var list = new List<string>();
            using (var t = StartOpenCloseTransaction())
            {
                var layerTable = t.GetObject(Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (layerTable == null)
                {
                    return list;
                }

                foreach (var layerId in layerTable)
                {
                    var layer = t.GetObject(layerId, OpenMode.ForRead) as LayerTableRecord;
                    if (layer != null && !layer.IsOff)
                    {
                        list.Add(layer.Name);
                    }
                }
            }

            return list;
        }

        public ObjectId FindObject(string blockName)
        {
            using (var t = StartOpenCloseTransaction())
            {
                var blktbl = t.GetObject(Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                return blktbl.Has(blockName) ? blktbl[blockName] : ObjectId.Null;
            }
        }

        public PromptPointResult GetPoint(string message)
        {
            using (WorkingDocument.LockDocument())
            {
                return WorkingDocument.Editor.GetPoint(message);
            }
        }

        /// <summary>
        /// 判断块参照是否存在
        /// </summary>
        /// <param name="blockName">块名称</param>
        /// <returns></returns>
        public bool CheckBlockExists(string blockName)
        {
            using (var t = StartOpenCloseTransaction())
            {
                var blockTable = t.GetObject(Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                return blockTable != null && blockTable.Has(blockName);
            }
        }

        /// <summary>
        /// 判断图层是否存在
        /// </summary>
        /// <param name="layerName">图层名称</param>
        /// <returns></returns>
        public bool CheckLayerExists(string layerName)
        {
            using (var t = StartOpenCloseTransaction())
            {
                var layerTable = t.GetObject(Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                return layerTable != null && layerTable.Has(layerName);
            }
        }

        public PromptEntityResult GetEntity(string message)
        {
            using (WorkingDocument.LockDocument())
            {
                return WorkingDocument.Editor.GetEntity(message);
            }
        }

        public PromptResult GetKeywords(string message, string[] keywords)
        {
            using (WorkingDocument.LockDocument())
            {
                var options = new PromptKeywordOptions(message);
                foreach (var k in keywords)
                {
                    options.Keywords.Add(k);
                }

                return WorkingDocument.Editor.GetKeywords(options);
            }
        }

        public ObjectIdCollection GetLayerObjects(string layerName)
        {
            var tvs = new[] { new TypedValue((int)DxfCode.LayerName, layerName) };
            var sf = new SelectionFilter(tvs);
            var psr = WorkingDocument.Editor.SelectAll(sf);

            if (psr.Status == PromptStatus.OK)
            {
                return new ObjectIdCollection(psr.Value.GetObjectIds());
            }
            else
            {
                return new ObjectIdCollection();
            }
        }

        public void WriteMessage(string message)
        {
            WorkingDocument.Editor.WriteMessage($"\n{message}");
        }
    }
}