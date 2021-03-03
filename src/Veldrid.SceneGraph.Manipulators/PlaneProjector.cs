using System.Numerics;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IPlaneProjector : IProjector
    {
        IPlane Plane { get; }
    }

    public class PlaneProjector : Projector, IPlaneProjector
    {
        protected PlaneProjector(IPlane plane)
        {
            Plane = plane;
        }

        public IPlane Plane { get; protected set; }


        public override bool Project(IPointerInfo pi, out Vector3 projectedPoint)
        {
            var objectNearPoint = WorldToLocal.PreMultiply(pi.NearPoint);
            var objectFarPoint = WorldToLocal.PreMultiply(pi.FarPoint);

            return GetPlaneLineIntersection(
                new Vector4(Plane.Nx, Plane.Ny, Plane.Nz, Plane.D),
                objectNearPoint,
                objectFarPoint, out projectedPoint);
        }


        public static IPlaneProjector Create(IPlane plane)
        {
            return new PlaneProjector(plane);
        }
    }
}