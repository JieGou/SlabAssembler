using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using Urbbox.SlabAssembler.Repositories;
using System.Linq;
using Urbbox.SlabAssembler.Core.Variations;
using Autodesk.AutoCAD.GraphicsInterface;
using Urbbox.SlabAssembler.Core.Models;
using Urbbox.SlabAssembler.Managers;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;

namespace Urbbox.SlabAssembler.Core
{
    public class SlabBuilder : IDisposable
    {
        private AutoCadManager _acad;
        private readonly IPartRepository _partRepository;
        private Polyline _outline;
        private DBObjectCollection _girders;
        private DBObjectCollection _collumns;
        private DBObjectCollection _empties;

        public SlabBuilder(IPartRepository repo)
        {
            _partRepository = repo;
            _acad = new AutoCadManager();
            _girders = new DBObjectCollection();
            _collumns = new DBObjectCollection();
            _empties = new DBObjectCollection();
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
                    if (girder?.Bounds != null)
                        _girders.Add(girder);
                }

                _collumns.Clear();
                foreach (ObjectId o in _acad.GetLayerObjects(prop.Parts.SelectedColumnsLayer))
                {
                    var collumn = t.GetObject(o, OpenMode.ForRead) as Entity;
                    if (collumn?.Bounds != null)
                        _collumns.Add(collumn);
                }

                _empties.Clear();
                foreach (ObjectId o in _acad.GetLayerObjects(prop.Parts.SelectedEmptiesLayer))
                {
                    var emptyOutline = t.GetObject(o, OpenMode.ForRead) as Polyline;
                    if (emptyOutline?.Bounds != null)
                        _empties.Add(emptyOutline);
                }
            }
        }

        public async Task Start(SlabProperties prop)
        {
            SelectedCollisionObjects(prop);
            var algorythim = new SlabAlgorythim(prop);
            IMeshManager manager;

            if (prop.Algorythim.OrientationAngle == 90)
                manager = new MeshManager(prop, _outline);
            else
                manager = new HorizontalMeshManager(prop, _outline);


            using (manager)
            using (_acad.WorkingDocument.LockDocument())
            {
                if (prop.Algorythim.Options.UseLds)
                { 
                    var ldsList = await manager.LdsList;
                    BuildLds(ldsList, prop);
                    BuildLd(await manager.LdList, ldsList, prop);
                } else
                    BuildLd(await manager.LdList, new Point3dCollection(), prop);


                BuildLp(await manager.LpList, prop);

                if (prop.Algorythim.SelectedStartLp != null)
                    BuildStartLp(await manager.StartLpList, prop);

                BuildHead(await manager.HeadList, prop);

                if (!prop.Algorythim.OnlyCimbrament)
                {
                    BuildCast(await manager.CastList, prop);
                    DebugPoints(await manager.CastList, Color.FromRgb(255, 255, 0), 3);
                }


                _acad.WorkingDocument.Editor.WriteMessage("\nLaje finalizada.");
            }
        }

        private void ClearOutlineParts()
        {
            using (_acad.WorkingDocument.LockDocument())
            {
                using (var t = _acad.StartTransaction())
                {
                    var blkTbl = t.GetObject(_acad.Database.BlockTableId, OpenMode.ForWrite) as BlockTable;
                    foreach (var p in _partRepository.GetAll())
                    {
                        if (!blkTbl.Has(p.OutlineReferenceName)) continue;
                        var record = t.GetObject(blkTbl[p.OutlineReferenceName], OpenMode.ForWrite) as BlockTableRecord;
                        record.Erase();
                    }
                    t.Commit();
                }
            }
        }

        public Point3dCollection PlaceMultipleParts(SlabProperties prop, Point3dCollection locations, Part part, ObjectIdCollection placedObjects)
        {
            var collisions = new Point3dCollection();
            var partOutline = GetOrCreatePartOutline(prop, part);
            var n = 0;

            foreach (Point3d loc in locations)
            {
                var orientationAngle = part.GetOrientationAngle(prop.Algorythim.OrientationAngle);
                if (CanPlacePart(loc, part, orientationAngle, partOutline))
                    placedObjects.Add(PlacePart(part, loc, orientationAngle, GetOrCreatePart(part)));
                else
                   collisions.Add(loc);

                if (n % 25 == 0)
                    _acad.WorkingDocument.Editor.UpdateScreen();
                n++;
            }

            return collisions;
        }

        public Point3dCollection PlaceMultipleParts(SlabProperties prop, Point3dCollection locations, Part part)
        {
            var placedObjects = new ObjectIdCollection();
            return PlaceMultipleParts(prop, locations, part, placedObjects);
        }

        private ObjectId PlacePart(Part part, Point3d loc, float orientationAngle, ObjectId blockId)
        {
            var referenceId = ObjectId.Null;

            using (var t = _acad.StartTransaction())
            {
                var blkTbl = t.GetObject(_acad.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                var layerTbl = t.GetObject(_acad.Database.LayerTableId, OpenMode.ForRead, true, true) as LayerTable;
                var modelspace = t.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (var blkRef = new BlockReference(loc, blockId))
                {
                    blkRef.Layer = layerTbl != null && layerTbl.Has(part.Layer)? part.Layer : "0";

                    if (part.UsageType == UsageType.Head && orientationAngle == 90)
                        FixHeadOrientation(part, loc, orientationAngle, blkRef);
                    else if (part.UsageType == UsageType.StartLp && orientationAngle == 90)
                        FixStartLpOrientation(part, loc, orientationAngle, blkRef);
                    else if (part.UsageType == UsageType.Box)
                        FixCastOrientation(part, loc, orientationAngle, blkRef);
                    else
                        FixPartOrientation(part, loc, orientationAngle, blkRef);


                    if (modelspace != null) referenceId = modelspace.AppendEntity(blkRef);
                    t.AddNewlyCreatedDBObject(blkRef, true);
                }


                t.Commit();
            }

            return referenceId;
        }

        #region Builders
        public void BuildCast(Point3dCollection points, SlabProperties properties)
        {
            PlaceMultipleParts(properties, points, properties.Parts.SelectedCast);
        }

        public void BuildLp(Point3dCollection points, SlabProperties properties)
        {
            if (points.Count == 0) return;

            var dangerZoneList = new Point3dCollection();
            var normalZoneList = new Point3dCollection();
            var lastPt = points[points.Count - 1];
            var part = properties.Parts.SelectedLp;
            var orientationAngle = properties.Algorythim.OrientationAngle;

            
            for (var i = 0; i < points.Count; i++)
            {
                var p = points[i];
                normalZoneList.Add(p);
                /*
                 if (al.IsAtTheEnd(lastPt, p) || !SlabAlgorythim.IsInsidePolygon(_outline, p))
                 {
                     var b = al.GetBelowLpPoint(points, p);
                     if (b.HasValue && SlabAlgorythim.IsInsidePolygon(_outline, b.Value))
                     {
                         dangerZoneList.Add(b.Value);
                         normalZoneList.Remove(b.Value);
                         normalZoneList.Remove(p);
                     }
                */
            }

            do
                normalZoneList = PlaceMultipleParts(properties, normalZoneList, part);
            while (normalZoneList.Count > 0 && (part = _partRepository.GetNextSmaller(part, part.UsageType)) != null);

            //if (dangerZoneList.Count > 0)
                //BuildDangerZoneLp(al, dangerZoneList);
        }
       
        /**
         * Monta o conjunto LP + LP Final ou LP + LP caso properties.Algorythim.Options.UseEndLp seja falso.
         */
        private void BuildDangerZoneLp(Point3dCollection points, SlabProperties properties)
        {
            var orientationAngle = properties.Parts.SelectedLp.GetOrientationAngle(properties.Algorythim.OrientationAngle);
            foreach (Point3d p in points)
            {
                var collisionPt = LineCast(p, orientationAngle, properties.Parts.SelectedLp.Width * 2);
                if (collisionPt.HasValue)
                {
                    var dist = collisionPt.Value.DistanceTo(p);
                    Part firstLp, secondLp;
                    FindBetterLpCombination(properties, dist, out firstLp, out secondLp);

                    if (firstLp != null)
                    {
                        var firtLpOutline = GetOrCreatePartOutline(properties, firstLp);
                        if (CanPlacePart(p, firstLp, orientationAngle, firtLpOutline))
                            PlacePart(firstLp, p, orientationAngle, GetOrCreatePart(firstLp));

                        var lpDirection = SlabAlgorythim.VectorFrom(orientationAngle);
                        var nextPoint = p.Add(lpDirection * (firstLp.Width + properties.Algorythim.Options.DistanceBetweenLp));
                        if (secondLp != null)
                        {
                            var secondLpOutline = GetOrCreatePartOutline(properties, secondLp);
                            if (CanPlacePart(nextPoint, secondLp, orientationAngle, secondLpOutline))
                                PlacePart(firstLp, nextPoint, orientationAngle, GetOrCreatePart(secondLp));
                        }
                    }
                }
            }
        }

        public void BuildStartLp(Point3dCollection points, SlabProperties properties)
        {
            var part = properties.Algorythim.SelectedStartLp;
            var orientationAngle = properties.Algorythim.OrientationAngle;

            do
            { 
                points = PlaceMultipleParts(properties, points, part);
            } while (points.Count > 0 && (part = _partRepository.GetNextSmaller(part, part.UsageType)) != null);
        }

        private void BuildAlternativeZoneStartLp(SlabProperties prop, Point3dCollection points, double orientationAngle, ObjectIdCollection placedObjects)
        {
            var alternativePoints = new Point3dCollection();
            var direction = SlabAlgorythim.VectorFrom(orientationAngle);
            var lastPart = prop.Algorythim.SelectedStartLp;
            var part = _partRepository.GetNextSmaller(lastPart, lastPart.UsageType);

            do
            {
                foreach (Point3d point in points)
                {
                    var desloc = direction * (lastPart.Width - part.Width);
                    alternativePoints.Add(point.Add(desloc));
                }

                points = PlaceMultipleParts(prop, points, part, placedObjects);
                lastPart = part;
            } while (points.Count > 0 && (part = _partRepository.GetNextSmaller(part, lastPart.UsageType)) != null);
        }

        public void BuildLd(Point3dCollection points, Point3dCollection ldsPoints, SlabProperties properties)
        {
            if (points.Count == 0) return;
            foreach (Point3d p in ldsPoints)
                try { points.Remove(p); }
                catch (Exception) { }

            var part = properties.Parts.SelectedLd;
            var orientationAngle = 90 - properties.Algorythim.OrientationAngle;
            do
            {
                points = PlaceMultipleParts(properties, points, part);
            } while (points.Count > 0 && (part = _partRepository.GetNextSmaller(part, part.UsageType)) != null);

            //BuildAlternativeZoneLd(properties, points, orientationAngle);
        }

        private void BuildLds(Point3dCollection points, SlabProperties prop)
        {
            if (points.Count == 0) return;

            var part = _partRepository.GetRespectiveOfType(prop.Parts.SelectedLd, UsageType.Lds);
            var orientationAngle = 90 - prop.Algorythim.OrientationAngle;

            do
            {
                points = PlaceMultipleParts(prop, points, part);
            } while (points.Count > 0 && (part = _partRepository.GetNextSmaller(part, part.UsageType)) != null);
        }

        private void BuildAlternativeZoneLd(SlabProperties prop, Point3dCollection points, double orientationAngle)
        {
            var alternativePoints = new Point3dCollection();
            var direction = SlabAlgorythim.VectorFrom(orientationAngle);
            var lastPart = prop.Parts.SelectedLd;
            var part = _partRepository.GetNextSmaller(lastPart, UsageType.Ld);

            do
            {
                foreach (Point3d point in points)
                {
                    var desloc = direction * (lastPart.Width - part.Width);
                    alternativePoints.Add(point.Add(desloc));
                }

                points = PlaceMultipleParts(prop, points, part);
                lastPart = part;
            } while (points.Count > 0 && (part = _partRepository.GetNextSmaller(part, part.UsageType)) != null);
        }

        public void BuildHead(Point3dCollection points, SlabProperties properties)
        {
            var head = _partRepository.GetByModulaton(properties.Algorythim.SelectedModulation).WhereType(UsageType.Head).FirstOrDefault();
            if (head != null) PlaceMultipleParts(properties, points, head);
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
            var curUCSMatrix = _acad.WorkingDocument.Editor.CurrentUserCoordinateSystem;
            var curUCS = curUCSMatrix.CoordinateSystem3d;

            using (var t = _acad.StartTransaction())
            {
                var blockTable = t.GetObject(_acad.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                var modelspace = t.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (var circle = new Circle(point, curUCS.Zaxis, size))
                {
                    circle.Layer = "0";
                    circle.Color = color;
                    modelspace?.AppendEntity(circle);
                    t.AddNewlyCreatedDBObject(circle, true);
                }

                t.Commit();
            }
        }
        #endregion

        #region Helpers
        private ObjectId GetOrCreatePartOutline(SlabProperties prop, Part part)
        {
            var outlinePartId = ObjectId.Null;

            using (var t = _acad.StartTransaction())
            {
                var blkTbl = t.GetObject(_acad.Database.BlockTableId, OpenMode.ForWrite) as BlockTable;
                if (blkTbl.Has(part.OutlineReferenceName)) return blkTbl[part.OutlineReferenceName];

                var border = (part.UsageType == UsageType.Head)? 0 : prop.Algorythim.Options.OutlineDistance - 0.01F;
                using (var record = new BlockTableRecord { Name = part.OutlineReferenceName, Units = UnitsValue.Centimeters, Explodable = true })
                {
                    record.Origin = part.PivotPoint;
                    outlinePartId = blkTbl.Add(record);
                    t.AddNewlyCreatedDBObject(record, true);

                    using (var poly = EntityGenerator.CreateSquare(part.Dimensions, border))
                    {
                        record.AppendEntity(poly);
                        t.AddNewlyCreatedDBObject(poly, true);
                    }

                    foreach (var line in EntityGenerator.CreateCrossLines(part.Dimensions, border))
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
            var partObjectId = ObjectId.Null;

            using (var t = _acad.StartTransaction())
            {
                var blkTbl = t.GetObject(_acad.Database.BlockTableId, OpenMode.ForWrite) as BlockTable;

                if (!blkTbl.Has(part.ReferenceName))
                {
                    if (blkTbl.Has(part.GenericReferenceName))
                        return blkTbl[part.GenericReferenceName];

                    using (var record = new BlockTableRecord())
                    {
                        record.Name = part.GenericReferenceName;
                        record.Units = UnitsValue.Centimeters;
                        record.Origin = part.PivotPoint;
                        record.Explodable = true;
                        partObjectId = blkTbl.Add(record);
                        t.AddNewlyCreatedDBObject(record, true);

                        using (var poly = EntityGenerator.CreateSquare(part.Dimensions, 0))
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

        private void FixPartOrientation(Part part, Point3d loc, float orientationAngle, BlockReference blkRef)
        {
            var angle = GetFixedRotationAngle(blkRef, orientationAngle);

            blkRef.TransformBy(Matrix3d.Rotation(angle, _acad.UCS.Zaxis, loc));
            if (part.UsageType != UsageType.Head)
            {
                var vectorPivot = (part.PivotPoint - Point3d.Origin).RotateBy(-orientationAngle * Math.PI / 180, Vector3d.ZAxis);
                blkRef.Position = blkRef.Position.Add(vectorPivot);
            }
            else
            {
                if (orientationAngle == 0)
                {
                    var middleVector = new Vector3d(part.Width / 2.0, part.Height / 2.0, 0);
                    var vectorPivot = part.PivotPoint - Point3d.Origin;
                    blkRef.Position = blkRef.Position.Subtract(middleVector.Subtract(vectorPivot));
                } 
            }
        }

        private void FixCastOrientation(Part part, Point3d loc, float orientationAngle, BlockReference blkRef)
        {
            var angle = GetFixedRotationAngle(blkRef, orientationAngle);
            var pivotVector = part.PivotPoint - Point3d.Origin;
            blkRef.Position = blkRef.Position.Add(pivotVector);
            blkRef.TransformBy(Matrix3d.Rotation(angle, _acad.UCS.Zaxis, loc));
            blkRef.Position = blkRef.Position.Add(new Vector3d(part.Height, 0, 0));
        }

        private void FixStartLpOrientation(Part part, Point3d loc, float orientationAngle, BlockReference blkRef)
        {
            var angle = GetFixedRotationAngle(blkRef, orientationAngle);
            var pivotVector = part.PivotPoint - Point3d.Origin;
            blkRef.Position = blkRef.Position.Add(pivotVector);
            blkRef.TransformBy(Matrix3d.Rotation(angle, _acad.UCS.Zaxis, loc));
            blkRef.Position = blkRef.Position.Add(new Vector3d(part.Height, 0, 0));
        }

        private void FixHeadOrientation(Part part, Point3d loc, float orientationAngle, BlockReference blkRef)
        {
            var angle = GetFixedRotationAngle(blkRef, orientationAngle);
            var pivotVector = part.PivotPoint - Point3d.Origin;
            blkRef.Position = blkRef.Position.Add(pivotVector);
            blkRef.TransformBy(Matrix3d.Rotation(angle, _acad.UCS.Zaxis, loc));
            blkRef.Position = blkRef.Position.Add(new Vector3d(part.Height, 0, 0));
        }

        private static double GetFixedRotationAngle(Drawable entity, double orientationAngle)
        {
            if (entity.Bounds != null)
            {
                var entityWidth = entity.Bounds.Value.MaxPoint.X - entity.Bounds.Value.MinPoint.X;
                var entityHeight = entity.Bounds.Value.MaxPoint.Y - entity.Bounds.Value.MinPoint.Y;
                var currentAngle = entityWidth >= entityHeight ? 0 : 90;

                return (orientationAngle - currentAngle) * Math.PI / 180D;
            }

            return 0;
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
                    if (reference != null)
                    {
                        reference.IntersectWith(collider, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);

                        if (intersections.Count > 0)
                        {
                            reference.Erase();
                            t.Commit();
                            return;
                        }
                    }
                }
            }
        }
        #endregion

        #region Testing Algorythims
        private void FindBetterLpCombination(SlabProperties properties, double dist, out Part firstLp, out Part secondLp)
        {
            var secondUsageType = properties.Algorythim.Options.UseEndLp ? UsageType.EndLp : UsageType.Lp;
            var firstList = _partRepository.GetByModulaton(properties.Algorythim.SelectedModulation).WhereType(UsageType.Lp);
            var secondList = _partRepository.GetByModulaton(properties.Algorythim.SelectedModulation).WhereType(secondUsageType);
            SlabAlgorythim.FindBetterLpCombination(properties, firstList.ToArray(), secondList.ToArray(), dist, out firstLp, out secondLp);
        }

        protected Point3d? LineCast(Point3d startPoint, double angle, double distance)
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

            var smallestDistance = double.MaxValue;
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

        protected bool CanPlacePart(Point3d loc, Part part, float orientationAngle, ObjectId outlinePartId)
        {
            var isInside = SlabAlgorythim.IsInsidePolygon(_outline, loc);
            if (!isInside) return false;

            //if (part.UsageType == UsageType.Head) return true;
            
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

                partOutlineRef?.Erase();
                t.Commit();
            }

            return true;
        }
        #endregion

        public void Dispose()
        {
            ((IDisposable)_outline)?.Dispose();
            _girders.Dispose();
            _empties.Dispose();
            _collumns.Dispose();

            ClearOutlineParts();
        }

    }
}
