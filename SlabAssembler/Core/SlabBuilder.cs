using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ReactiveUI;
using System;
using System.Collections.Generic;
using Urbbox.SlabAssembler.Repositories;
using Urbbox.SlabAssembler.ViewModels;

namespace Urbbox.SlabAssembler.Core
{
    public class SlabBuilder
    {
        protected AutoCadManager Acad;
        public EspecificationsViewModel EspecificationsViewModel {
            set
            {
                Especifications.PartsEspecifications = value;
                Especifications.PartsEspecifications.SelectOutline.Subscribe(x => {
                    try { Especifications.PartsEspecifications.SelectedOutline = SelectOutline(); }
                    catch (ArgumentException ex) {
                        Acad.WorkingDocument.Editor.WriteMessage(ex.Message);
                    }
                });
                Especifications.PartsEspecifications.DrawSlab.Subscribe(x => Start());
            }
        }

        public AlgorythimViewModel AlgorythimViewModel {
            set { Especifications.AlgorythimEspecifications = value; }
        }

        public SlabEspecifications Especifications { get; protected set; }

        public SlabBuilder(AutoCadManager acad)
        {
            this.Especifications = new SlabEspecifications();
            this.Acad = acad;
        }

        public Point3d GetStartPoint()
        {
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
            var result = Acad.GetPoint("Informe o ponto de partida");
            if (result.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                return result.Value;
            else
                throw new OperationCanceledException();
        }

        public Orientation GetOrientation()
        {
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
            var result = Acad.GetKeywords("\nSelecione uma orientação", new[] { "Vertical", "Horizontal" });
            if (result.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                return (result.StringResult == "Vertical") ? Orientation.Vertical : Orientation.Horizontal;
            else
                throw new OperationCanceledException();
        }

        public bool ValidateOutline(ObjectId objectId)
        {
            using (var t = Acad.StartOpenCloseTransaction())
            {
                var outline = t.GetObject(objectId, OpenMode.ForRead) as Polyline;
                return outline != null && outline.Closed;
            }
        }

        public ObjectId SelectOutline()
        {
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
            var result = Acad.GetEntity("\nSelecione o contorno da laje");
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

        public SlabBuildingResult Start()
        {
            InitializeBuilding();
            var result = new SlabBuildingResult();
            Acad.WorkingDocument.Editor.WriteMessage("\nBuild Started!");
            return result;
        }

        private void InitializeBuilding()
        {
            if (Especifications.PartsEspecifications.SelectedOutline == ObjectId.Null) Especifications.PartsEspecifications.SelectedOutline = SelectOutline();
            if (Especifications.AlgorythimEspecifications.SpecifyStartPoint && Especifications.StartPoint == null)
            {
                var p = GetStartPoint();
                Especifications.StartPoint = (new Point2d(p.X, p.Y)).Add(Especifications.StartPointDeslocation);
            }
        }

        public IEnumerable<string> GetLayers()
        {
            return Acad.GetLayers();
        }
     
    }
}
