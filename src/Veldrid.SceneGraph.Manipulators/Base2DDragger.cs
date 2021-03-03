using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators
{
    public abstract class Base2DDragger : Dragger
    {
        protected Base2DDragger(Matrix4x4 matrix, bool usePhongShading) : base(matrix, usePhongShading)
        {
            PlaneProjector = Manipulators.PlaneProjector.Create(
                Plane.Create(0.0f, 1.0f, 0.0f, 0.0f));
        }

        protected IPlaneProjector PlaneProjector { get; set; }
    }
}