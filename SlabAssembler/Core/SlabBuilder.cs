using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using Urbbox.SlabAssembler.Repositories;
using Urbbox.SlabAssembler.ViewModels;

namespace Urbbox.SlabAssembler.Core
{
    public class SlabBuilder
    {
        protected AutoCadManager _acad;
        public EspecificationsViewModel EspecificationsViewModel {
            set
            {
                Especifications.Parts = value;
                Especifications.Parts.SelectOutline.Subscribe(x => {
                    try {
                        Especifications.Parts.SelectedOutline = SelectOutline();
                        Especifications.MaxPoint = GetMaxPoint();
                        Especifications.StartPoint = GetStartPoint();
                    } catch (ArgumentException ex) {
                        _acad.WorkingDocument.Editor.WriteMessage(ex.Message);
                    }
                });
                Especifications.Parts.DrawSlab.Subscribe(x => Start());
            }
        }

        public AlgorythimViewModel AlgorythimViewModel {
            set { Especifications.Algorythim = value; }
        }

        public SlabEspecifications Especifications { get; protected set; }
        protected ConfigurationsRepository _config;
        protected Polyline _outline;
        protected DBObjectCollection _girders;
        protected DBObjectCollection _collumns;
        protected DBObjectCollection _empties;

        public SlabBuilder(AutoCadManager acad, ConfigurationsRepository config)
        {
            _acad = acad;
            _config = config;
            Especifications = new SlabEspecifications();
            _girders = new DBObjectCollection();
            _collumns = new DBObjectCollection();
            _empties = new DBObjectCollection();
        }

        public Point3d GetStartPoint()
        {
            if (Especifications.Parts.SpecifyStartPoint)
            {
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
                var result = _acad.GetPoint("\nInforme o ponto de partida");
                if (result.Status == PromptStatus.OK)
                    return result.Value.Add(Especifications.StartPointDeslocation);
                else
                    throw new OperationCanceledException();
            } else
            { 
                using (var t = _acad.StartOpenCloseTransaction())
                {
                    var outline = t.GetObject(Especifications.Parts.SelectedOutline, OpenMode.ForRead) as Polyline;
                    return outline.Bounds.Value.MinPoint.Add(Especifications.StartPointDeslocation);
                }
            }
        }

        public Point3d GetMaxPoint()
        {
            using (var t = _acad.StartOpenCloseTransaction())
            {
                var bounds = t.GetObject(Especifications.Parts.SelectedOutline, OpenMode.ForRead).Bounds.Value;
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

        private void InitializeBuilding()
        {
            if (Especifications.Parts.SelectedOutline == ObjectId.Null)
                Especifications.Parts.SelectedOutline = SelectOutline();
            if (Especifications.StartPoint == null)
                Especifications.StartPoint = GetStartPoint();
        }

        public SlabBuildingResult Start()
        {
            InitializeBuilding();
            var algorythim = new SlabAlgorythim(Especifications);
            var result = new SlabBuildingResult();

            using (_acad.WorkingDocument.LockDocument())
            {
                SelectedCollisionObjects();
                try {
                    BuildLp(result, algorythim);
                    BuildLd(result, algorythim);
                    if (!Especifications.Algorythim.OnlyCimbrament)
                        BuildCast(result, algorythim);
                }
                catch (OperationCanceledException) {
                    _acad.WorkingDocument.Editor.WriteMessage("\nLaje cancelada.");
                }
                catch (Exception e)
                {
                    _acad.WorkingDocument.Editor.WriteMessage($"\n{e.Message}\n {e.StackTrace}");
                }
            }

            return result;
        }

        private void SelectedCollisionObjects()
        {
            using (var t = _acad.StartOpenCloseTransaction())
            {
                _outline = t.GetObject(Especifications.Parts.SelectedOutline, OpenMode.ForRead) as Polyline;

                _girders.Clear();
                foreach (ObjectId o in _acad.GetLayerObjects(Especifications.Parts.SelectedGirdersLayer))
                {
                    var girder = t.GetObject(o, OpenMode.ForRead) as Entity;
                    if (girder != null && girder.Bounds.HasValue)
                        _girders.Add(girder);
                }

                _collumns.Clear();
                foreach (ObjectId o in _acad.GetLayerObjects(Especifications.Parts.SelectedColumnsLayer))
                {
                    var collumn = t.GetObject(o, OpenMode.ForRead) as Entity;
                    if (collumn != null && collumn.Bounds.HasValue)
                        _collumns.Add(collumn);
                }

                _empties.Clear();
                foreach (ObjectId o in _acad.GetLayerObjects(Especifications.Parts.SelectedEmptiesLayer))
                {
                    var emptyOutline = t.GetObject(o, OpenMode.ForRead) as Polyline;
                    if (emptyOutline != null && emptyOutline.Bounds.HasValue)
                        _empties.Add(emptyOutline);
                }
            }
        }

        public Point3dCollection PlaceMultipleParts(SlabBuildingResult result, Point3dCollection locations, Part part, double orientationAngle)
        {
            Point3dCollection collisions = new Point3dCollection();
            ObjectId partOutline = CreatePartOutline(part);

            foreach (Point3d loc in locations)
            {
                if (CanPlacePart(loc, part, orientationAngle, partOutline))
                {
                    var blockId = GetOrCreatePart(part);
                    PlacePart(part, loc, orientationAngle, blockId);

                    result.CountNewPart(part);
                }
                else collisions.Add(loc);
            }

            _acad.WorkingDocument.Editor.UpdateScreen();
            return collisions;
        }

        private ObjectId CreatePartOutline(Part part)
        {
            var outlinePartId = ObjectId.Null;

            using (var t = _acad.StartTransaction())
            {
                var blkTbl = t.GetObject(_acad.Database.BlockTableId, OpenMode.ForWrite) as BlockTable;
                if (blkTbl.Has(part.OutlineReferenceName)) return blkTbl[part.OutlineReferenceName];

                var border = Especifications.Algorythim.OutlineDistance - 0.001;
                using (var record = new BlockTableRecord())
                {
                    record.Name = part.OutlineReferenceName;
                    record.Units = UnitsValue.Centimeters;
                    record.Origin = part.PivotPoint;
                    outlinePartId = blkTbl.Add(record);
                    t.AddNewlyCreatedDBObject(record, true);

                    using (var poly = SlabAlgorythim.CreateSquare(part, border))
                    {
                        record.AppendEntity(poly);
                        t.AddNewlyCreatedDBObject(poly, true);
                    }

                    foreach (var line in SlabAlgorythim.CreateCrossLines(part, border))
                    { 
                        record.AppendEntity(line);
                        t.AddNewlyCreatedDBObject(line, true);
                        line.Dispose();
                    }
                }

                t.Commit();
            }
            
            return outlinePartId;
        }

        private ObjectId PlacePart(Part part, Point3d loc, double orientationAngle, ObjectId blockId)
        {
            var referenceId = ObjectId.Null;

            using (var t = _acad.StartTransaction())
            {
                BlockTable blkTbl = t.GetObject(_acad.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelspace = t.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (var blkRef = new BlockReference(loc, blockId))
                {
                    FixPartOrientation(part, orientationAngle, loc, blkRef);

                    referenceId = modelspace.AppendEntity(blkRef);
                    t.AddNewlyCreatedDBObject(blkRef, true);
                }

                t.Commit();
            }

            _acad.WorkingDocument.Editor.UpdateScreen();
            return referenceId;
        }

        private void FixPartOrientation(Part part, double orientationAngle, Point3d loc, BlockReference blkRef)
        {
            using (var t = _acad.StartTransaction())
            {
                var angle = GetFixedRotationAngle(blkRef, orientationAngle);
                if (angle != 0) { 
                    blkRef.TransformBy(Matrix3d.Rotation(angle, _acad.UCS.Zaxis, loc));
                    var vectorPivot = (part.PivotPoint - Point3d.Origin)
                        .Add(new Vector3d(part.Height - part.PivotPointY, -part.PivotPointY, 0));
                    //.Add(new Vector3d(0, Especifications.Algorythim.DistanceBetweenLpAndLd, 0))
                    //.TransformBy(Matrix3d.Rotation(-orientationAngle, _acad.UCS.Zaxis, loc));
                    blkRef.Position = blkRef.Position.Add(vectorPivot);
                } else 
                    blkRef.Position = blkRef.Position.Add(part.PivotPoint - Point3d.Origin);

                t.Commit();
            }
        }

        private double GetFixedRotationAngle(BlockReference entity, double orientationAngle)
        {
            var entityWidth = entity.Bounds.Value.MaxPoint.X - entity.Bounds.Value.MinPoint.X;
            var entityHeight = entity.Bounds.Value.MaxPoint.Y - entity.Bounds.Value.MinPoint.Y;
            var currentAngle = entityWidth >= entityHeight ? 0 : 90;

            return ((orientationAngle - currentAngle) * Math.PI) / 180D;
        }

        public ObjectId GetOrCreatePart(Part part)
        {
            ObjectId partObjectId = ObjectId.Null;

            using (var t = _acad.StartTransaction())
            {
                BlockTable blkTbl = t.GetObject(_acad.Database.BlockTableId, OpenMode.ForWrite) as BlockTable;
                BlockTableRecord modelspace = t.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                if (!blkTbl.Has(part.ReferenceName))
                {
                    if (blkTbl.Has(part.GenericReferenceName)) return blkTbl[part.GenericReferenceName];

                    using (var record = new BlockTableRecord())
                    {
                        record.Name = part.GenericReferenceName;
                        record.Units = UnitsValue.Centimeters;
                        record.Origin = part.PivotPoint;
                        partObjectId = blkTbl.Add(record);
                        t.AddNewlyCreatedDBObject(record, true);

                        using (var poly = SlabAlgorythim.CreateSquare(part, 0))
                        {
                            record.AppendEntity(poly);
                            t.AddNewlyCreatedDBObject(poly, true);
                        }
                    }
                } else
                    return blkTbl[part.ReferenceName];

                t.Commit();
            }

            return partObjectId;
        }

        public void BuildCast(SlabBuildingResult result, SlabAlgorythim algorythim)
        {
            PlaceMultipleParts(result, algorythim.GetCastPointList(), Especifications.Parts.SelectedCast, Especifications.Algorythim.OrientationAngle - 90);
        }

        public void BuildLp(SlabBuildingResult result, SlabAlgorythim algorythim)
        {
            var dangerZoneList = new Point3dCollection();
            var points = algorythim.GetLpPointList();
            var lastPt = points[points.Count];
            var firstPt = points[0];
            var part = Especifications.Parts.SelectedLp;
            var orientationAngle = Especifications.Algorythim.OrientationAngle;

            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];
                if (!SlabAlgorythim.IsInsidePolygon(_outline, p))
                {
                    if (algorythim.isAtTheEnd(lastPt, p))
                    {
                        var b = GetBelowLPIndex(points[0], points[points.Count], i);
                        if (b > 0 && b < points.Count && SlabAlgorythim.IsInsidePolygon(_outline, points[b]))
                        {
                            dangerZoneList.Add(points[b]);
                            points.RemoveAt(b);
                            points.RemoveAt(i);
                        }
                    }
                }
            }

            do
                points = PlaceMultipleParts(result, points, part, orientationAngle);
            while (points.Count > 0 && (part = _config.GetNextSmallerPart(part)) != null);

            while (dangerZoneList.Count > 0)
                dangerZoneList = BuildDangerZoneLp(result, dangerZoneList, orientationAngle);
        }

        private Point3dCollection BuildDangerZoneLp(SlabBuildingResult result, Point3dCollection points, double orientationAngle)
        {
            foreach (Point3d p in points)
            {
                var direction = SlabAlgorythim.VectorFrom(p, orientationAngle);
                var collision = LineCast(p, direction, Especifications.Parts.SelectedLp.Width * 2);
                if (collision.HasValue)
                {
                    //TODO
                }
            }

            return new Point3dCollection();
        }

        public void BuildLd(SlabBuildingResult result, SlabAlgorythim algorythim)
        {
            var points = algorythim.GetLdPointList();
            var part = Especifications.Parts.SelectedLd;

            do
                points = PlaceMultipleParts(result, points, part, Especifications.Algorythim.OrientationAngle - 90);
            while (points.Count > 0 && (part = _config.GetNextSmallerPart(part)) != null);
        }

        public Point3d? LineCast(Point3d startPoint, Vector3d direction, float distance)
        {
            var intersections = new Point3dCollection();

            using (var line = new Line(startPoint, startPoint.Add(direction * distance)))
            {
                _outline.IntersectWith(line, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);

                foreach (DBObject o in _girders)
                    line.IntersectWith(o as Entity, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);

                foreach (DBObject o in _collumns)
                    line.IntersectWith(o as Entity, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);

                foreach (DBObject o in _empties)
                    line.IntersectWith(o as Entity, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
            }

            double smallestDistance = Double.MaxValue;
            Point3d? nearestPoint = null;
            foreach (Point3d p in intersections)
            {
                var dist = startPoint.DistanceTo(p);
                if (dist < smallestDistance) {
                    smallestDistance = dist;
                    nearestPoint = p;
                }
            }

            return nearestPoint;
        }

        private int GetBelowLDIndex(Point3d startPoint, Point3d lastPoint, int currentIndex)
        {
            var orientation = Especifications.Algorythim.SelectedOrientation;
            var distanceBetweenLd = Especifications.Algorythim.DistanceBetweenLpAndLd * 2 + Especifications.Parts.SelectedLp.Height;

            double sizeWidth = (orientation == Orientation.Vertical) ? lastPoint.Y - startPoint.Y : lastPoint.X - startPoint.X;
            int width = ((int)Math.Floor(sizeWidth / distanceBetweenLd)) + 1;
            int x = SlabAlgorythim.getXCoordOfElementAt(currentIndex, width);
            int y = SlabAlgorythim.getYCoordOfElementAt(currentIndex, width);
            if (orientation == Orientation.Vertical)
                return SlabAlgorythim.getElementNumberAt(x - 1, y, width);
            else
                return SlabAlgorythim.getElementNumberAt(x, y - 1, width);
        }

        private int GetBelowLPIndex(Point3d startPoint, Point3d lastPoint, int currentIndex)
        {
            var orientation = Especifications.Algorythim.SelectedOrientation;

            double sizeWidth = (orientation == Orientation.Vertical) ? lastPoint.X - startPoint.X : lastPoint.Y - startPoint.Y;
            int width = ((int) Math.Floor(sizeWidth / Especifications.Algorythim.DistanceBetweenLp)) + 1;
            int x = SlabAlgorythim.getXCoordOfElementAt(currentIndex, width);
            int y = SlabAlgorythim.getYCoordOfElementAt(currentIndex, width);
            if (orientation == Orientation.Vertical)
                return SlabAlgorythim.getElementNumberAt(x, y - 1, width);
            else
                return SlabAlgorythim.getElementNumberAt(x - 1, y, width);
        }

        private bool CanPlacePart(Point3d loc, Part part, double orientationAngle, ObjectId outlinePartId)
        {
            var isInside = SlabAlgorythim.IsInsidePolygon(_outline, loc);
            if (!isInside) return false;
            
            using (var t = _acad.StartTransaction())
            {
                var partOutlineRefId = PlacePart(part, loc, orientationAngle, outlinePartId);
                var partOutlineRef = t.GetObject(partOutlineRefId, OpenMode.ForRead) as BlockReference;

                var intersections = new Point3dCollection();
                _outline.IntersectWith(partOutlineRef, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                if (intersections.Count > 0) return false;

                foreach (DBObject o in _girders)
                {
                    partOutlineRef.IntersectWith(o as Entity, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                    if (intersections.Count > 0) return false;
                }

                foreach (DBObject o in _collumns)
                {
                    partOutlineRef.IntersectWith(o as Entity, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                    if (intersections.Count > 0) return false;
                }

                foreach (DBObject o in _empties)
                {
                    partOutlineRef.IntersectWith(o as Entity, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                    if (intersections.Count > 0) return false;

                    if (SlabAlgorythim.IsInsidePolygon(o as Polyline, loc)) return false;
                }

                partOutlineRef.Erase();
                t.Commit();
            }

            return true;
        }

    }
}
