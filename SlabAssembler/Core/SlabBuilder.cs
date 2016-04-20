using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using Urbbox.SlabAssembler.Repositories;
using Urbbox.SlabAssembler.ViewModels;
using System.Linq;
using Urbbox.SlabAssembler.Core.Variations;
using Autodesk.AutoCAD.Colors;

namespace Urbbox.SlabAssembler.Core
{
    public class SlabBuilder
    {
        protected AutoCadManager _acad;
        protected ConfigurationsRepository _config;
        protected Polyline _outline;
        protected DBObjectCollection _girders;
        protected DBObjectCollection _collumns;
        protected DBObjectCollection _empties;

        public SlabBuilder(AutoCadManager acad, ConfigurationsRepository config)
        {
            _acad = acad;
            _config = config;
            _girders = new DBObjectCollection();
            _collumns = new DBObjectCollection();
            _empties = new DBObjectCollection();
        }

        #region Initializers
        public Point3d GetStartPoint(SlabProperties prop)
        {
            if (prop.Parts.SpecifyStartPoint)
            {
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
                var result = _acad.GetPoint("\nInforme o ponto de partida");
                if (result.Status == PromptStatus.OK)
                    return result.Value.Add(prop.StartPointDeslocation);
                else
                    throw new OperationCanceledException();
            } else {
                using (var t = _acad.StartTransaction())
                {
                    var outline = t.GetObject(prop.Parts.SelectedOutline, OpenMode.ForRead);
                    return outline.Bounds.Value.MinPoint.Add(prop.StartPointDeslocation);
                }
                    
            }
        }

        public Point3d GetMaxPoint(SlabProperties prop)
        {
            using (var t = _acad.StartOpenCloseTransaction())
            {
                var bounds = t.GetObject(prop.Parts.SelectedOutline, OpenMode.ForRead).Bounds.Value;
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

        private void InitializeBuilding(SlabProperties prop)
        {
            SelectedCollisionObjects(prop);

            if (prop.Parts.SelectedOutline == ObjectId.Null)
                prop.Parts.SelectedOutline = SelectOutline();

            if (prop.StartPoint == null)
                prop.StartPoint = GetStartPoint(prop);
        }

        private void SelectedCollisionObjects(SlabProperties prop)
        {
            using (var t = _acad.StartOpenCloseTransaction())
            {
                _outline = t.GetObject(prop.Parts.SelectedOutline, OpenMode.ForRead) as Polyline;

                _girders.Clear();
                foreach (ObjectId o in _acad.GetLayerObjects(prop.Parts.SelectedGirdersLayer))
                {
                    var girder = t.GetObject(o, OpenMode.ForRead) as Entity;
                    if (girder != null && girder.Bounds.HasValue)
                        _girders.Add(girder);
                }

                _collumns.Clear();
                foreach (ObjectId o in _acad.GetLayerObjects(prop.Parts.SelectedColumnsLayer))
                {
                    var collumn = t.GetObject(o, OpenMode.ForRead) as Entity;
                    if (collumn != null && collumn.Bounds.HasValue)
                        _collumns.Add(collumn);
                }

                _empties.Clear();
                foreach (ObjectId o in _acad.GetLayerObjects(prop.Parts.SelectedEmptiesLayer))
                {
                    var emptyOutline = t.GetObject(o, OpenMode.ForRead) as Polyline;
                    if (emptyOutline != null && emptyOutline.Bounds.HasValue)
                        _empties.Add(emptyOutline);
                }
            }
        }

        #endregion

        public void Start(SlabProperties prop)
        {
            InitializeBuilding(prop);
            var algorythim = new SlabAlgorythim(prop);

            using (_acad.WorkingDocument.LockDocument())
            {
                try {
                    if (prop.Algorythim.UseStartLp && prop.Algorythim.SelectedStartLp != null)
                        BuildStartLp(algorythim);

                    BuildLp(algorythim);

                    if (prop.Algorythim.UseLds)
                        BuildLds(algorythim);

                    BuildLd(algorythim);

                    BuildHead(algorythim);

                    if (!prop.Algorythim.OnlyCimbrament)
                        BuildCast(algorythim);
                }
                catch (OperationCanceledException) { _acad.WorkingDocument.Editor.WriteMessage("\nLaje cancelada."); }
                catch (Exception e) { _acad.WorkingDocument.Editor.WriteMessage($"\n{e.Message}\n {e.StackTrace}"); }

                _acad.WorkingDocument.Editor.WriteMessage("\nLaje finalizada.");

                ClearOutlineParts();
            }
        }

        private void ClearOutlineParts()
        {
            using (var t = _acad.StartTransaction())
            {
                var blkTbl = t.GetObject(_acad.Database.BlockTableId, OpenMode.ForWrite) as BlockTable;

                foreach (var p in _config.Data.Parts)
                {
                    if (blkTbl.Has(p.OutlineReferenceName))
                    {
                        var record = t.GetObject(blkTbl[p.OutlineReferenceName], OpenMode.ForWrite) as BlockTableRecord;
                        record.Erase();
                    }
                }

                t.Commit();
            }
        }


        public Point3dCollection PlaceMultipleParts(SlabProperties prop, Point3dCollection locations, Part part, double orientationAngle, ObjectIdCollection placedObjects)
        {
            Point3dCollection collisions = new Point3dCollection();
            ObjectId partOutline = CreatePartOutline(prop, part);

            foreach (Point3d loc in locations)
            {
                if (CanPlacePart(loc, part, orientationAngle, partOutline))
                    placedObjects.Add(PlacePart(part, loc, orientationAngle, GetOrCreatePart(part)));
                else
                   collisions.Add(loc);
            }

            return collisions;
        }

        public Point3dCollection PlaceMultipleParts(SlabProperties prop, Point3dCollection locations, Part part, double orientationAngle)
        {
            var placedObjects = new ObjectIdCollection();
            return PlaceMultipleParts(prop, locations, part, orientationAngle, placedObjects);
        }

        private ObjectId PlacePart(Part part, Point3d loc, double orientationAngle, ObjectId blockId)
        {
            var referenceId = ObjectId.Null;

            using (var t = _acad.StartTransaction())
            {
                BlockTable blkTbl = t.GetObject(_acad.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                LayerTable layerTbl = t.GetObject(_acad.Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                BlockTableRecord modelspace = t.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (var blkRef = new BlockReference(loc, blockId))
                {
                    blkRef.Layer = (layerTbl.Has(part.Layer))? part.Layer : "0";
                    FixPartOrientation(part, orientationAngle, loc, blkRef);

                    referenceId = modelspace.AppendEntity(blkRef);
                    t.AddNewlyCreatedDBObject(blkRef, true);
                }

                t.Commit();
            }

            _acad.WorkingDocument.Editor.UpdateScreen();
            return referenceId;
        }

        #region Builders
        public void BuildCast(SlabAlgorythim al)
        {
            PlaceMultipleParts(al.Properties, al.GetCastPointList(), al.Properties.Parts.SelectedCast, 90 - al.Properties.Algorythim.OrientationAngle);
        }

        public void BuildLp(SlabAlgorythim al)
        {
            var points = al.GetLpPointList(al.Properties.Algorythim.SelectedStartLp != null);
            if (points.Count == 0) return;

            var dangerZoneList = new Point3dCollection();
            var normalZoneList = new Point3dCollection();
            var lastPt = points[points.Count - 1];
            var firstPt = points[0];
            var part = al.Properties.Parts.SelectedLp;
            var orientationAngle = al.Properties.Algorythim.OrientationAngle;

            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];
                normalZoneList.Add(p);
              
                if (al.IsAtTheEnd(lastPt, p) || !SlabAlgorythim.IsInsidePolygon(_outline, p))
                {
                    var b = al.GetBelowLpPoint(points, p);
                    if (b.HasValue && SlabAlgorythim.IsInsidePolygon(_outline, b.Value))
                    {
                        dangerZoneList.Add(b.Value);
                        normalZoneList.Remove(b.Value);
                        normalZoneList.Remove(p);
                    }
                }
            }

            do
                normalZoneList = PlaceMultipleParts(al.Properties, normalZoneList, part, orientationAngle);
            while (normalZoneList.Count > 0 && (part = _config.GetNextSmallerPart(part)) != null);

            if (dangerZoneList.Count > 0)
                BuildDangerZoneLp(al, dangerZoneList, orientationAngle);
        }
       
        private void BuildDangerZoneLp(SlabAlgorythim al, Point3dCollection points, double orientationAngle)
        {
            foreach (Point3d p in points)
            {
                var collisionPt = LineCast(p, orientationAngle, al.Properties.Parts.SelectedLp.Width * 2);
                if (collisionPt.HasValue)
                {
                    var dist = collisionPt.Value.DistanceTo(p);
                    Part firstLp, secondLp;
                    ObjectId firtLpOutline, secondLpOutline;
                    FindBetterLpCombination(al, dist, out firstLp, out secondLp);

                    if (firstLp != null)
                    {
                        firtLpOutline = CreatePartOutline(al.Properties, firstLp);
                        if (CanPlacePart(p, firstLp, orientationAngle, firtLpOutline))
                            PlacePart(firstLp, p, orientationAngle, GetOrCreatePart(firstLp));

                        var lpDirection = SlabAlgorythim.VectorFrom(orientationAngle);
                        var nextPoint = p.Add(lpDirection * (firstLp.Width + al.Properties.Algorythim.DistanceBetweenLp));
                        if (secondLp != null)
                        {
                            secondLpOutline = CreatePartOutline(al.Properties, secondLp);
                            if (CanPlacePart(nextPoint, secondLp, orientationAngle, secondLpOutline))
                                PlacePart(firstLp, nextPoint, orientationAngle, GetOrCreatePart(secondLp));
                        }
                    }
                }
            }
        }

        public void BuildStartLp(SlabAlgorythim al)
        {
            var points = al.GetStartLpPointList();
            var part = al.Properties.Algorythim.SelectedStartLp;
            var orientationAngle = al.Properties.Algorythim.OrientationAngle;
            var placedObjects = new ObjectIdCollection();
            var altPlacedObjects = new ObjectIdCollection();
            var lastPoints = new Point3dCollection();

            do { 
                points = PlaceMultipleParts(al.Properties, points, part, orientationAngle, placedObjects);
                if (points.Count > 0) lastPoints = points;
            } while (points.Count > 0 && (part = _config.GetNextSmallerPart(part)) != null);

            BuildAlternativeZoneStartLp(al.Properties, lastPoints, orientationAngle, altPlacedObjects);

            foreach (ObjectId altId in altPlacedObjects)
                EraseIfColliding(altId, placedObjects);
        }

        private void BuildAlternativeZoneStartLp(SlabProperties prop, Point3dCollection points, double orientationAngle, ObjectIdCollection placedObjects)
        {
            var alternativePoints = new Point3dCollection();
            var direction = SlabAlgorythim.VectorFrom(orientationAngle);
            var lastPart = prop.Algorythim.SelectedStartLp;
            var part = _config.GetNextSmallerPart(lastPart);

            do
            {
                foreach (Point3d point in points)
                {
                    var desloc = direction * (lastPart.Width - part.Width);
                    alternativePoints.Add(point.Add(desloc));
                }

                points = PlaceMultipleParts(prop, points, part, orientationAngle, placedObjects);
                lastPart = part;
            } while (points.Count > 0 && (part = _config.GetNextSmallerPart(part)) != null);
        }

        public void BuildLd(SlabAlgorythim al)
        {
            var points = al.GetLdPointList(al.Properties.Algorythim.UseLds);
            if (points.Count == 0) return;

            var part = al.Properties.Parts.SelectedLd;
            var orientationAngle = 90 - al.Properties.Algorythim.OrientationAngle;
            var directionVector = SlabAlgorythim.VectorFrom(orientationAngle);
            var lastPoints = new Point3dCollection();
            var placedObjects = new ObjectIdCollection();
            var altPlacedObjects = new ObjectIdCollection();

            do
            {
                points = PlaceMultipleParts(al.Properties, points, part, orientationAngle, placedObjects);
                if (points.Count > 0) lastPoints = points;
            } while (points.Count > 0 && (part = _config.GetNextSmallerPart(part)) != null);

            BuildAlternativeZoneLd(al.Properties, lastPoints, orientationAngle, false, altPlacedObjects);

            foreach (ObjectId altId in altPlacedObjects)
                EraseIfColliding(altId, placedObjects);
        }

        private void BuildAlternativeZoneLd(SlabProperties prop, Point3dCollection points, double orientationAngle, bool isLds, ObjectIdCollection placedObjects)
        {
            var alternativePoints = new Point3dCollection();
            var direction = SlabAlgorythim.VectorFrom(orientationAngle);
            var lastPart = prop.Parts.SelectedLd;
            var part = _config.GetNextSmallerPart(lastPart, (isLds)? UsageType.Lds : UsageType.Ld);

            do
            {
                foreach (Point3d point in points)
                {
                    var desloc = direction * (lastPart.Width - part.Width);
                    alternativePoints.Add(point.Add(desloc));
                }

                points = PlaceMultipleParts(prop, points, part, orientationAngle, placedObjects);
                lastPart = part;
            } while (points.Count > 0 && (part = _config.GetNextSmallerPart(part)) != null);
        }

        public void BuildLds(SlabAlgorythim al)
        {
            var points = al.GetLdsPointList();
            if (points.Count == 0) return;

            var lastPoints = new Point3dCollection();
            var part = _config.GetRespectiveOfUsageType(al.Properties.Parts.SelectedLd, UsageType.Lds);
            var rightPart = _config.GetNextSmallerPart(part);
            var orientationAngle = 90 - al.Properties.Algorythim.OrientationAngle;
            var directionVector = SlabAlgorythim.VectorFrom(orientationAngle);
            var placedObjects = new ObjectIdCollection();
            var altPlacedObjects = new ObjectIdCollection();

            do {
                points = PlaceMultipleParts(al.Properties, points, part, orientationAngle, placedObjects);
                if (points.Count > 0) lastPoints = points;
            } while (points.Count > 0 && (part = _config.GetNextSmallerPart(part)) != null);

            BuildAlternativeZoneLd(al.Properties, lastPoints, orientationAngle, true, altPlacedObjects);

            foreach (ObjectId altId in altPlacedObjects)
                EraseIfColliding(altId, placedObjects);
        }

        public void BuildHead(SlabAlgorythim al)
        {
            var part = _config.Data.Parts.Where(p => p.UsageType == UsageType.Head).First();
            PlaceMultipleParts(al.Properties, al.GetHeadPointList(part), part, 90 - al.Properties.Algorythim.OrientationAngle);
        }
        #endregion

        #region Debugging
        private void DebugPoints(Point3dCollection list, Color color, int size)
        {
            foreach (Point3d point in list)
            {
                DebugPoint(point, color, size);
            }
        }

        private void DebugPoint(Point3d point, Color color, int size)
        {
            Matrix3d curUCSMatrix = _acad.WorkingDocument.Editor.CurrentUserCoordinateSystem;
            CoordinateSystem3d curUCS = curUCSMatrix.CoordinateSystem3d;

            using (var t = _acad.StartTransaction())
            {
                BlockTable blockTable = t.GetObject(_acad.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelspace = t.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (Circle circle = new Circle(point, curUCS.Zaxis, size))
                {
                    circle.Layer = "0";
                    circle.Color = color;
                    modelspace.AppendEntity(circle);
                    t.AddNewlyCreatedDBObject(circle, true);
                }

                t.Commit();
            }
        }
        #endregion

        #region Helpers
        private ObjectId CreatePartOutline(SlabProperties prop, Part part)
        {
            var outlinePartId = ObjectId.Null;

            using (var t = _acad.StartTransaction())
            {
                var blkTbl = t.GetObject(_acad.Database.BlockTableId, OpenMode.ForWrite) as BlockTable;
                if (blkTbl.Has(part.OutlineReferenceName)) return blkTbl[part.OutlineReferenceName];

                var border = prop.Algorythim.OutlineDistance - 0.01;
                using (var record = new BlockTableRecord())
                {
                    record.Name = part.OutlineReferenceName;
                    record.Units = UnitsValue.Centimeters;
                    record.Explodable = true;
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
                        record.Explodable = true;
                        partObjectId = blkTbl.Add(record);
                        t.AddNewlyCreatedDBObject(record, true);

                        using (var poly = SlabAlgorythim.CreateSquare(part, 0))
                        {
                            poly.Color = part.UsageType.ToColor();

                            record.AppendEntity(poly);
                            t.AddNewlyCreatedDBObject(poly, true);
                        }
                    }
                }
                else
                    return blkTbl[part.ReferenceName];

                t.Commit();
            }

            return partObjectId;
        }

        private void FixPartOrientation(Part part, double orientationAngle, Point3d loc, BlockReference blkRef)
        {
            using (var t = _acad.StartTransaction())
            {
                var angle = GetFixedRotationAngle(blkRef, orientationAngle);
                blkRef.TransformBy(Matrix3d.Rotation(angle, _acad.UCS.Zaxis, loc));
                var vectorPivot = SlabAlgorythim.RotatePoint(part.PivotPoint, -orientationAngle) - Point3d.Origin;
                blkRef.Position = blkRef.Position.Add(vectorPivot);

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

        private void EraseIfColliding(ObjectId objId, ObjectIdCollection colliders)
        {
            using (var t = _acad.StartTransaction())
            {
                var intersections = new Point3dCollection();
                var reference = t.GetObject(objId, OpenMode.ForWrite) as BlockReference;
                foreach (ObjectId colliderId in colliders)
                {
                    var collider = t.GetObject(colliderId, OpenMode.ForRead) as BlockReference;
                    reference.IntersectWith(collider, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);

                    if (intersections.Count > 0)
                    {
                        reference.Erase();
                        t.Commit();
                        t.Dispose();
                        return;
                    }
                }
            }
        }
        #endregion

        #region Testing Algorythims
        private void FindBetterLpCombination(SlabAlgorythim al, double dist, out Part firstLp, out Part secondLp)
        {
            var secondUsageType = (al.Properties.Algorythim.UseEndLp) ? Variations.UsageType.EndLp : Variations.UsageType.Lp;
            var firstList = _config.Data.Parts.Where(p => p.UsageType == Variations.UsageType.Lp);
            var secondList = _config.Data.Parts.Where(p => p.UsageType == secondUsageType);
            al.FindBetterPartCombination(firstList, secondList, dist, out firstLp, out secondLp);
        }

        protected Point3d? LineCast(Point3d startPoint, double angle, float distance)
        {
            var intersections = new Point3dCollection();
            var direction = SlabAlgorythim.VectorFrom(angle);

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
                if (dist < smallestDistance)
                {
                    smallestDistance = dist;
                    nearestPoint = p;
                }
            }

            return nearestPoint;
        }

        protected bool CanPlacePart(Point3d loc, Part part, double orientationAngle, ObjectId outlinePartId)
        {
            var isInside = SlabAlgorythim.IsInsidePolygon(_outline, loc);
            if (!isInside) return false;

            if (part.UsageType == UsageType.Head) return true;
            
            using (var t = _acad.StartTransaction())
            {
                var partOutlineRefId = PlacePart(part, loc, orientationAngle, outlinePartId);
                var partOutlineRef = t.GetObject(partOutlineRefId, OpenMode.ForRead) as BlockReference;

                var intersections = new Point3dCollection();
                _outline.IntersectWith(partOutlineRef, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                if (intersections.Count > 0) return false;

                foreach (Entity e in _girders)
                {
                    e.BoundingBoxIntersectWith(partOutlineRef, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                    if (intersections.Count > 0) return false;
                }

                foreach (Entity e in _collumns)
                {
                    e.IntersectWith(partOutlineRef, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                    if (intersections.Count > 0) return false;
                }

                foreach (Entity e in _empties)
                {
                    e.IntersectWith(partOutlineRef, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                    if (intersections.Count > 0) return false;

                    if (SlabAlgorythim.IsInsidePolygon(e as Polyline, loc)) return false;
                }

                partOutlineRef.Erase();
                t.Commit();
            }

            return true;
        }
        #endregion

    }
}
