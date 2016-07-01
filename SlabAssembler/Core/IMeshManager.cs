using System;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;

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
    }
}