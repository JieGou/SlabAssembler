using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using Urbbox.SlabAssembler.Core.Models;
using Urbbox.SlabAssembler.Managers;
using Urbbox.SlabAssembler.Repositories;

namespace Urbbox.SlabAssembler.Core.Strategies
{
    public abstract class Strategy : IStrategy
    {
        private Part _part;
        private AutoCadManager _acad;

        protected SlabProperties Properties { get; }
        protected Point3d StartPoint { get; }
        protected Point3d CurrentPoint => new Point3d(X, Y, 0);
        protected double X { get; set; }
        protected double Y { get; set; }
        protected ObjectId PartOutlineBlockId { get; private set; }
        protected ObjectId PartBlockId { get; private set; }
        protected IPartRepository PartRepository { get; }
        protected Document Document => _acad.WorkingDocument;

        public AcEnvironment Environment { get; }

        public Strategy(SlabProperties properties, IPartRepository repo, AcEnvironment environment)
        {
            Properties = properties;
            Environment = environment;
            PartRepository = repo;
            StartPoint = properties.StartPoint.Add(StartVector);
            _acad = new AutoCadManager();

            ResetX();
            ResetY();
        }

        protected Part CurrentPart
        {
            get { return _part; }
            set
            {
                _part = value;

                using (var tr = _acad.StartTransaction())
                {
                    var blockTable = tr.GetObject(_acad.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (!blockTable.Has(CurrentPart.OutlineReferenceName))
                        CreateOutlineRecord(tr, blockTable);

                    PartOutlineBlockId = blockTable[CurrentPart.OutlineReferenceName];
                    PartBlockId = blockTable.Has(CurrentPart.ReferenceName) ? blockTable[CurrentPart.ReferenceName] : blockTable[CurrentPart.GenericReferenceName];

                    tr.Commit();
                }
            }
        }

        protected void CreateOutlineRecord(Transaction tr, BlockTable blockTable)
        {
            using (var record = new OutlineBlockTableRecord(CurrentPart))
            {
                blockTable.UpgradeOpen();
                blockTable.Add(record);
                tr.AddNewlyCreatedDBObject(record, true);

                record.BuildObject(tr, Properties.Algorythim.Options.OutlineDistance);
            }
        }

        protected virtual bool IsCollidingOutline(BlockReference blkRef)
        {
            var intersections = new Point3dCollection();
            Environment.Outline.IntersectWith(blkRef, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);

            return intersections.Count > 0;
        }

        protected virtual bool IsCollidingAnyObstacle(BlockReference blkRef)
        {
            var intersections = new Point3dCollection();

            foreach (var girder in Environment.Girders)
            {
                blkRef.IntersectWith(girder, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                if (intersections.Count > 0) return true;
            }

            foreach (var collumn in Environment.Collumns)
            {
                blkRef.IntersectWith(collumn, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                if (intersections.Count > 0) return true;
            }

            foreach (var empty in Environment.Empties)
            {
                blkRef.IntersectWith(empty, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                if (intersections.Count > 0) return true;
            }

            return false;
        }

        protected virtual bool IsOutsideOutline()
        {
            return !SlabAlgorythim.IsInsidePolygon(Environment.Outline, CurrentPoint);
        }

        protected virtual bool IsInsideAnyEmpty()
        {
            foreach (var empty in Environment.Empties)
                if (SlabAlgorythim.IsInsidePolygon(empty, CurrentPoint))
                    return true;

            return false;
        }

        protected void ResetX()
        {
            X = StartPoint.X;
        }

        protected void ResetY()
        {
            Y = StartPoint.Y;
        }

        protected void Place()
        {
            using (var tr = _acad.StartTransaction())
            {
                var blockTable = tr.GetObject(_acad.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                var workspace = tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (var blkRef = new BlockReference(CurrentPoint, PartBlockId))
                {
                    Place(blkRef);

                    workspace.AppendEntity(blkRef);
                    tr.AddNewlyCreatedDBObject(blkRef, true);
                }

                tr.Commit();
            }
        }

        protected void DebugPoint(Point3d point, Color color, int size)
        {
            var curUCSMatrix = _acad.WorkingDocument.Editor.CurrentUserCoordinateSystem;
            var curUCS = curUCSMatrix.CoordinateSystem3d;

            using (var t = _acad.StartOpenCloseTransaction())
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

        protected virtual void Place(BlockReference blkRef)
        {
            var pivotVector = CurrentPart.PivotPoint - Point3d.Origin;
            var orientationAngle = PartOrientationAngle * Math.PI / 180.0;

            blkRef.Position = blkRef.Position.Add(pivotVector);
            blkRef.TransformBy(Matrix3d.Rotation(orientationAngle, _acad.UCS.Zaxis, CurrentPoint));
            blkRef.Position = blkRef.Position.Add(new Vector3d(CurrentPart.Height, 0, 0));
        }

        protected bool CanPlace()
        {
            var result = true;

            using (var tr = _acad.StartTransaction())
            {
                var blockTable = tr.GetObject(_acad.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                var workspace = tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (var blkRef = new BlockReference(CurrentPoint, PartOutlineBlockId))
                {
                    workspace.AppendEntity(blkRef);
                    tr.AddNewlyCreatedDBObject(blkRef, true);
                    Place(blkRef);

                    result = CanPlace(blkRef);
                }

                tr.Abort();
            }

            return result;
        }

        protected Part NextPart
            => PartRepository.GetNextSmaller(CurrentPart, CurrentPart.UsageType);

        protected abstract bool CanPlace(BlockReference blkRef);
        protected abstract Vector3d StartVector { get; }
        protected abstract double XIncrement { get; }
        protected abstract double YIncrement { get; }
        protected abstract float PartOrientationAngle { get; }

        public abstract void Run();
    }
}
