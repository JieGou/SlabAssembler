using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using Urbbox.SlabAssembler.Managers;

namespace Urbbox.SlabAssembler.Core
{
    class BuildingHelper
    {
        protected AutoCadManager _acad;

        public BuildingHelper(AutoCadManager manager)
        {
            _acad = manager;
        }

        public Point3d GetStartPoint(ObjectId selectedOutline, bool specifyStartPoint)
        {
            if (specifyStartPoint)
            {
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
                var result = _acad.GetPoint("\nInforme o ponto de partida");
                if (result.Status == PromptStatus.OK)
                    return result.Value;
                else
                    throw new OperationCanceledException();
            }
            else
            {
                using (var t = _acad.StartTransaction())
                {
                    var outline = t.GetObject(selectedOutline, OpenMode.ForRead);
                    return outline.Bounds.Value.MinPoint;
                }

            }
        }

        public Point3d GetMaxPoint(ObjectId selectedOutline)
        {
            using (var t = _acad.StartOpenCloseTransaction())
            {
                var bounds = t.GetObject(selectedOutline, OpenMode.ForRead).Bounds.Value;
                return bounds.MaxPoint;
            }
        }

        public Orientation GetOrientation()
        {
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
            var result = _acad.GetKeywords("\nSelecione uma orientação", new[] { "Vertical", "Horizontal" });
            if (result.Status == PromptStatus.OK)
                return (result.StringResult == "Vertical") ? Orientation.Vertical : Orientation.Horizontal;
            else
                throw new OperationCanceledException();
        }

        public bool ValidateOutline(ObjectId objectId)
        {
            using (var t = _acad.StartOpenCloseTransaction())
            {
                var outline = t.GetObject(objectId, OpenMode.ForRead) as Polyline;
                return outline != null && outline.Bounds.HasValue;
            }
        }

        public ObjectId SelectOutline()
        {
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
            var result = _acad.GetEntity("\nSelecione o contorno da laje");
            if (result.Status == PromptStatus.OK)
            {
                var selected = result.ObjectId;
                if (ValidateOutline(selected))
                    return selected;
                else
                    throw new ArgumentException("\nSelecione um contorno válido.");
            }
            else
                return ObjectId.Null;
        }
    }
}
