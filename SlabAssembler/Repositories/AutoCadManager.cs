using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace Urbbox.SlabAssembler.Repositories
{
    public class AutoCadManager
    {
        public Document WorkingDocument { get; set; }
        public Autodesk.AutoCAD.DatabaseServices.Database Database => WorkingDocument.Database;

        public AutoCadManager(Document workingDocument)
        {
            WorkingDocument = workingDocument;
        }

        public Transaction StartOpenCloseTransaction()
        {
            return Database.TransactionManager.StartOpenCloseTransaction();
        }

        public List<string> GetLayers()
        {
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

        public PromptSelectionResult SelectSingle(string message)
        {
            using (WorkingDocument.LockDocument())
            {
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
                var options = new PromptSelectionOptions();
                options.MessageForAdding = message;
                options.RejectObjectsFromNonCurrentSpace = true;
                options.RejectObjectsOnLockedLayers = true;
                options.SingleOnly = true;
                return WorkingDocument.Editor.GetSelection(options);
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

    }
}
