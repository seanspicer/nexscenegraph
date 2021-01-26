

using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators
{
    public abstract class Base2DDragger : Dragger 
    {
        protected IPlaneProjector PlaneProjector { get; set; }
        
        protected Base2DDragger(Matrix4x4 matrix) : base(matrix)
        {
            PlaneProjector = Veldrid.SceneGraph.Manipulators.PlaneProjector.Create(
                Plane.Create(0.0f, 1.0f, 0.0f, 0.0f));
        }
    }
}