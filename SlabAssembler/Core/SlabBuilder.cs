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

        protected Polyline _outline;
        protected DBObjectCollection _girders;
        protected DBObjectCollection _collumns;
        protected DBObjectCollection _empties;

        public SlabBuilder(AutoCadManager acad)
        {
            Especifications = new SlabEspecifications();
            _acad = acad;
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
                if (result.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
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
            var castPointList = algorythim.GetCastPointList();
            var lpPointList = algorythim.GetLpPointList();
            var ldPointList = algorythim.GetLdPointList();

            using (_acad.WorkingDocument.LockDocument())
            {
                SelectedCollisionObjects();
                try { 
                    if (!Especifications.Algorythim.OnlyCimbrament)
                        PlaceMultipleParts(result, castPointList, Especifications.Parts.SelectedCast, 0, 0);
                    PlaceMultipleParts(result, lpPointList, Especifications.Parts.SelectedLp, -90, 0);
                    PlaceMultipleParts(result, ldPointList, Especifications.Parts.SelectedLd, 0, -90);
                }
                catch (OperationCanceledException) {
                    _acad.WorkingDocument.Editor.WriteMessage("\nLaje cancelada.");
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

        public void PlaceMultipleParts(SlabBuildingResult result, Point3dCollection locations, Part part, double vertRot, double horizRot)
        {
            Point3dCollection collisions = new Point3dCollection();

            foreach (Point3d loc in locations)
            {
                if (CanPlacePart(loc, part))
                {
                    var blockId = GetOrCreatePart(part);
                    var referenceId = PlacePart(part, vertRot, horizRot, loc, blockId);

                    result.CountNewPart(part);
                }
                else collisions.Add(loc);
            }

            _acad.WorkingDocument.Editor.UpdateScreen();
        }

        private ObjectId PlacePart(Part part, double vertRot, double horizRot, Point3d loc, ObjectId blockId)
        {
            var referenceId = ObjectId.Null;

            using (var t = _acad.StartTransaction())
            {
                BlockTable blkTbl = t.GetObject(_acad.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelspace = t.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (var blkRef = new BlockReference(loc, blockId))
                {
                    var record = t.GetObject(
                        blkRef.IsDynamicBlock ? blkRef.DynamicBlockTableRecord : blkRef.BlockTableRecord,
                        OpenMode.ForRead) as BlockTableRecord;
                    var angle = GetFixedRotationAngle(blkRef, vertRot, horizRot);
                    if (angle != 0) { 
                        blkRef.TransformBy(Matrix3d.Rotation(angle, Vector3d.ZAxis, loc));
                        blkRef.Position = blkRef.Position.Add(new Vector3d(part.Height - part.PivotPointY, -part.PivotPointY, 0));
                    }

                    referenceId = modelspace.AppendEntity(blkRef);
                    t.AddNewlyCreatedDBObject(blkRef, true);
                }
                
                t.Commit();
            }

            _acad.WorkingDocument.Editor.UpdateScreen();
            return referenceId;
        }

        private double GetFixedRotationAngle(BlockReference entity, double vertRot, double horizRot)
        {
            var entityWidth = entity.Bounds.Value.MaxPoint.X - entity.Bounds.Value.MinPoint.X;
            var entityHeight = entity.Bounds.Value.MaxPoint.Y - entity.Bounds.Value.MinPoint.Y;
            var currentAngle = entityWidth >= entityHeight ? 0 : 90;
            var orientationAngle = (Especifications.Algorythim.SelectedOrientation == Orientation.Vertical) ? vertRot : horizRot;

            return ((currentAngle - orientationAngle) * Math.PI) / 180D;
        }

        public ObjectId GetOrCreatePart(Part part)
        {
            ObjectId partObjectId = ObjectId.Null;
            const string genericSuffix = "_GEN";

            using (var t = _acad.StartTransaction())
            {
                BlockTable blkTbl = t.GetObject(_acad.Database.BlockTableId, OpenMode.ForWrite) as BlockTable;
                BlockTableRecord modelspace = t.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                if (!blkTbl.Has(part.ReferenceName) && !blkTbl.Has(part.ReferenceName + genericSuffix))
                {
                    using (var genSpline = SlabAlgorythim.CreateSquare(Point3d.Origin, part, 0))
                    {
                        genSpline.SetDatabaseDefaults();

                        var record = new BlockTableRecord();
                        record.Name = part.ReferenceName + genericSuffix;
                        record.Origin = part.PivotPoint;
                        partObjectId = _acad.Database.AddDBObject(record);
                        t.AddNewlyCreatedDBObject(record, true);

                        record.AppendEntity(genSpline);
                        t.AddNewlyCreatedDBObject(genSpline, true);
                    }
                }
                else partObjectId = blkTbl[part.ReferenceName];

                t.Commit();
            }

            return partObjectId;
        }

        private bool CanPlacePart(Point3d loc, Part part)
        {
            using (var outerSpline = SlabAlgorythim.CreateSquare(loc, part, Especifications.Algorythim.OutlineDistance - 0.001))
            {
                var isInside = SlabAlgorythim.IsInsidePolygon(_outline, loc);
                if (!isInside) return false;

                var intersections = new Point3dCollection();

                _outline.IntersectWith(outerSpline, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                if (intersections.Count > 0) return false;

                foreach (DBObject o in _girders)
                { 
                    outerSpline.IntersectWith(o as Entity, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                    if (intersections.Count > 0) return false;
                }

                foreach (DBObject o in _collumns)
                {
                    outerSpline.IntersectWith(o as Entity, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                    if (intersections.Count > 0) return false;
                }

                foreach (DBObject o in _empties)
                { 
                    outerSpline.IntersectWith(o as Entity, Intersect.OnBothOperands, intersections, IntPtr.Zero, IntPtr.Zero);
                    if (intersections.Count > 0) return false;

                    Polyline p = o as Polyline;
                    if (SlabAlgorythim.IsInsidePolygon(p, loc)) return false;
                }
            }

            return true;
        }

    }
}
