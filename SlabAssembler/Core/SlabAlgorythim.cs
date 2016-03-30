using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Colors;
using System;
using Autodesk.AutoCAD.Customization;
using System.Collections.Generic;

namespace Urbbox.SlabAssembler.Core
{
    public class SlabAlgorythim
    {
        protected SlabEspecifications Especifications;

        public SlabAlgorythim(SlabEspecifications especifications)
        {
            Especifications = especifications;
        }

        public static Point3d RotatePoint(double x, double y, double z, double angle)
        {
            return new Point3d(x * Math.Cos(angle) + y * Math.Sin(angle), y * Math.Cos(angle) + x * Math.Sin(angle), z);
        }

        public static Point3d RotatePoint(Point3d point, double angle)
        {
            return new Point3d(point.X * Math.Cos(angle) + point.Y * Math.Sin(angle) , point.Y * Math.Cos(angle) + point.X * Math.Sin(angle), point.Z);
        }

        protected Point3dCollection GetPointMatrix(Vector3d startDesloc, double yIncr, double xIncr)
        {
            Point3dCollection list = new Point3dCollection();
            var startPt = Especifications.StartPoint.Add(startDesloc);

            for (double y = startPt.Y; y < Especifications.MaxPoint.Y; y += yIncr)
                for (double x = startPt.X; x < Especifications.MaxPoint.X; x += xIncr)
                    list.Add(RotatePoint(x, y, 0, 90 - Especifications.Algorythim.OrientationAngle));

            return list;
        }

        public Point3dCollection GetCastPointList()
        {
            var result = new Point3dCollection();
            var selectedLp = Especifications.Parts.SelectedLp;
            var selectedLd = Especifications.Parts.SelectedLd;
            var selectedCast = Especifications.Parts.SelectedCast;
            var spacing = Especifications.Algorythim.DistanceBetweenLpAndLd;
            var orientationAngle = Especifications.Algorythim.OrientationAngle;

            var startDesloc = new Vector3d(selectedLp.Height + spacing, selectedLd.Height / 2.0D, 0);
            var xIncr = selectedLd.Width + 2 * spacing + selectedLp.Height;
            var yIncr = selectedCast.Width;
            var auxPts = GetPointMatrix(startDesloc, yIncr, xIncr);

            foreach (Point3d p in auxPts)
                for (int i = 0; i < Especifications.CastGroupSize; i++)
                    result.Add(RotatePoint(p.X + (i * selectedCast.Width), p.Y, 0, 90 - orientationAngle));

            return result;
        }

        public Point3dCollection GetLdPointList()
        {
            var selectedLd = Especifications.Parts.SelectedLd;
            var selectedLp = Especifications.Parts.SelectedLp;
            var selectedCast = Especifications.Parts.SelectedCast;
            var spacing = Especifications.Algorythim.DistanceBetweenLpAndLd;
            var orientationAngle = Especifications.Algorythim.OrientationAngle;

            var startDesloc = new Vector3d(selectedLp.Height + spacing, 0, 0);
            var xIncr = selectedLd.Width + 2 * spacing + selectedLp.Height;
            var yIncr = selectedCast.Height;

            return GetPointMatrix(startDesloc, yIncr, xIncr);
        }

        public Point3dCollection GetLpPointList()
        {
            var selectedLd = Especifications.Parts.SelectedLd;
            var selectedLp = Especifications.Parts.SelectedLp;
            var selectedCast = Especifications.Parts.SelectedCast;
            var spacing = Especifications.Algorythim.DistanceBetweenLpAndLd;
            var orientationAngle = Especifications.Algorythim.OrientationAngle;

            var startDesloc = new Vector3d(0, 0, 0);
            var xIncr = selectedLp.Height + selectedLd.Width + spacing * 2;
            var yIncr = selectedLp.Width + Especifications.Algorythim.DistanceBetweenLp;

            return GetPointMatrix(startDesloc, yIncr, xIncr);
        }

        public static Polyline3d CreateSquare(Part part, double border)
        {
            var pts = new Point3dCollection();
            pts.Add(new Point3d(-border, -border, 0));
            pts.Add(new Point3d(-border, part.Height + border, 0));
            pts.Add(new Point3d(part.Width + border, part.Height + border, 0));
            pts.Add(new Point3d(part.Width + border, - border, 0));
            var polyline = new Polyline3d(Poly3dType.SimplePoly, pts, true);
            if (border > 0) polyline.Color = Color.FromRgb(255, 0, 0);

            return polyline;
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

        public bool checkIfIsStartLP(string startBlockName, Point3d startPoint, Point3d point)
        {
            var orientation = Especifications.Algorythim.SelectedOrientation;
            return startBlockName != null 
                && ((point.Y <= startPoint.Y && orientation == Orientation.Vertical) 
                || (point.X <= startPoint.X && orientation == Orientation.Horizontal));
        }

        public bool checkIfIsLDS(Point3d point, Point3d firstPoint, Point3d lastPoint)
        {
            var orientation = Especifications.Algorythim.SelectedOrientation;
            var isAtTheBeginningOrEndingOnVertical = (point.Y <= firstPoint.Y || point.Y >= lastPoint.Y) && (orientation == Orientation.Vertical);
            var isAtTheBeginningEndingOnHorizontal = (point.X <= firstPoint.X || point.X >= lastPoint.X) && (orientation == Orientation.Horizontal);

            return (isAtTheBeginningOrEndingOnVertical || isAtTheBeginningEndingOnHorizontal);
        }

        public bool isAtTheEnd(Point3d lastPoint, Point3d point)
        {
            var orientation = Especifications.Algorythim.SelectedOrientation;
            return ((point.Y >= lastPoint.Y && orientation == Orientation.Vertical) 
                || (point.X >= lastPoint.X && orientation == Orientation.Horizontal));
        }

        public static int getElementNumberAt(int x, int y, int width)
        {
            return width * y + x;
        }

        public static int getXCoordOfElementAt(int i, int width)
        {
            return i % width;
        }

        public static int getYCoordOfElementAt(int i, int width)
        {
            return i / width;
        }

        public static IEnumerable<Line> CreateCrossLines(Part part, double border)
        {
            yield return new Line(
                new Point3d(part.Width / 2.0F, -border, 0),
                new Point3d(part.Width / 2.0F, part.Height + border, 0)
            );
            yield return new Line(
                new Point3d(-border, part.Height / 2.0F, 0),
                new Point3d(part.Width + border, part.Height / 2.0F, 0)
            );
        }

        public static Vector3d VectorFrom(Point3d p, double angle)
        {
            return new Point3d(Math.Cos(angle), Math.Sin(angle), 0) - p;
        }
    }
}
