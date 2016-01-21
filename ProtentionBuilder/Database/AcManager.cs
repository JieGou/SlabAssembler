using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace Urbbox.AutoCAD.ProtentionBuilder.Database
{
    public class AcManager
    {
        private Document WorkingDocument { get; set; }
        private Autodesk.AutoCAD.DatabaseServices.Database Database => WorkingDocument.Database;

        public AcManager(Document workingDocument)
        {
            WorkingDocument = workingDocument;
        }

        private Transaction StartOpenCloseTransaction()
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
    }
}
