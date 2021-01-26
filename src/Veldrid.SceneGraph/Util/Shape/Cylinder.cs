

using System.Numerics;

namespace Veldrid.SceneGraph.Util.Shape
{
    public interface ICylinder : IShape
    {
        float Radius { get; }
        float Height { get; }
    }

    public class Cylinder : Shape, ICylinder
    {
        public float Radius { get; }

        public float Height { get; }

        public static ICylinder Create(Vector3 center, float radius, float height)
        {
            return new Cylinder(center, radius, height);
        }

        public static ICylinder CreateUnitCone()
        {
            return Create(Vector3.Zero, 0.5f, 1);
        }

        internal Cylinder(Vector3 center, float radius, float height)
        {
            Center = center;
            Radius = radius;
            Height = height;
        }

        public override void Accept(IShapeVisitor shapeVisitor)
        {
            shapeVisitor.Apply(this);
        }
    }
}