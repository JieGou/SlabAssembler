using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.DatabaseServices;
using System;

namespace Urbbox.SlabAssembler.Core
{
    class SlabAlgorythim
    {
        protected SlabEspecifications Especifications;

        public SlabAlgorythim(SlabEspecifications especifications)
        {
            Especifications = especifications;
        }

        protected Point3d CorrectOrientation(double x, double y, double z)
        {
            if (Especifications.Algorythim.SelectedOrientation == Orientation.Vertical)
                return new Point3d(x, y, z);
            else
                return new Point3d(y, x, z);
        }

        protected Point3d CorrectOrientation(Point3d point)
        {
            if (Especifications.Algorythim.SelectedOrientation == Orientation.Vertical)
                return point;
            else
                return new Point3d(point.Y, point.X, point.Z);
        }

        protected Point3dCollection GetPointMatrix(Vector3d startDesloc, double yIncr, double xIncr)
        {
            Point3dCollection list = new Point3dCollection();
            var startPt = Especifications.StartPoint.Add(startDesloc);

            for (double y = startPt.Y; y < Especifications.MaxPoint.Y; y += yIncr)
                for (double x = startPt.X; x < Especifications.MaxPoint.X; x += xIncr)
                    list.Add(CorrectOrientation(x, y, 0));

            return list;
        }

        public Point3dCollection GetCastPointList()
        {
            var result = new Point3dCollection();
            var selectedLp = Especifications.Parts.SelectedLp;
            var selectedLd = Especifications.Parts.SelectedLd;
            var selectedCast = Especifications.Parts.SelectedCast;
            var spacing = Especifications.Algorythim.DistanceBetweenLpAndLd;

            var startDesloc = new Vector3d(selectedLp.Height + spacing, selectedLd.Height / 2.0D, 0);
            var xIncr = selectedLd.Width + 2 * spacing + selectedLp.Height;
            var yIncr = selectedCast.Width;
            var pivotVector = CorrectOrientation(selectedCast.PivotPoint) - Point3d.Origin;
            var auxPts = GetPointMatrix(startDesloc + pivotVector, yIncr, xIncr);

            foreach (Point3d p in auxPts)
                for (int i = 0; i < Especifications.CastGroupSize; i++)
                    result.Add(CorrectOrientation(p.X + (i * selectedCast.Width), p.Y, 0));

            return result;
        }

        public Point3dCollection GetLdPointList()
        {
            var selectedLd = Especifications.Parts.SelectedLd;
            var selectedLp = Especifications.Parts.SelectedLp;
            var selectedCast = Especifications.Parts.SelectedCast;
            var spacing = Especifications.Algorythim.DistanceBetweenLpAndLd;

            var startDesloc = new Vector3d(selectedLp.Height + spacing, 0, 0);
            var xIncr = selectedLd.Width + 2 * spacing + selectedLp.Height;
            var yIncr = selectedCast.Height;
            var pivotVector = selectedLd.PivotPoint - Point3d.Origin;

            return GetPointMatrix(startDesloc + pivotVector, yIncr, xIncr);
        }

        public Point3dCollection GetLpPointList()
        {
            var selectedLd = Especifications.Parts.SelectedLd;
            var selectedLp = Especifications.Parts.SelectedLp;
            var selectedCast = Especifications.Parts.SelectedCast;
            var spacing = Especifications.Algorythim.DistanceBetweenLpAndLd;

            var startDesloc = new Vector3d(0, 0, 0);
            var xIncr = selectedLp.Height + selectedLd.Width + spacing * 2;
            var yIncr = selectedLp.Width + Especifications.Algorythim.DistanceBetweenLp;
            var pivotVector = selectedLp.PivotPoint - Point3d.Origin;

            return GetPointMatrix(startDesloc + pivotVector, yIncr, xIncr);
        }

        public static Polyline3d CreateSquare(Point3d location, Part part, double border)
        {
            var pts = new Point3dCollection();
            pts.Add(new Point3d(location.X - border, location.Y - border, 0));
            pts.Add(new Point3d(location.X - border, location.Y + part.Height + border, 0));
            pts.Add(new Point3d(location.X + part.Width + border, location.Y + part.Height + border, 0));
            pts.Add(new Point3d(location.X + part.Width + border, location.Y - border, 0));

            return new Polyline3d(Poly3dType.SimplePoly, pts, true);
        }

        public static bool IsInsidePolygon(Polyline polygon, Point3d pt)
        {
            int n = polygon.NumberOfVertices;
            double angle = 0;
            Point2d pt1, pt2;

            for (int i = 0; i < n; i++)
            {
                pt1 = new Point2d(polygon.GetPoint2dAt(i).X - pt.X, polygon.GetPoint2dAt(i).Y - pt.Y);
                pt2 = new Point2d(polygon.GetPoint2dAt((i + 1) % n).X - pt.X, polygon.GetPoint2dAt((i + 1) % n).Y - pt.Y);
                angle += GetAngle2D(pt1.X, pt1.Y, pt2.X, pt2.Y);
            }

            if (Math.Abs(angle) < Math.PI)
                return false;
            else
                return true;
        }

        private static double GetAngle2D(double x1, double y1, double x2, double y2)
        {
            double dtheta, theta1, theta2;

            theta1 = Math.Atan2(y1, x1);
            theta2 = Math.Atan2(y2, x2);
            dtheta = theta2 - theta1;
            while (dtheta > Math.PI)
                dtheta -= (Math.PI * 2);
            while (dtheta < -Math.PI)
                dtheta += (Math.PI * 2);
            return (dtheta);
        }

    }
}
