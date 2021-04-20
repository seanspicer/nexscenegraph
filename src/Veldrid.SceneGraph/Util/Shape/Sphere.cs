using System.Numerics;

namespace Veldrid.SceneGraph.Util.Shape
{
    public interface ISphere : IShape
    {
        float Radius { get; }
    }

    public class Sphere : Shape, ISphere
    {
        internal Sphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public float Radius { get; }

        public override void Accept(IShapeVisitor shapeVisitor)
        {
            shapeVisitor.Apply(this);
        }

        public static ISphere Create(Vector3 center, float radius)
        {
            return new Sphere(center, radius);
        }
    }
}