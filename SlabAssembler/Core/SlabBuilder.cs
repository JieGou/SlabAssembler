using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Threading.Tasks;
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
            if (Especifications.Algorythim.SpecifyStartPoint)
            { 
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
                var result = _acad.GetPoint("Informe o ponto de partida");
                if (result.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                    return result.Value;
                else
                    throw new OperationCanceledException();
            } else
            {
                using (var t = _acad.StartOpenCloseTransaction())
                {
                    var outline = t.GetObject(Especifications.Parts.SelectedOutline, OpenMode.ForRead) as Polyline;
                    return outline.Bounds.Value.MinPoint;
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
            if (result.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
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
                Especifications.StartPoint = GetStartPoint().Add(Especifications.StartPointDeslocation);
        }

        public SlabBuildingResult Start()
        {
            InitializeBuilding();
            var algorythim = new SlabAlgorythim(Especifications);
            var result = new SlabBuildingResult();

            using (_acad.WorkingDocument.LockDocument())
            {
                GetSelectedObjects();

                var castList = algorythim.GetCastPointList();
                _acad.WorkingDocument.Editor.WriteMessage($"\n CAST COUNT: {castList.Count}");
                PlaceMultipleParts(result, castList, Especifications.Parts.SelectedCast);
            }

            return result;
        }

        private void GetSelectedObjects()
        {
            using (var t = _acad.StartOpenCloseTransaction())
            {
                _outline = t.GetObject(Especifications.Parts.SelectedOutline, OpenMode.ForRead) as Polyline;

                _girders.Clear();
                foreach (ObjectId o in _acad.GetLayerObjects(Especifications.Parts.SelectedGirdersLayer))
                {
                    var girder = t.GetObject(o, OpenMode.ForRead) as Entity;
                    if (girder != null)
                        _girders.Add(girder);
                }

                _collumns.Clear();
                foreach (ObjectId o in _acad.GetLayerObjects(Especifications.Parts.SelectedColumnsLayer))
                {
                    var collumn = t.GetObject(o, OpenMode.ForRead) as Entity;
                    if (collumn != null)
                        _collumns.Add(collumn);
                }

                _empties.Clear();
                foreach (ObjectId o in _acad.GetLayerObjects(Especifications.Parts.SelectedEmptiesLayer))
                {
                    var emptyOutline = t.GetObject(o, OpenMode.ForRead) as Polyline;
                    if (emptyOutline != null)
                        _empties.Add(emptyOutline);
                }
            }
        }

        protected void PlaceMultipleParts(SlabBuildingResult result, Point3dCollection locations, Part part)
        {
            Point3dCollection collisions = new Point3dCollection();

            foreach (Point3d loc in locations)
            {
                if (CanPlacePart(loc, part))
                {
                    PlaceRepresentationOrReference(part, loc);
                    result.CountNewPart(part);
                }
                else collisions.Add(loc);
            }

            _acad.WorkingDocument.Editor.UpdateScreen();
        }

        private void PlaceRepresentationOrReference(Part part, Point3d loc)
        {
            using (var t = _acad.StartTransaction())
            {
                BlockTable blkTbl = t.GetObject(_acad.Database.BlockTableId, OpenMode.ForWrite) as BlockTable;
                LayerTable layerTbl = t.GetObject(_acad.Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                ObjectId layerId = layerTbl.Has(part.Layer) ? layerTbl[part.Layer] : ObjectId.Null;
                BlockTableRecord modelspace = t.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                if (blkTbl.Has(part.ReferenceName))
                {
                    using (var blkRef = new BlockReference(loc, blkTbl[part.ReferenceName]))
                    {
                        blkRef.LayerId = layerId;
                        modelspace.AppendEntity(blkRef);
                        t.AddNewlyCreatedDBObject(blkRef, true);
                    }
                }
                else
                {
                    using (var genSpline = MakeSplineFromPart(loc, part, 0))
                    {
                        genSpline.LineWeight = LineWeight.LineWeight005;
                        modelspace.AppendEntity(genSpline);
                        t.AddNewlyCreatedDBObject(genSpline, true);
                    }
                }

                t.Commit();
            }
        }

        private bool CanPlacePart(Point3d loc, Part part)
        {
            using (var outerSpline = MakeSplineFromPart(loc, part, Especifications.Algorythim.OutlineDistance))
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

        private Polyline3d MakeSplineFromPart(Point3d location, Part part, double border)
        {
            var pts = new Point3dCollection();
            pts.Add(new Point3d(location.X - border, location.Y - border, 0));
            pts.Add(new Point3d(location.X - border, location.Y + part.Height + border, 0));
            pts.Add(new Point3d(location.X + part.Width + border, location.Y + part.Height + border, 0));
            pts.Add(new Point3d(location.X + part.Width + border, location.Y - border, 0));

            return new Polyline3d(Poly3dType.SimplePoly, pts, true);
        }
    }
}
