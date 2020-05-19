using System.Numerics;

namespace Veldrid.SceneGraph.Util.Shape
{
    public interface IBox : IShape
    {
        Vector3 Center { get; }
        Vector3 HalfLengths { get; }
        Quaternion Rotation { get; }
    }
    
    public class Box : IBox
    {
        private Vector3 _center;
        public Vector3 Center => _center;
        
        private Vector3 _halfLengths;
        public Vector3 HalfLengths => _halfLengths;
        
        private Quaternion _rotation;
        public Quaternion Rotation => _rotation;
        
        public static IBox Create(Vector3 center, Vector3 halfLengths)
        {
            return new Box(center, halfLengths, Quaternion.Identity);
        }

        public static IBox CreateUnitBox()
        {
            return Box.Create(new Vector3(0f, 0f, 0f), new Vector3(0.5f, 0.5f, 0.5f));
        }

        internal Box(Vector3 center, Vector3 halfLengths, Quaternion quaternion)
        {
            _center = center;
            _halfLengths = halfLengths;
            _rotation = quaternion;
        }

        public void Accept(IShapeVisitor shapeVisitor)
        {
            shapeVisitor.Apply(this);
        }
    }
}