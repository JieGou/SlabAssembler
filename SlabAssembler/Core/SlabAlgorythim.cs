using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using Autodesk.AutoCAD.Customization;
using System.Collections.Generic;
using Urbbox.SlabAssembler.Core.Models;
using System.Drawing;
using Urbbox.SlabAssembler.Core.Variations;

namespace Urbbox.SlabAssembler.Core
{
    public class SlabAlgorythim
    {
        public SlabProperties Properties { get; set; }

        public SlabAlgorythim(SlabProperties prop)
        {
            Properties = prop;
        }

        public static bool CheckIfIsLDS(Point3d point, Point3d firstPoint, Point3d lastPoint, float orientationAngle)
        {
            bool isAtTheBeginningOrEndingOnVertical = (point.Y <= firstPoint.Y || point.Y >= lastPoint.Y) && (orientationAngle == 90);
            bool isAtTheBeginningEndingOnHorizontal = (point.X <= firstPoint.X || point.X >= lastPoint.X) && (orientationAngle == 0);

            return (isAtTheBeginningOrEndingOnVertical || isAtTheBeginningEndingOnHorizontal);
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
            var theta1 = Math.Atan2(y1, x1);
            var theta2 = Math.Atan2(y2, x2);
            var dtheta = theta2 - theta1;
            while (dtheta > Math.PI)
                dtheta -= (Math.PI * 2);
            while (dtheta < -Math.PI)
                dtheta += (Math.PI * 2);
            return dtheta;
        }

        public static double ToRadians(double angle)
        {
            return (angle * Math.PI) / 180F;
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

        public static Vector3d VectorFrom(double angle)
        {
            return RotatePoint(new Point3d(1, 0, 0), angle) - Point3d.Origin;
        }

        public static void FindBetterLpCombination(SlabProperties properties, Part[] firstList, Part[] secondList, double distance, out Part firstPart, out Part secondPart)
        {
            firstPart = null;
            secondPart = null;
            var distanceToInterference = distance - properties.Algorythim.Options.OutlineDistance;
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
                    tmpDelta = (part1.Width + properties.Algorythim.Options.DistanceBetweenLp + part2.Width) - distanceToInterference;
                    if (!(tmpDelta <= 0) || !(Math.Abs(tmpDelta) < delta)) continue;

                    delta = Math.Abs(tmpDelta);
                    firstPart = part1;
                    secondPart = part2;
                }
            }

        }
    }
}
