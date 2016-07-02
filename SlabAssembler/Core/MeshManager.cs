using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Threading.Tasks;

namespace Urbbox.SlabAssembler.Core
{
    public class MeshManager : IDisposable, IMeshManager
    {
        public Task<Point3dCollection> CastList { get; private set; }
        public Task<Point3dCollection> LpList { get; private set; }
        public Task<Point3dCollection> EndLpList { get; private set; }
        public Task<Point3dCollection> StartLpList { get; private set; }
        public Task<Point3dCollection> LdList { get; private set; }
        public Task<Point3dCollection> HeadList { get; private set; }
        public Task<Point3dCollection> LdsList { get; private set; }

        private readonly SlabProperties _properties;
        private readonly double _globalOrientation;
        private readonly Polyline _outline;

        public MeshManager(SlabProperties properties, Polyline ouline)
        {
            _properties = properties;
            _outline = ouline;
            _globalOrientation = -ToRadians(90 - _properties.Algorythim.OrientationAngle);

            if (!properties.Algorythim.OnlyCimbrament)
                CastList = Task.Factory.StartNew(() => InitializeCastMesh());

            if (properties.Algorythim.SelectedStartLp != null)
                StartLpList = Task.Factory.StartNew(() => InitializeStartLpMesh());

            LpList = Task.Factory.StartNew(() => InitializeLpMesh());
            //EndLpList = Task.Factory.StartNew(() => InitializeEndLpMesh());

            if (properties.Algorythim.Options.UseLds)
                LdsList = Task.Factory.StartNew(() => InitializeLdsMesh());

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
            var startPoint = _properties.StartPoint.Add(startVector);
            var incrVector = new Vector2d(ld.Width + spacing * 2.0 + lp.Height, cast.Width);

            for (double y = startPoint.Y; y < _properties.MaxPoint.Y; y += incrVector.Y)
            {
                for (double x = startPoint.X; x < _properties.MaxPoint.X; x += incrVector.X)
                {
                    list.Add(new Point3d(x, y, 0));
                }
            }

            return list;
        }

        private Point3dCollection InitializeLdsMesh()
        {
            var list = new Point3dCollection();
            var ld = _properties.Parts.SelectedLd;
            var lp = _properties.Parts.SelectedLp;
            var cast = _properties.Parts.SelectedCast;
            var spacing = _properties.Algorythim.Options.DistanceBetweenLpAndLd;
            var startVector = new Vector3d(lp.Height + spacing, 0, 0);
            var startPoint = _properties.StartPoint.Add(startVector);
            var incrVector = new Vector2d(ld.Width + spacing * 2.0 + lp.Height, cast.Width);

            for (double x = startPoint.X; x < _properties.MaxPoint.X; x += incrVector.X)
            {
                list.Add(new Point3d(x, startPoint.Y, 0));
                double y = 0;
                for (y = startPoint.Y; y < _properties.MaxPoint.Y && SlabAlgorythim.IsInsidePolygon(_outline, new Point3d(x, y + incrVector.Y, 0)); y += incrVector.Y);

                if (SlabAlgorythim.IsInsidePolygon(_outline, new Point3d(x, y + _properties.Algorythim.Options.OutlineDistance / 2.0, 0)))
                {
                    if (SlabAlgorythim.IsInsidePolygon(_outline, new Point3d(x + cast.Width, y + _properties.Algorythim.Options.OutlineDistance / 2.0, 0)))
                        list.Add(new Point3d(x, y, 0));
                    else
                        list.Add(new Point3d(x, y - incrVector.Y, 0));
                }
                else
                    list.Add(new Point3d(x, y - incrVector.Y, 0));
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
            var startVector = new Vector3d(lp.Height / 2.0, ld.Height / 2.0, 0);
            var startPoint = _properties.StartPoint.Add(startVector);
            var useStartLp = _properties.Algorythim.SelectedStartLp != null;
            var incrVect = new Vector2d(ld.Width + spacing * 2.0 + lp.Height, cast.Width);

            for (double y = startPoint.Y; y < _properties.MaxPoint.Y; y += incrVect.Y)
            {
                for (double x = startPoint.X; x < _properties.MaxPoint.X; x += incrVect.X)
                {
                    if (_properties.Algorythim.Options.UseEndLp && (y + cast.Width) >= _properties.MaxPoint.Y)
                        continue;

                    if (useStartLp && y <= startPoint.Y)
                        continue;

                    list.Add(new Point3d(x, y, 0));
                }

            }

            return list;
        }

        private Point3dCollection InitializeStartLpMesh()
        {
            var list = new Point3dCollection();
            var ld = _properties.Parts.SelectedLd;
            var lp = _properties.Parts.SelectedLp;
            var startLp = _properties.Algorythim.SelectedStartLp;
            var cast = _properties.Parts.SelectedCast;
            var spacing = _properties.Algorythim.Options.DistanceBetweenLpAndLd;
            var startVector = new Vector3d(0, startLp?.StartOffset ?? _properties.Parts.SelectedLp.StartOffset, 0);
            var startPoint = _properties.StartPoint.Add(startVector);
            var incrVect = new Vector2d(ld.Width + lp.Height + spacing * 2.0, lp.Width + _properties.Algorythim.Options.DistanceBetweenLp);

            double x = 0;
            for (x = startPoint.X; x < _properties.MaxPoint.X; x += incrVect.X)
                list.Add(new Point3d(x, startPoint.Y, 0));

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
            var spacing = _properties.Algorythim.Options.DistanceBetweenLpAndLd;
            var startVector = new Vector3d(0, startLp?.StartOffset ?? _properties.Parts.SelectedLp.StartOffset, 0);
            var startPoint = _properties.StartPoint.Add(startVector);
            var incrVect = new Vector2d(ld.Width + lp.Height + spacing * 2.0, lp.Width + _properties.Algorythim.Options.DistanceBetweenLp);

            if (useStartLp)
                startPoint = startPoint.Add(new Vector3d(0, startLp.Width + _properties.Algorythim.Options.DistanceBetweenLp, 0));

            for (double y = startPoint.Y; y < _properties.MaxPoint.Y; y += incrVect.Y)
            {
                for (double x = startPoint.X; x < _properties.MaxPoint.X; x += incrVect.X)
                {
                    list.Add(new Point3d(x, y, 0));
                }
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
            var startPt = _properties.StartPoint.Add(startVector);
            var incrVector = new Vector2d(cast.Width, cast.Height);
            var countX = 0;
            var countY = 0;

            for (var y = startPt.Y; y < _properties.MaxPoint.Y; y += incrVector.Y)
            {
                countX = 0;
                for (var x = startPt.X; x < _properties.MaxPoint.X; x += incrVector.X)
                {
                    if (countX > 0 && countX % _properties.CastGroupSize == 0 )
                        x += spaceBetweenGroups;

                    list.Add(new Point3d(x, y, 0));
                    countX++;
                }

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
                    CastList?.Dispose();
                    LdsList?.Dispose();
                    StartLpList?.Dispose();
                    LpList?.Dispose();
                    LdList?.Dispose();
                    HeadList?.Dispose();
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
