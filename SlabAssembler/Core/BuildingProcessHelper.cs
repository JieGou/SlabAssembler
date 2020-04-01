using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using Urbbox.SlabAssembler.Managers;

namespace Urbbox.SlabAssembler.Core
{
    internal class BuildingProcessHelper
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
                var result = _acad.GetPoint("\n选择起点:");
                if (result.Status == PromptStatus.OK)
                {
                    return result.Value;
                }

                throw new OperationCanceledException();
            }

            using (var t = _acad.StartTransaction())
            {
                var outline = t.GetObject(selectedOutline, OpenMode.ForRead);
                if (outline.Bounds != null)
                {
                    return outline.Bounds.Value.MinPoint;
                }
            }

            throw new InvalidOperationException("找不到所选轮廓的起点.");
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

                throw new InvalidOperationException("无法获得最大轮廓点.");
            }
        }

        public Orientation GetOrientation()
        {
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
            var result = _acad.GetKeywords("\n选择方向", new[] { "Vertical", "Horizontal" });
            if (result.Status == PromptStatus.OK)
            {
                return (result.StringResult == "Vertical") ? Orientation.Vertical : Orientation.Horizontal;
            }
            else
            {
                throw new OperationCanceledException();
            }
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
            var result = _acad.GetEntity("\n选择板轮廓");
            if (result.Status == PromptStatus.OK)
            {
                var selected = result.ObjectId;
                if (ValidateOutline(selected))
                {
                    return selected;
                }

                throw new ArgumentException("\n选择一个有效的轮廓.");
            }

            return ObjectId.Null;
        }
    }
}