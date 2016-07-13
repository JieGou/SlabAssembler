using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Urbbox.SlabAssembler.Core
{
    public class ScanLine
    {
        public static Point3dCollection GetOutlineSurroudingPointsX(Point3d startPoint, Point3d maxPoint, float step, Polyline outline)
        {
            var list = new Point3dCollection();

            for (double x = startPoint.X; x < maxPoint.X; x += step)
            {
                var point = new Point3d(x + step, startPoint.Y, 0);
                if (!SlabAlgorythim.IsInsidePolygon(outline, point))
                {
                    var hit = FirstHitX(startPoint, maxPoint, outline);
                    if (hit.HasValue) list.Add(hit.Value);

                    list.Add(new Point3d(x, startPoint.Y, 0));
                    list.Add(new Point3d(x - step, startPoint.Y, 0));
                    break;
                }
            }

            return list;
        }

        public static Point3d? FirstHitX(Point3d startPoint, Point3d maxPoint, Polyline outline)
        {
            for (double x = startPoint.X; x < maxPoint.X; x += 0.1)
            {
                var point = new Point3d(x + 0.1, startPoint.Y, 0);
                if (!SlabAlgorythim.IsInsidePolygon(outline, point))
                    return new Point3d(x, startPoint.Y, 0);
            }

            return null;
        }

        public static Point3dCollection GetOutlineSurroudingPointsY(Point3d startPoint, Point3d maxPoint, float step, Polyline outline)
        {
            var list = new Point3dCollection();

            for (double y = startPoint.Y; y < maxPoint.Y; y += step)
            {
                var point = new Point3d(startPoint.X, y + step, 0);
                if (!SlabAlgorythim.IsInsidePolygon(outline, point))
                {
                    var hit = FirstHitY(startPoint, maxPoint, outline);
                    if (hit.HasValue) list.Add(hit.Value);

                    list.Add(new Point3d(startPoint.X, y, 0));
                    list.Add(new Point3d(startPoint.X, y - step, 0));
                    break;
                }
            }

            return list;
        }

        public static Point3d? FirstHitY(Point3d startPoint, Point3d maxPoint, Polyline outline)
        {
            for (double y = startPoint.Y; y < maxPoint.Y; y += 0.1)
            {
                var point = new Point3d(startPoint.X, y + 0.1, 0);
                if (!SlabAlgorythim.IsInsidePolygon(outline, point))
                    return new Point3d(startPoint.X, y, 0);
            }

            return null;
        }
    }
}
