using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using Urbbox.SlabAssembler.Repositories;
using System.Linq;
using Urbbox.SlabAssembler.Core.Variations;
using Urbbox.SlabAssembler.Core.Models;
using Urbbox.SlabAssembler.Managers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Urbbox.SlabAssembler.Core.Strategies;
using Urbbox.SlabAssembler.Core.Strategies.LD;

namespace Urbbox.SlabAssembler.Core
{
    public class SlabBuilder : IDisposable
    {
        private readonly AutoCadManager _acad;
        private readonly IPartRepository _partRepository;
        private readonly SlabProperties _properties;
        private AcEnvironment _environment;

        public SlabBuilder(IPartRepository repo, SlabProperties properties)
        {
            _partRepository = repo;
            _properties = properties;
            _acad = new AutoCadManager();
            _environment = new AcEnvironment(properties.Parts.SelectedOutline)
            {
                GirdersLayer = properties.Parts.SelectedGirdersLayer,
                CollumnsLayer = properties.Parts.SelectedColumnsLayer,
                EmptiesLayer = properties.Parts.SelectedEmptiesLayer,
            };
        }

        public async Task Start()
        {
            IMeshManager manager;
            if (_properties.Algorythim.GlobalOrientationAngle == 90)
                manager = new MeshManager(_properties, _environment.Outline);
            else
                manager = new HorizontalMeshManager(_properties, _environment.Outline);

            using (manager)
            using (_acad.WorkingDocument.LockDocument())
            {
                if (_properties.Algorythim.Options.UseLds)
                { 
                    var ldsList = await manager.LdsList;
                    BuildLds(ldsList);
                    BuildLd(ldsList);
                } else
                    BuildLd(new Point3dCollection());

                var endings = await manager.EndLpList;
                BuildEndLp(endings);
                BuildLp(await manager.LpList, endings);

                if (_properties.Algorythim.SelectedStartLp != null)
                    BuildStartLp(await manager.StartLpList);

                BuildHead(await manager.HeadList);

                if (!_properties.Algorythim.OnlyCimbrament)
                    BuildCast(await manager.CastList);

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

        public Point3dCollection PlaceMultipleParts(Point3dCollection locations, Part part, ObjectIdCollection placedObjects)
        {
            var collisions = new Point3dCollection();
            var partOutline = GetOrCreatePartOutline(part);
            var n = 0;

            foreach (Point3d loc in locations)
            {
                var orientationAngle = part.GetOrientationAngle(_properties.Algorythim.GlobalOrientationAngle);
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

        public Point3dCollection PlaceMultipleParts(Point3dCollection locations, Part part)
        {
            var placedObjects = new ObjectIdCollection();
            return PlaceMultipleParts(locations, part, placedObjects);
        }

        private ObjectId PlacePart(Part part, Point3d loc, float orientationAngle, ObjectId blockId)
        {
            var referenceId = ObjectId.Null;

            using (var t = _acad.StartTransaction())
            {
                var blkTbl = t.GetObject(_acad.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                var layerTbl = t.GetObject(_acad.Database.LayerTableId, OpenMode.ForRead, true, true) as LayerTable;
                var modelspace = t.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                var entity = t.GetObject(blockId, OpenMode.ForRead) as Entity;

                using (var blkRef = new BlockReference(loc, blockId))
                {
                    blkRef.Layer = layerTbl != null && layerTbl.Has(part.Layer)? part.Layer : "0";
                    var angle = orientationAngle * Math.PI / 180.0;

                    if (part.UsageType == UsageType.Head && orientationAngle == 90)
                        FixOrientation(part, loc, angle, blkRef);
                    else if (part.UsageType == UsageType.StartLp && orientationAngle == 90)
                        FixOrientation(part, loc, angle, blkRef);
                    else if (part.UsageType == UsageType.Box && orientationAngle == 90)
                        FixOrientation(part, loc, angle, blkRef);
                    else
                        FixPartOrientation(part, loc, angle, blkRef);

                    referenceId = modelspace.AppendEntity(blkRef);
                    t.AddNewlyCreatedDBObject(blkRef, true);
                }


                t.Commit();
            }

            return referenceId;
        }

        #region Builders
        public void BuildCast(Point3dCollection points)
        {
            PlaceMultipleParts(points, _properties.Parts.SelectedCast);
        }

        public void BuildLp(Point3dCollection points, Dictionary<Point3d, Point3dCollection> scanlines)
        {
            if (points.Count == 0) return;

            var part = _properties.Parts.SelectedLp;
            foreach (var scanline in scanlines)
                foreach (Point3d p in scanline.Value)
                    if (points.Contains(p))
                        points.Remove(p);

            do
                points = PlaceMultipleParts(points, part);
            while (points.Count > 0 && (part = _partRepository.GetNextSmaller(part, part.UsageType)) != null);
        }
       
        private void BuildEndLp(Dictionary<Point3d, Point3dCollection> scanlines)
        {
            var orientationAngle = _properties.Parts.SelectedLp.GetOrientationAngle(_properties.Algorythim.GlobalOrientationAngle);
            foreach (var scanline in scanlines)
            {
                var p = scanline.Value[2];
                var dist = scanline.Value[0].DistanceTo(scanline.Value[2]);
                Part firstLp, secondLp;
                FindBetterLpCombination(_properties, dist, out firstLp, out secondLp);

                if (firstLp != null)
                {
                    var firtLpOutline = GetOrCreatePartOutline(firstLp);
                    if (CanPlacePart(p, firstLp, orientationAngle, firtLpOutline))
                        PlacePart(firstLp, p, orientationAngle, GetOrCreatePart(firstLp));
                    if (secondLp != null)
                    {
                        var lpDirection = SlabAlgorythim.VectorFrom(orientationAngle);
                        var nextPoint = p.Add(lpDirection * (firstLp.Width + _properties.Algorythim.Options.DistanceBetweenLp));
                        var secondLpOutline = GetOrCreatePartOutline(secondLp);
                        if (CanPlacePart(nextPoint, secondLp, orientationAngle, secondLpOutline))
                            PlacePart(firstLp, nextPoint, orientationAngle, GetOrCreatePart(secondLp));
                    }
                }
            }
        }

        public void BuildStartLp(Point3dCollection points)
        {
            var part = _properties.Algorythim.SelectedStartLp;
            var orientationAngle = _properties.Algorythim.GlobalOrientationAngle;

            do
            { 
                points = PlaceMultipleParts(points, part);
            } while (points.Count > 0 && (part = _partRepository.GetNextSmaller(part, part.UsageType)) != null);
        }

        private void BuildAlternativeZoneStartLp(Point3dCollection points, double orientationAngle, ObjectIdCollection placedObjects)
        {
            var alternativePoints = new Point3dCollection();
            var direction = SlabAlgorythim.VectorFrom(orientationAngle);
            var lastPart = _properties.Algorythim.SelectedStartLp;
            var part = _partRepository.GetNextSmaller(lastPart, lastPart.UsageType);

            do
            {
                foreach (Point3d point in points)
                {
                    var desloc = direction * (lastPart.Width - part.Width);
                    alternativePoints.Add(point.Add(desloc));
                }

                points = PlaceMultipleParts(points, part, placedObjects);
            } while (points.Count > 0 && (part = _partRepository.GetNextSmaller(part, lastPart.UsageType)) != null);
        }

        public void BuildLd(Point3dCollection ldsPoints)
        {
            IStrategy ldStrategy;

            if (_properties.Algorythim.GlobalOrientationAngle == 90)
                ldStrategy = new VerticalLDStrategy(ldsPoints, _properties, _partRepository, _environment);
            else
                ldStrategy = new HorizontalLDStrategy(ldsPoints, _properties, _partRepository, _environment);

            ldStrategy.Run();
        }

        private void BuildLds(Point3dCollection points)
        {
            if (points.Count == 0) return;

            var part = _partRepository.GetRespectiveOfType(_properties.Parts.SelectedLd, UsageType.Lds);
            var orientationAngle = 90 - _properties.Algorythim.GlobalOrientationAngle;

            do
            {
                points = PlaceMultipleParts(points, part);
            } while (points.Count > 0 && (part = _partRepository.GetNextSmaller(part, part.UsageType)) != null);
        }

        public void BuildAlternativeLd(Point3dCollection points, Part lastPart)
        {
            if (points.Count == 0) return;
            var part = lastPart;
            var direction = SlabAlgorythim.VectorFrom(90 - _properties.Algorythim.GlobalOrientationAngle);
            var nextPart = _partRepository.GetNextSmaller(part, lastPart.UsageType);

            for (int i = 0; i < points.Count; i++)
                points[i] = points[i].Add(direction * (lastPart.Width - part.Width));
            
            points = PlaceMultipleParts(points, part);

            if (points.Count > 0 && nextPart != null)
                BuildAlternativeLd(points, nextPart);
        }

        public void BuildHead(Point3dCollection points)
        {
            var head = _partRepository.GetByModulaton(_properties.Algorythim.SelectedModulation).WhereType(UsageType.Head).FirstOrDefault();
            if (head != null) PlaceMultipleParts(points, head);
        }
        #endregion

        #region Helpers
        private ObjectId GetOrCreatePartOutline(Part part)
        {
            var outlinePartId = ObjectId.Null;

            using (var t = _acad.StartTransaction())
            {
                var blkTbl = t.GetObject(_acad.Database.BlockTableId, OpenMode.ForWrite) as BlockTable;
                if (blkTbl.Has(part.OutlineReferenceName)) return blkTbl[part.OutlineReferenceName];

                var border = (part.UsageType == UsageType.Head)? 0 : _properties.Algorythim.Options.OutlineDistance - 0.01F;
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

                    using (var poly = EntityGenerator.CreateSquare(part.Dimensions))
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

        private void FixPartOrientation(Part part, Point3d loc, double orientationAngle, BlockReference blkRef)
        {
            blkRef.TransformBy(Matrix3d.Rotation(orientationAngle, _acad.UCS.Zaxis, loc));
            if (part.UsageType != UsageType.Head)
            {
                var vectorPivot = (part.PivotPoint - Point3d.Origin).RotateBy(-orientationAngle, Vector3d.ZAxis);
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

        private void FixOrientation(Part part, Point3d loc, double orientationAngle, BlockReference blkRef)
        {
            var pivotVector = part.PivotPoint - Point3d.Origin;
            blkRef.Position = blkRef.Position.Add(pivotVector);
            blkRef.TransformBy(Matrix3d.Rotation(orientationAngle, _acad.UCS.Zaxis, loc));
            blkRef.Position = blkRef.Position.Add(new Vector3d(part.Height, 0, 0));
        }

        private static double GetFixedRotationAngle(Entity entity, double orientationAngle)
        {
            if (entity.Bounds.HasValue)
            {
                var extends = entity.GeometricExtents;
                var entityWidth = extends.MaxPoint.X - extends.MinPoint.X;
                var entityHeight = extends.MaxPoint.Y - extends.MinPoint.Y;
                var currentAngle = entityWidth >= entityHeight ? 0 : 90;

                return (orientationAngle - currentAngle) * Math.PI / 180.0;
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
                _environment.Outline.IntersectWith(line, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);

                foreach (var o in _environment.Girders)
                    line.IntersectWith(o, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);

                foreach (var o in _environment.Collumns)
                    line.IntersectWith(o, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);

                foreach (var o in _environment.Empties)
                    line.IntersectWith(o, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
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
            var isInside = SlabAlgorythim.IsInsidePolygon(_environment.Outline, loc);
            if (!isInside) return false;

            //if (part.UsageType == UsageType.Head) return true;
            
            using (var t = _acad.StartTransaction())
            {
                var partOutlineRefId = PlacePart(part, loc, orientationAngle, outlinePartId);
                var partOutlineRef = t.GetObject(partOutlineRefId, OpenMode.ForRead) as BlockReference;

                var intersections = new Point3dCollection();
                _environment.Outline.IntersectWith(partOutlineRef, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                if (intersections.Count > 0) return false;

                foreach (var e in _environment.Girders)
                {
                    partOutlineRef.IntersectWith(e, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                    if (intersections.Count > 0) return false;
                }

                foreach (var e in _environment.Collumns)
                {
                    partOutlineRef.IntersectWith(e, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                    if (intersections.Count > 0) return false;
                }

                foreach (var e in _environment.Empties)
                {
                    partOutlineRef.IntersectWith(e, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                    if (intersections.Count > 0) return false;

                    if (SlabAlgorythim.IsInsidePolygon(e, loc)) return false;
                }

                partOutlineRef?.Erase();
                t.Commit();
            }

            return true;
        }
        #endregion

        public void Dispose()
        {
            ClearOutlineParts();
        }

    }
}
