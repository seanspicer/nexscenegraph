using System.Numerics;
using SharpDX.Mathematics.Interop;
using Veldrid.Sdl2;

namespace Veldrid.SceneGraph.Util.Shape
{
    public interface ISphere : IShape
    {
        Vector3 Center { get; }
        float Radius { get; }
    }
    
    public class Sphere : ISphere
    {
        private Vector3 _center;
        public Vector3 Center => _center;

        private float _radius;
        public float Radius => _radius;

        public static ISphere Create(Vector3 center, float radius)
        {
            return new Sphere(center, radius);
        }

        internal Sphere(Vector3 center, float radius)
        {
            _center = center;
            _radius = radius;
        }
        
        public void Accept(IShapeVisitor shapeVisitor)
        {
            shapeVisitor.Apply(this);
        }
    }
}