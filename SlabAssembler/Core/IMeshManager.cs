using System;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;

namespace Urbbox.SlabAssembler.Core
{
    public interface IMeshManager : IDisposable
    {
        Task<Point3dCollection> CastList { get; }
        Task<Point3dCollection> HeadList { get; }
        Task<Point3dCollection> LdList { get; }
        Task<Point3dCollection> LdsList { get; }
        Task<Point3dCollection> LpList { get; }
        Task<Point3dCollection> StartLpList { get; }
        Task<Dictionary<Point3d, Point3dCollection>> EndLpList { get; }
    }
}