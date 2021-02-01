

using System;
using System.Numerics;

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
            throw new NotImplementedException();
        }
    }
}