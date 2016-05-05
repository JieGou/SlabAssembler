using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using System;

namespace Urbbox.SlabAssembler.Managers
{
    public class AutoCadManager
    {
        public Document WorkingDocument => AcApplication.DocumentManager.MdiActiveDocument;
        public Autodesk.AutoCAD.DatabaseServices.Database Database => WorkingDocument.Database;
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
                return blockTable.Has(blockName);
            }
        }

        public bool CheckLayerExists(string layerName)
        {
            using (var t = StartOpenCloseTransaction())
            {
                var layerTable = t.GetObject(Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                return layerTable.Has(layerName);
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
            TypedValue[] tvs = new TypedValue[1] {
                new TypedValue((int) DxfCode.LayerName, layerName)
            };
            SelectionFilter sf = new SelectionFilter(tvs);
            PromptSelectionResult psr = WorkingDocument.Editor.SelectAll(sf);

            if (psr.Status == PromptStatus.OK)
                return new ObjectIdCollection(psr.Value.GetObjectIds());
            else
                return new ObjectIdCollection();
        }

    }
}
