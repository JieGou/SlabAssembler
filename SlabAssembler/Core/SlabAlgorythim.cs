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

        protected Point3dCollection GetPointMatrix(Vector3d startDesloc, double yIncr, double xIncr)
        {
            Point3dCollection list = new Point3dCollection();
            var startPt = Especifications.StartPoint.Add(startDesloc);

            for (double y = startPt.Y; y < Especifications.MaxPoint.Y; y += yIncr)
                for (double x = startPt.X; x < Especifications.MaxPoint.X; x += xIncr)
                    if (Especifications.Algorythim.SelectedOrientation == Orientation.Vertical)
                        list.Add(new Point3d(x, y, 0));
                    else
                        list.Add(new Point3d(y, x, 0));


            return list;
        }

        public Point3dCollection GetCastPointList()
        {
            var result = new Point3dCollection();
            var selectedLp = Especifications.Parts.SelectedLp;
            var selectedLd = Especifications.Parts.SelectedLd;
            var selectedCast = Especifications.Parts.SelectedCast;
            var spacing = (2 * Especifications.Algorythim.DistanceBetweenLpAndLd);

            var startDesloc = new Vector3d(0, 0, 0);
            var xIncr = selectedLd.Width + spacing + selectedLp.Height;
            var yIncr = selectedCast.Width;
            var auxPts = GetPointMatrix(startDesloc, yIncr, xIncr);

            foreach (Point3d p in auxPts)
                for (int i = 0; i < Especifications.CastGroupSize; i++)
                    if (Especifications.Algorythim.SelectedOrientation == Orientation.Vertical)
                        result.Add(p.Add(new Vector3d(i * Especifications.Parts.SelectedCast.Width, 0, 0)));
                    else
                        result.Add(p.Add(new Vector3d(0, i * Especifications.Parts.SelectedCast.Width, 0)));

            return result;
        }

        public Point3dCollection GetLdPointList()
        {
            var selectedLd = Especifications.Parts.SelectedLd;
            var selectedLp = Especifications.Parts.SelectedLp;
            var selectedCast = Especifications.Parts.SelectedCast;
            var spacing = (2 * Especifications.Algorythim.DistanceBetweenLpAndLd);

            var startDesloc = new Vector3d(0, -selectedLd.Height / 2.0D, 0);
            var xIncr = selectedLd.Width + spacing + selectedLp.Height;
            var yIncr = selectedCast.Height;

            return GetPointMatrix(startDesloc, yIncr, xIncr);
        }

        public Point3dCollection GetLpPointList()
        {
            var selectedLd = Especifications.Parts.SelectedLd;
            var selectedLp = Especifications.Parts.SelectedLp;
            var selectedCast = Especifications.Parts.SelectedCast;
            var spacing = Especifications.Algorythim.DistanceBetweenLpAndLd;

            var startDesloc = new Vector3d(-selectedLp.Height - spacing, -selectedLd.Height / 2.0D, 0);
            var xIncr = selectedLd.Width + spacing * 2;
            var yIncr = selectedLp.Width + Especifications.Algorythim.DistanceBetweenLp;

            return GetPointMatrix(startDesloc, yIncr, xIncr);
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
