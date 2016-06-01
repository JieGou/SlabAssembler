using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Colors;
using System;
using Autodesk.AutoCAD.Customization;
using System.Collections.Generic;
using Urbbox.SlabAssembler.Core.Models;

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

        private static double Clamp(double value, double min, double max)
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;
            else
                return value;
        }

        protected Point3dCollection GetPointMatrix(Vector3d startDesloc, double yIncr, double xIncr)
        {
            var list = new Point3dCollection();
            var angle = ToRadians(90 - Properties.Algorythim.OrientationAngle);
            var startPt = Properties.StartPoint.Add(startDesloc.RotateBy(angle, Vector3d.ZAxis));
            var incrementVec = new Vector3d(xIncr, yIncr, 0).RotateBy(angle, Vector3d.ZAxis);

            for (var y = startPt.Y; y < Properties.MaxPoint.Y; y += incrementVec.Y)
                for (var x = startPt.X; x < Properties.MaxPoint.X; x += incrementVec.X)
                        list.Add(new Point3d(x, y, 0));

            return list;
        }

        public Point3dCollection GetCastPointList()
        {
            var result = new Point3dCollection();
            var selectedLp = Properties.Parts.SelectedLp;
            var selectedLd = Properties.Parts.SelectedLd;
            var selectedCast = Properties.Parts.SelectedCast;
            var spacing = Properties.Algorythim.Options.DistanceBetweenLpAndLd;
            var orientationAngle = 90 - Properties.Algorythim.OrientationAngle;

            var startDesloc = new Vector3d(selectedLp.Height + spacing, selectedLd.Height / 2.0D, 0);
            var xIncr = selectedLd.Width + 2 * spacing + selectedLp.Height;
            var yIncr = selectedCast.Width;
            var auxPts = GetPointMatrix(startDesloc, yIncr, xIncr);

            foreach (Point3d p in auxPts)
                for (var i = 0; i < Properties.CastGroupSize; i++)
                    result.Add(RotatePoint(p.X + i * selectedCast.Width, p.Y, 0, orientationAngle));

            return result;
        }

        public Point3dCollection GetLdPointList(bool ignoreLds)
        {
            var selectedLd = Properties.Parts.SelectedLd;
            var selectedLp = Properties.Parts.SelectedLp;
            var selectedCast = Properties.Parts.SelectedCast;
            var spacing = Properties.Algorythim.Options.DistanceBetweenLpAndLd;

            var startDesloc = new Vector3d(selectedLp.Height + spacing, 0, 0);
            var xIncr = selectedLd.Width + 2.0D * spacing + selectedLp.Height;
            var yIncr = selectedCast.Height;
            var points = GetPointMatrix(startDesloc, yIncr, xIncr);
            if (points.Count == 0 || !ignoreLds) return points;

            var firstPoint = points[0];
            var lastPoint = points[points.Count - 1];
            var result = new Point3dCollection();

            foreach (Point3d point in points)
                if (!CheckIfIsLDS(point, firstPoint, lastPoint))
                    result.Add(point);

            return result;
        }

        public Point3dCollection GetLpPointList()
        {
            var selectedLd = Properties.Parts.SelectedLd;
            var selectedLp = Properties.Parts.SelectedLp;
            var selectedStartLp = Properties.Algorythim.SelectedStartLp;
            var spacing = Properties.Algorythim.Options.DistanceBetweenLpAndLd;
            var useStartLp = Properties.Algorythim.SelectedStartLp != null;

            var startLpOffsetFix = selectedStartLp.Width + Clamp(selectedLp.Width - selectedStartLp.Width, 0, selectedLp.Width) + Properties.Algorythim.Options.DistanceBetweenLp;
            var offset = useStartLp ? selectedStartLp.StartOffset + startLpOffsetFix : selectedLp.StartOffset;
            var startDesloc = new Vector3d(0, offset, 0);
            var xIncr = selectedLp.Height + selectedLd.Width + spacing * 2.0D;
            var yIncr = selectedLp.Width + Properties.Algorythim.Options.DistanceBetweenLp;

            return GetPointMatrix(startDesloc, yIncr, xIncr);
        }

        public Point3dCollection GetStartLpPointList()
        {
            var selectedLd = Properties.Parts.SelectedLd;
            var selectedLp = Properties.Parts.SelectedLp;
            var selectedStartLp = Properties.Algorythim.SelectedStartLp;
            var spacing = Properties.Algorythim.Options.DistanceBetweenLpAndLd;

            var startDesloc = new Vector3d(0, selectedStartLp.StartOffset, 0);
            var xIncr = selectedLp.Height + selectedLd.Width + spacing * 2.0D;

            return GetPointMatrix(startDesloc, 0, xIncr);
        }

        public Point3dCollection GetHeadPointList(Part selectedHead)
        {
            var selectedLd = Properties.Parts.SelectedLd;
            var selectedLp = Properties.Parts.SelectedLp;
            var selectedCast = Properties.Parts.SelectedCast;
            var distBetweenLdAndLp = Properties.Algorythim.Options.DistanceBetweenLpAndLd;
            var startDesloc = new Vector3d(selectedLd.Width + distBetweenLdAndLp + (selectedLp.Height / 2.0D), 0, 0);
            var yIncr = selectedCast.Height;
            var xIncr = selectedLp.Height + distBetweenLdAndLp + selectedLd.Width;

            return GetPointMatrix(startDesloc, xIncr, yIncr);
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
            var pts = new Point3dCollection
            {
                new Point3d(-border, -border, 0),
                new Point3d(-border, part.Height + border, 0),
                new Point3d(part.Width + border, part.Height + border, 0),
                new Point3d(part.Width + border, -border, 0)
            };

            var polyline = new Polyline3d(Poly3dType.SimplePoly, pts, true);
            if (border > 0) polyline.Color = Color.FromRgb(255, 0, 0);

            return polyline;
        }

        public static bool IsInsidePolygon(Polyline polygon, Point3d pt)
        {
            var n = polygon.NumberOfVertices;
            double angle = 0;

            for (var i = 0; i < n; i++)
            {
                var pt1 = new Point2d(polygon.GetPoint2dAt(i).X - pt.X, polygon.GetPoint2dAt(i).Y - pt.Y);
                var pt2 = new Point2d(polygon.GetPoint2dAt((i + 1) % n).X - pt.X, polygon.GetPoint2dAt((i + 1) % n).Y - pt.Y);
                angle += GetAngle2D(pt1.X, pt1.Y, pt2.X, pt2.Y);
            }

            return !(Math.Abs(angle) < Math.PI);
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
            var dist = Properties.Parts.SelectedLp.Width + Properties.Algorythim.Options.DistanceBetweenLp;
            var orientation = Properties.Algorythim.SelectedOrientation;

            foreach (Point3d point in points)
            {
                if (point != current)
                {
                    var isBellow = (orientation == Orientation.Vertical && point.Y < current.Y)
                        || (orientation == Orientation.Horizontal && point.X > current.X);

                    if (isBellow && Math.Abs(current.DistanceTo(point) - dist) < 0.01f)
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

        public void FindBetterPartCombination(Part[] firstList, Part[] secondList, double distance, out Part firstPart, out Part secondPart)
        {
            firstPart = null;
            secondPart = null;
            var distanceToInterference = distance - Properties.Algorythim.Options.OutlineDistance;
            var delta = double.MaxValue;

            foreach (var part1 in firstList)
            {
                var tmpDelta = part1.Width - distanceToInterference;
                if (tmpDelta <= 0 && Math.Abs(tmpDelta) < delta)
                {
                    delta = Math.Abs(tmpDelta);
                    firstPart = part1;
                }

                foreach (var part2 in secondList)
                {
                    tmpDelta = (part1.Width + Properties.Algorythim.Options.DistanceBetweenLp + part2.Width) - distanceToInterference;
                    if (!(tmpDelta <= 0) || !(Math.Abs(tmpDelta) < delta)) continue;

                    delta = Math.Abs(tmpDelta);
                    firstPart = part1;
                    secondPart = part2;
                }
            }

        }

    }
}
