using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Urbbox.SlabAssembler.Core.Models;
using Urbbox.SlabAssembler.Repositories;

namespace Urbbox.SlabAssembler.Core.Strategies.LD
{
    public abstract class LDStrategy : Strategy
    {
        protected Part MainPart { get; private set; }

        public LDStrategy(SlabProperties properties, IPartRepository repo, AcEnvironment env) : base(properties, repo, env)
        {
            MainPart = Properties.Parts.SelectedLd;
        }

        protected override bool CanPlace(BlockReference blkRef)
        {
            if (IsCollidingOutline(blkRef))
                return false;

            if (IsCollidingAnyObstacle(blkRef))
                return false;

            if (IsInsideAnyEmpty())
                return false;

            return true;
        }

        protected abstract override Vector3d StartVector { get; } 
        protected abstract override double XIncrement { get; }
        protected abstract override double YIncrement { get; }       
        protected abstract override float PartOrientationAngle { get; }
        protected abstract void JumpToRight(Part part);
        protected abstract void ResetJumpToRight(Part part);

        public override void Run()
        {
            while (X < Properties.MaxPoint.X)
            {
                while (Y < Properties.MaxPoint.Y)
                {
                    PlacedAtRight = false;
                    ChooseAndPlacePart(MainPart);
                    Y += YIncrement;
                }
                ResetY();

                X += XIncrement;
            }
            ResetX();
        }

        protected bool PlacedAtRight { get; set; }
        private void ChooseAndPlacePart(Part part)
        {
            if (part == null || IsOutsideOutline())
                return;

            CurrentPart = part;
            if (CanPlace())
                Place();
            else
            {
                if (!PlacedAtRight) ChooseAndPlacePartAtRight(NextPart);
                ChooseAndPlacePart(NextPart);
            }
        }

        private void ChooseAndPlacePartAtRight(Part part)
        {
            if (part == null)
                return;

            JumpToRight(part);

            if (IsOutsideOutline())
            {
                ResetJumpToRight(part);
                return;
            }

            CurrentPart = part;
            if (CanPlace())
            {
                Place();
                PlacedAtRight = true;
            }

            ResetJumpToRight(part);
        }

    }
}
