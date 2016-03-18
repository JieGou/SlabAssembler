using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.Geometry;
using Urbbox.SlabAssembler.ViewModels;

namespace Urbbox.SlabAssembler.Core
{
    public class SlabEspecifications
    {
        public EspecificationsViewModel PartsEspecifications { get; set; }
        public AlgorythimViewModel AlgorythimEspecifications { get; set; }

        public Point2d StartPoint { get; set; }
        public Vector2d StartPointDeslocation => new Vector2d(0, -(PartsEspecifications.SelectedCast.Height / 2.0f));
        public int CastGroupSize => (int) (PartsEspecifications.SelectedLd.GreatestDimension / PartsEspecifications.SelectedCast.Width);
    }
}
