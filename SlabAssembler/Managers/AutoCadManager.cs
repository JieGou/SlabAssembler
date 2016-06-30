using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Urbbox.SlabAssembler.Managers
{
    public class AutoCadManager
    {
        public Document WorkingDocument => AcApplication.DocumentManager.MdiActiveDocument;
        public Database Database => WorkingDocument.Database;
        public CoordinateSystem3d UCS => WorkingDocument.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d;

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
            if (WorkingDocument == null) return new List<string>();

            var list = new List<string>();
            using (var t = StartOpenCloseTransaction())
            {
                var layerTable = t.GetObject(Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (layerTable == null) return list;
                foreach (var layerId in layerTable)
                {
                    var layer = t.GetObject(layerId, OpenMode.ForRead) as LayerTableRecord; 
                    if (layer != null && !layer.IsOff) list.Add(layer.Name);
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

        public bool CheckBlockExists(string blockName)
        {
            using (var t = StartOpenCloseTransaction())
            {
                var blockTable = t.GetObject(Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                return blockTable != null && blockTable.Has(blockName);
            }
        }

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
                    options.Keywords.Add(k);
                return WorkingDocument.Editor.GetKeywords(options);
            }
        }

        public ObjectIdCollection GetLayerObjects(string layerName)
        {
            var tvs = new[] { new TypedValue((int) DxfCode.LayerName, layerName) };
            var sf = new SelectionFilter(tvs);
            var psr = WorkingDocument.Editor.SelectAll(sf);

            if (psr.Status == PromptStatus.OK)
                return new ObjectIdCollection(psr.Value.GetObjectIds());
            else
                return new ObjectIdCollection();
        }

    }
}
