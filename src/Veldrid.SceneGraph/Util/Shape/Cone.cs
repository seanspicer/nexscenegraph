

using System.Numerics;

namespace Veldrid.SceneGraph.Util.Shape
{
    public interface ICone : IShape
    {
        Vector3 Center { get; }
        float Radius { get; }
        float Height { get; }
    }
    
    public class Cone : ICone
    {
        public Vector3 Center { get; }
        
        public float Radius { get; }
        
        public float Height { get; }

        public static ICone Create(Vector3 center, float radius, float height)
        {
            return new Cone(center, radius, height);
        }

        public static ICone CreateUnitCone()
        {
            return Create(Vector3.Zero,0.5f, 1);
        }
        
        internal Cone(Vector3 center, float radius, float height)
        {
            Center = center;
            Radius = radius;
            Height = height;
        }
        
        public void Accept(IShapeVisitor shapeVisitor)
        {
            shapeVisitor.Apply(this);
        }
        
    }
}