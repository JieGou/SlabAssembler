using Autodesk.AutoCAD.Geometry;
using Urbbox.SlabAssembler.ViewModels;

namespace Urbbox.SlabAssembler.Core
{
    public class SlabEspecifications
    {
        public EspecificationsViewModel Parts { get; set; }
        public AlgorythimViewModel Algorythim { get; set; }

        public Point3d StartPoint { get; set; }
        public Point3d MaxPoint { get; set; }
        public Vector3d StartPointDeslocation =>
            (Parts.SpecifyStartPoint)?
            new Vector3d(-Parts.SelectedLp.Height - Algorythim.DistanceBetweenLpAndLd, -(Parts.SelectedCast.Height / 2.0f) - (Parts.SelectedLd.Height / 2.0f), 0) :
            new Vector3d(Algorythim.OutlineDistance, Algorythim.OutlineDistance, 0);
        public int CastGroupSize => (int) (Parts.SelectedLd.GreatestDimension / Parts.SelectedCast.Width);
    }
}
