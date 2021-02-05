

using System;
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
        public IPlane Plane { get; protected set; }
        
        
        public static IPlaneProjector Create(IPlane plane)
        {
            return new PlaneProjector(plane);
        }

        protected PlaneProjector(IPlane plane)
        {
            Plane = plane;
        }


        public override bool Project(IPointerInfo pi, out Vector3 projectedPoint)
        {
            var objectNearPoint = WorldToLocal.PreMultiply(pi.NearPoint);
            var objectFarPoint = WorldToLocal.PreMultiply(pi.FarPoint);

            return GetPlaneLineIntersection(
                new Vector4(Plane.Nx, Plane.Ny, Plane.Nz, Plane.D), 
                objectNearPoint,
                objectFarPoint, out projectedPoint);
        }

        public bool GetPlaneLineIntersection(Vector4 plane, Vector3 lineStart, Vector3 lineEnd, out Vector3 isect)
        {
            var deltaX = lineEnd.X - lineStart.X;
            var deltaY = lineEnd.Y - lineStart.Y;
            var deltaZ = lineEnd.Z - lineStart.Z;

            var denominator = (plane.X*deltaX + plane.Y*deltaY + plane.Z*deltaZ);
            if (0 == denominator)
            {
                isect = Vector3.Zero;
                return false;
            }

            var c = (plane.X*lineStart.X + plane.Y*lineStart.Y + plane.Z*lineStart.Z + plane.W) / denominator;

            isect = new Vector3(
                (float)(lineStart.X - deltaX * c),
                (float)(lineStart.Y - deltaY * c),
                (float)(lineStart.Z - deltaZ * c)
            );
            
            return true;
        }
    }
}