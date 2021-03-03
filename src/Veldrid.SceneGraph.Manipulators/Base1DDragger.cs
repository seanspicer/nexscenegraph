using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators
{
    public abstract class Base1DDragger : Dragger
    {
        protected Base1DDragger(Matrix4x4 matrix, bool usePhongShading) : this(new Vector3(-0.5f, 0.0f, 0.0f),
            new Vector3(0.5f, 0.0f, 0.0f), matrix, usePhongShading)
        {
        }

        protected Base1DDragger(Vector3 s, Vector3 e, Matrix4x4 matrix, bool usePhongShading) : base(matrix,
            usePhongShading)
        {
            LineProjector = Manipulators.LineProjector.Create(
                LineSegment.Create(s, e));
        }

        protected ILineProjector LineProjector { get; set; }
    }
}