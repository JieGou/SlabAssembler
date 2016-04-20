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
        public SlabProperties Properties { get; set; }

        public SlabAlgorythim(SlabProperties prop)
        {
            Properties = prop;
        }

        public static Point3d RotatePoint(double x, double y, double z, double angle)
        {
            var pt = new Point3d(x, y, z);
            return RotatePoint(pt, angle);
        }

        public static Point3d RotatePoint(Point3d point, double angle)
        {
            return point.RotateBy(ToRadians(angle), Vector3d.ZAxis, Point3d.Origin);
        }

        protected Point3dCollection GetPointMatrix(Vector3d startDesloc, double yIncr, double xIncr)
        {
            Point3dCollection list = new Point3dCollection();
            var angle = ToRadians(90 - Properties.Algorythim.OrientationAngle);
            var startPt = Properties.StartPoint.Add(startDesloc.RotateBy(angle, Vector3d.ZAxis));

            for (double y = startPt.Y; y < Properties.MaxPoint.Y; y += yIncr * Math.Cos(angle) + xIncr * Math.Sin(angle))
                for (double x = startPt.X; x < Properties.MaxPoint.X; x += xIncr * Math.Cos(angle) + yIncr * Math.Sin(angle))
                        list.Add(new Point3d(x, y, 0));

            return list;
        }

        public Point3dCollection GetCastPointList()
        {
            var result = new Point3dCollection();
            var selectedLp = Properties.Parts.SelectedLp;
            var selectedLd = Properties.Parts.SelectedLd;
            var selectedCast = Properties.Parts.SelectedCast;
            var spacing = Properties.Algorythim.DistanceBetweenLpAndLd;
            var orientationAngle = 90 - Properties.Algorythim.OrientationAngle;

            var startDesloc = new Vector3d(selectedLp.Height + spacing, selectedLd.Height / 2.0D, 0);
            var xIncr = selectedLd.Width + 2 * spacing + selectedLp.Height;
            var yIncr = selectedCast.Width;
            var auxPts = GetPointMatrix(startDesloc, yIncr, xIncr);

            foreach (Point3d p in auxPts)
                for (int i = 0; i < Properties.CastGroupSize; i++)
                    result.Add(RotatePoint(p.X + (i * selectedCast.Width), p.Y, 0, orientationAngle));

            return result;
        }

        public Point3dCollection GetLdPointList(bool ignoreLds)
        {
            var selectedLd = Properties.Parts.SelectedLd;
            var selectedLp = Properties.Parts.SelectedLp;
            var selectedCast = Properties.Parts.SelectedCast;
            var spacing = Properties.Algorythim.DistanceBetweenLpAndLd;

            var startDesloc = new Vector3d(selectedLp.Height + spacing, 0, 0);
            var xIncr = selectedLd.Width + 2 * spacing + selectedLp.Height;
            var yIncr = selectedCast.Height;
            var points = GetPointMatrix(startDesloc, yIncr, xIncr);
            if (points.Count == 0) return points;

            if (ignoreLds)
            {
                var firstPoint = points[0];
                var lastPoint = points[points.Count - 1];
                var result = new Point3dCollection();

                foreach (Point3d point in points)
                    if (!CheckIfIsLDS(point, firstPoint, lastPoint))
                        result.Add(point);

                return result;
            }
           
            return points;
        }

        public Point3dCollection GetLpPointList(bool ignoreStartLp)
        {
            var selectedLd = Properties.Parts.SelectedLd;
            var selectedLp = Properties.Parts.SelectedLp;
            var selectedStartLp = Properties.Algorythim.SelectedStartLp;
            var selectedCast = Properties.Parts.SelectedCast;
            var spacing = Properties.Algorythim.DistanceBetweenLpAndLd;
            var useStartLp = Properties.Algorythim.UseStartLp && Properties.Algorythim.SelectedStartLp != null;

            var offset = (useStartLp)? selectedStartLp.StartOffset - (selectedLp.Width - selectedStartLp.Width) + Properties.Algorythim.DistanceBetweenLp : selectedLp.StartOffset;
            var startDesloc = new Vector3d(0, offset, 0);
            var xIncr = selectedLp.Height + selectedLd.Width + spacing * 2;
            var yIncr = selectedLp.Width + Properties.Algorythim.DistanceBetweenLp;
            var points = GetPointMatrix(startDesloc, yIncr, xIncr);
            if (points.Count == 0) return points;

            var startPoint = points[0];
            var result = new Point3dCollection();
            var orientationAngle = Properties.Algorythim.OrientationAngle;
     
            if (ignoreStartLp)
            {
                foreach (Point3d p in points)
                    if (!CheckIfIsStartLP(startPoint, p))
                        result.Add(p);
                return result;
            }

            return points;
        }

        public Point3dCollection GetStartLpPointList()
        {
            var lpPoints = GetLpPointList(false);
            if (lpPoints.Count == 0) return lpPoints;

            var firstPoint = lpPoints[0];
            var result = new Point3dCollection();
            var selectedLp = Properties.Parts.SelectedLp;
            var selectedStartLp = Properties.Algorythim.SelectedStartLp;
            var orientationAngle = ToRadians(Properties.Algorythim.OrientationAngle);
            var pivotFix = (selectedStartLp.PivotPoint - Point3d.Origin).RotateBy(-orientationAngle, Vector3d.ZAxis);
            var posFix = (selectedStartLp.PivotPoint - Point3d.Origin)
                .Add(new Vector3d(selectedLp.Width - selectedStartLp.Width, -selectedStartLp.Height, 0))
                .RotateBy(orientationAngle, Vector3d.ZAxis);

            foreach (Point3d p in lpPoints)
                if (CheckIfIsStartLP(firstPoint, p))
                    result.Add(p.Subtract(pivotFix).Add(posFix));

            return result;
        }

        public Point3dCollection GetHeadPointList(Part selectedHead)
        {
            var ldPoints = GetLdPointList(false);
            if (ldPoints.Count == 0) return ldPoints;

            var orientationAngle = ToRadians(90 - Properties.Algorythim.OrientationAngle);
            var selectedLd = Properties.Parts.SelectedLd;
            var selectedLp = Properties.Parts.SelectedLp;
            var pivotFix = (selectedHead.PivotPoint - Point3d.Origin).RotateBy(-orientationAngle, Vector3d.ZAxis);
            var posFix = new Vector3d(-selectedLp.Height / 2.0f, selectedLd.Height / 2.0f, 0).RotateBy(orientationAngle, Vector3d.ZAxis);
            var result = new Point3dCollection();

            foreach (Point3d p in ldPoints)
                result.Add(p.Subtract(pivotFix).Add(posFix));

            return result;
        }

        public Point3dCollection GetLdsPointList()
        {
            var ldPoints = GetLdPointList(false);
            if (ldPoints.Count == 0) return ldPoints;

            var firstPoint = ldPoints[0];
            var lastPoint = ldPoints[ldPoints.Count - 1];
            var result = new Point3dCollection();

            foreach (Point3d point in ldPoints)
                if (CheckIfIsLDS(point, firstPoint, lastPoint))
                    result.Add(point);

            return result;
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

        public bool CheckIfIsStartLP(Point3d startPoint, Point3d point)
        {
            var orientation = Properties.Algorythim.SelectedOrientation;
            return ((point.Y <= startPoint.Y && orientation == Orientation.Vertical) 
                || (point.X <= startPoint.X && orientation == Orientation.Horizontal));
        }

        private static double ToRadians(double angle)
        {
            return (angle * Math.PI) / 180D;
        }

        public bool CheckIfIsLDS(Point3d point, Point3d firstPoint, Point3d lastPoint)
        {
            var orientation = Properties.Algorythim.SelectedOrientation;
            var isAtTheBeginningOrEndingOnVertical = (point.Y <= firstPoint.Y || point.Y >= lastPoint.Y) && (orientation == Orientation.Vertical);
            var isAtTheBeginningEndingOnHorizontal = (point.X <= firstPoint.X || point.X >= lastPoint.X) && (orientation == Orientation.Horizontal);

            return (isAtTheBeginningOrEndingOnVertical || isAtTheBeginningEndingOnHorizontal);
        }

        public bool IsAtTheEnd(Point3d lastPoint, Point3d point)
        {
            var orientation = Properties.Algorythim.SelectedOrientation;
            return ((point.Y >= lastPoint.Y && orientation == Orientation.Vertical) 
                || (point.X >= lastPoint.X && orientation == Orientation.Horizontal));
        }

        public Point3d? GetBelowLpPoint(Point3dCollection points, Point3d current)
        {
            var dist = Properties.Parts.SelectedLp.Width + Properties.Algorythim.DistanceBetweenLp;
            var orientation = Properties.Algorythim.SelectedOrientation;

            foreach (Point3d point in points)
            {
                if (point != current)
                {
                    var isBellow = (orientation == Orientation.Vertical && point.Y < current.Y)
                        || (orientation == Orientation.Horizontal && point.X > current.X);

                    if (isBellow && current.DistanceTo(point) == dist)
                        return point;
                }
            }

            return null;
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

        public static Vector3d VectorFrom(double angle)
        {
            return RotatePoint(new Point3d(1, 0, 0), angle) - Point3d.Origin;
        }

        public void FindBetterPartCombination(IEnumerable<Part> firstList, IEnumerable<Part> secondList, double distance, out Part firstPart, out Part secondPart)
        {
            firstPart = null;
            secondPart = null;
            double distanceToInterference = distance - Properties.Algorythim.OutlineDistance;
            double delta = double.MaxValue;
            double tmpDelta = 0;

            foreach (var part1 in firstList)
            {
                tmpDelta = part1.Width - distanceToInterference;
                if (tmpDelta <= 0 && Math.Abs(tmpDelta) < delta)
                {
                    delta = Math.Abs(tmpDelta);
                    firstPart = part1;
                }

                foreach (var part2 in secondList)
                {
                    tmpDelta = (part1.Width + Properties.Algorythim.DistanceBetweenLp + part2.Width) - distanceToInterference;
                    if (tmpDelta <= 0 && Math.Abs(tmpDelta) < delta)
                    {
                        delta = Math.Abs(tmpDelta);
                        firstPart = part1;
                        secondPart = part2;
                    }
                }
            }

        }

    }
}
