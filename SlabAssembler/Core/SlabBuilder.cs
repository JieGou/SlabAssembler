using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urbbox.SlabAssembler.Repositories;

namespace Urbbox.SlabAssembler.Core
{
    public class SlabBuilder
    {
        protected AutoCadManager Acad;

        public SlabEspecifications Especifications { get; protected set; }

        public SlabBuilder(AutoCadManager acad)
        {
            this.Acad = acad;
        }

        public Orientation AskForOrientation()
        {
            var result = Acad.GetKeywords("\n Selecione uma orientação", new[] { "Vertical", "Horizontal" });
            if (result.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                return (result.StringResult == "Vertical") ? Orientation.Vertical : Orientation.Horizontal;
            else
                throw new OperationCanceledException();
        }

        public bool ValidateOutline(ObjectId objectId)
        {
            using (var t = Acad.StartOpenCloseTransaction())
            {
                var outline = t.GetObject(objectId, OpenMode.ForRead) as Polyline2d;
                return outline != null && outline.Closed;
            }
        }

        public ObjectId SelectOutline()
        {
            var result = Acad.SelectSingle("\nSelecione o contorno da laje");
            if (result.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
            {
                var selected = result.Value.GetObjectIds().First();
                if (ValidateOutline(selected))
                    return selected;
                else
                    throw new ArgumentException("Selecione um contorno válido");
            }
            else
                return ObjectId.Null;
        }

        public SlabBuildingResult Start()
        {
            if (Especifications.Orientation == null) Especifications.Orientation = AskForOrientation();
            if (Especifications.Outline == ObjectId.Null) Especifications.Outline = SelectOutline();
            var result = new SlabBuildingResult();

            System.Diagnostics.Debug.Print("Build Started!");

            return result;
        }

        public IEnumerable<string> GetLayers()
        {
            return Acad.GetLayers();
        }
     
    }
}
