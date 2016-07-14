using Autodesk.AutoCAD.Geometry;
using Urbbox.SlabAssembler.Core.Models;
using Urbbox.SlabAssembler.Repositories;

namespace Urbbox.SlabAssembler.Core.Strategies.LD
{
    public class HorizontalLDStrategy : LDStrategy
    {
        public HorizontalLDStrategy(SlabProperties prop, IPartRepository repo, AcEnvironment env)
            : base(new Point3dCollection(), prop, repo, env)
        {
        }

        public HorizontalLDStrategy(Point3dCollection ldsPoints, SlabProperties prop, IPartRepository repo, AcEnvironment env)
            : base(ldsPoints, prop, repo, env)
        {
        }

        protected override float PartOrientationAngle => 90;

        protected override double XIncrement => Properties.Parts.SelectedCast.Width;

        protected override double YIncrement
        {
            get
            {
                var lp = Properties.Parts.SelectedLp;
                var spacing = Properties.Algorythim.Options.DistanceBetweenLpAndLd;

                return MainPart.Width + spacing * 2.0 + lp.Height;
            }
        }

        protected override Vector3d StartVector
        {
            get
            {
                var lp = Properties.Parts.SelectedLp;
                var spacing = Properties.Algorythim.Options.DistanceBetweenLpAndLd;

                return new Vector3d(0, lp.Height + spacing, 0);
            }
        }

        protected override void ResetJumpToRight(Part part)
        {
            Y -= MainPart.Width - part.Width;
        }

        protected override void JumpToRight(Part part)
        {
            Y += MainPart.Width - part.Width;
        }
    }
}
