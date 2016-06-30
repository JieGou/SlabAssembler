using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using Autodesk.AutoCAD.Geometry;
using Urbbox.SlabAssembler.Managers;

namespace Urbbox.SlabAssembler.Core
{
    class BuildingProcessHelper
    {
        private readonly AutoCadManager _acad;

        public BuildingProcessHelper()
        {
            _acad = new AutoCadManager();
        }

        public Point3d GetStartPoint(ObjectId selectedOutline, bool specifyStartPoint)
        {
            if (specifyStartPoint)
            {
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
                var result = _acad.GetPoint("\nInforme o ponto de partida");
                if (result.Status == PromptStatus.OK)
                    return result.Value;

                throw new OperationCanceledException();
            }

            using (var t = _acad.StartTransaction())
            {
                var outline = t.GetObject(selectedOutline, OpenMode.ForRead);
                if (outline.Bounds != null) return outline.Bounds.Value.MinPoint;
            }

            throw new InvalidOperationException("Não foi possível descobrir o ponto de partida do contorno selecionada.");
        }

        public Point3d GetMaxPoint(ObjectId selectedOutline)
        {
            using (var t = _acad.StartOpenCloseTransaction())
            {
                var extents3D = t.GetObject(selectedOutline, OpenMode.ForRead, true, true).Bounds;
                if (extents3D != null)
                {
                    var bounds = extents3D.Value;
                    return bounds.MaxPoint;
                }

                throw new InvalidOperationException("Não foi possível obter o ponto máximo do contorno.");
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
                return outline?.Bounds != null;
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

                throw new ArgumentException("\nSelecione um contorno válido.");
            }

            return ObjectId.Null;
        }
    }
}
