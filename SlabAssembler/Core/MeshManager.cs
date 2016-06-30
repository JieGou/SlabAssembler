using Autodesk.AutoCAD.Geometry;
using System;
using System.Threading.Tasks;

namespace Urbbox.SlabAssembler.Core
{
    public class MeshManager : IDisposable
    {
        public Task<Point3dCollection> CastList { get; private set; }
        public Task<Point3dCollection> LpTask { get; private set; }
        public Task<Point3dCollection> LdList { get; private set; }
        public Task<Point3dCollection> HeadList { get; private set; }

        private readonly SlabProperties _properties;
        private readonly double _globalOrientation;

        public MeshManager(SlabProperties properties)
        {
            _properties = properties;
            _globalOrientation = -ToRadians(90 - _properties.Algorythim.OrientationAngle);
            CastList = Task.Factory.StartNew(() => InitializeCastMesh());
            LpTask = Task.Factory.StartNew(() => InitializeLpMesh());
            LdList = Task.Factory.StartNew(() => InitializeLdMesh());
            HeadList = Task.Factory.StartNew(() => InitializeHeadMesh());
        }

        private static double ToRadians(float degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        private Point3dCollection InitializeLdMesh()
        {
            var list = new Point3dCollection();
            var ld = _properties.Parts.SelectedLd;
            var lp = _properties.Parts.SelectedLp;
            var cast = _properties.Parts.SelectedCast;
            var spacing = _properties.Algorythim.Options.DistanceBetweenLpAndLd;
            var startVector = new Vector3d(lp.Height + spacing, 0, 0);
            var startPoint = _properties.StartPoint.Add(startVector.RotateBy(_globalOrientation, Vector3d.ZAxis));
            var incrVector = new Vector2d(ld.Width + spacing * 2.0 + lp.Height, cast.Width).RotateBy(_globalOrientation);

            for (double y = startPoint.Y; y < _properties.MaxPoint.Y; y += incrVector.Y)
            {
                for (double x = startPoint.X; x < _properties.MaxPoint.X; x += incrVector.X)
                {
                    list.Add(new Point3d(x, y, 0));
                }
            }

            return list;
        }

        private Point3dCollection InitializeHeadMesh()
        {
            var list = new Point3dCollection();
            var ld = _properties.Parts.SelectedLd;
            var lp = _properties.Parts.SelectedLp;
            var cast = _properties.Parts.SelectedCast;
            var spacing = _properties.Algorythim.Options.DistanceBetweenLpAndLd;
            var startVector = new Vector3d(lp.Height / 2.0, ld.Height / 2.0, 0).RotateBy(_globalOrientation, Vector3d.ZAxis);
            var startPoint = _properties.StartPoint.Add(startVector);
            var useStartLp = _properties.Algorythim.SelectedStartLp != null;
            var incrVect = new Vector2d(ld.Width + spacing * 2.0 + lp.Height, cast.Width).RotateBy(_globalOrientation);

            for (double y = startPoint.Y; y < _properties.MaxPoint.Y; y += incrVect.Y)
            {
                for (double x = startPoint.X; x < _properties.MaxPoint.X; x += incrVect.X)
                {
                    if (_properties.Algorythim.OrientationAngle == 90
                        && _properties.Algorythim.Options.UseEndLp && (y + cast.Width) >= _properties.MaxPoint.Y)
                        continue;

                    if (_properties.Algorythim.OrientationAngle == 90
                        && useStartLp && y <= startPoint.Y)
                        continue;

                    if (_properties.Algorythim.OrientationAngle == 0
                        && _properties.Algorythim.Options.UseEndLp && (x + cast.Width) >= _properties.MaxPoint.X)
                        continue;

                    if (_properties.Algorythim.OrientationAngle == 0
                        && useStartLp && x <= startPoint.X)
                        continue;

                    list.Add(new Point3d(x, y, 0));
                }

            }

            return list;
        }

        private Point3dCollection InitializeLpMesh()
        {
            var list = new Point3dCollection();
            var ld = _properties.Parts.SelectedLd;
            var lp = _properties.Parts.SelectedLp;
            var startLp = _properties.Algorythim.SelectedStartLp;
            var useStartLp = startLp != null;
            var cast = _properties.Parts.SelectedCast;
            var yOffset = startLp?.StartOffset ?? _properties.Parts.SelectedLp.StartOffset;
            var spacing = _properties.Algorythim.Options.DistanceBetweenLpAndLd;
            var countY = 0;

            for (double y = _properties.StartPoint.Y + yOffset; y < _properties.MaxPoint.Y; y += lp.Width)
            {
                if (useStartLp && countY == 0) y += startLp.Width;

                for (double x = _properties.StartPoint.X; x < _properties.MaxPoint.X; x += cast.Width * _properties.CastGroupSize + lp.Height + spacing * 2.0)
                {
                    list.Add(new Point3d(x, y, 0).RotateBy(_globalOrientation, Vector3d.ZAxis, Point3d.Origin));
                }

                if (useStartLp && countY == 0) y += _properties.Algorythim.Options.DistanceBetweenLp - lp.Width;
                countY++;
            }

            return list;
        }

        private Point3dCollection InitializeCastMesh()
        {
            var list = new Point3dCollection();
            var ld = _properties.Parts.SelectedLd;
            var lp = _properties.Parts.SelectedLp;
            var cast = _properties.Parts.SelectedCast;
            var spacing = _properties.Algorythim.Options.DistanceBetweenLpAndLd;
            var startVector = new Vector3d(lp.Height + spacing, ld.Height / 2.0, 0);
            var spaceBetweenGroups = lp.Height + 2.0 * spacing;
            var startPt = _properties.StartPoint.Add(startVector.RotateBy(_globalOrientation, Vector3d.ZAxis));
            var incrVector = new Vector2d(cast.Width, cast.Height).RotateBy(_globalOrientation);
            var countX = 0;
            var countY = 0;

            for (var y = startPt.Y; y < _properties.MaxPoint.Y; y += incrVector.Y)
            {
                countX = 0;
                for (var x = startPt.X; x < _properties.MaxPoint.X; x += incrVector.X)
                {
                    if (countX > 0 && countX % _properties.CastGroupSize == 0 && _properties.Algorythim.OrientationAngle == 90)
                        x += spaceBetweenGroups;

                    list.Add(new Point3d(x, y, 0));
                    countX++;
                }

                if (countY > 0 && countY % _properties.CastGroupSize == 0 && _properties.Algorythim.OrientationAngle == 0)
                    y += spaceBetweenGroups;

                countY++;
            }

            return list;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    CastList.Dispose();
                    LpTask.Dispose();
                    LdList.Dispose();
                    HeadList.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


    }
}
