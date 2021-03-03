using System.Numerics;

namespace Veldrid.SceneGraph.Util.Shape
{
    public interface ICone : IShape
    {
        float Radius { get; }
        float Height { get; }
    }

    public class Cone : Shape, ICone
    {
        internal Cone(Vector3 center, float radius, float height)
        {
            Center = center;
            Radius = radius;
            Height = height;
        }

        public float Radius { get; }

        public float Height { get; }

        public override void Accept(IShapeVisitor shapeVisitor)
        {
            shapeVisitor.Apply(this);
        }

        public static ICone Create(Vector3 center, float radius, float height)
        {
            return new Cone(center, radius, height);
        }

        public static ICone CreateUnitCone()
        {
            return Create(Vector3.Zero, 0.5f, 1);
        }
    }
}