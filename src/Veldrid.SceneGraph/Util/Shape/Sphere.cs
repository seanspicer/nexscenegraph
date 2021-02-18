using System.Numerics;
using SharpDX.Mathematics.Interop;
using Veldrid.Sdl2;

namespace Veldrid.SceneGraph.Util.Shape
{
    public interface ISphere : IShape
    {
        float Radius { get; }
    }
    
    public class Sphere : Shape, ISphere
    {

        private float _radius;
        public float Radius => _radius;

        public static ISphere Create(Vector3 center, float radius)
        {
            return new Sphere(center, radius);
        }

        internal Sphere(Vector3 center, float radius)
        {
            Center = center;
            _radius = radius;
        }
        
        public override void Accept(IShapeVisitor shapeVisitor)
        {
            shapeVisitor.Apply(this);
        }
    }
}