using Autodesk.AutoCAD.Geometry;
using System;
using Urbbox.SlabAssembler.ViewModels;

namespace Urbbox.SlabAssembler.Core
{
    public class SlabProperties
    {
        private Point3d _startPoint;
        public Point3d StartPoint
        {
            get { return _startPoint; }
            set { _startPoint = value.Add(_startPointDeslocation); }
        }

        public EspecificationsViewModel Parts { get; set; }
        public AlgorythimViewModel Algorythim { get; set; }
        public Point3d MaxPoint { get; set; }
        public int CastGroupSize => (int) (Parts.SelectedLd.Width / Parts.SelectedCast.Width);

        private Vector3d _startPointDeslocation
        {
            get
            {
                if (Parts.SpecifyStartPoint) {
                    if (Algorythim.GlobalOrientationAngle == 90)
                        return new Vector3d(-Parts.SelectedLp.Height - Algorythim.Options.DistanceBetweenLpAndLd, -(Parts.SelectedCast.Height / 2.0) - (Parts.SelectedLd.Height / 2.0), 0);
                    else
                        return new Vector3d(-Parts.SelectedLd.Height / 2.0, -Parts.SelectedCast.Height / 2.0, 0);
                } else {
                    return new Vector3d(Algorythim.Options.OutlineDistance, Algorythim.Options.OutlineDistance, 0);
                }
            }
        }
    }
}
