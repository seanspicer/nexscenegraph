using System.Numerics;

namespace Veldrid.SceneGraph.Util.Shape
{
    public interface IBox : IShape
    {
        Vector3 HalfLengths { get; }
    }
    
    public class Box : Shape, IBox
    {
        private Vector3 _halfLengths;
        public Vector3 HalfLengths => _halfLengths;
        
        public static IBox Create(Vector3 center, Vector3 halfLengths)
        {
            return new Box(center, halfLengths, Quaternion.Identity);
        }

        public static IBox Create(Vector3 center, float halfLength)
        {
            return new Box(center, new Vector3(halfLength, halfLength, halfLength), Quaternion.Identity);
        }
        
        public static IBox CreateUnitBox()
        {
            return Box.Create(new Vector3(0f, 0f, 0f), new Vector3(0.5f, 0.5f, 0.5f));
        }

        internal Box(Vector3 center, Vector3 halfLengths, Quaternion quaternion)
        {
            Center = center;
            _halfLengths = halfLengths;
            Rotation = quaternion;
        }

        public void Accept(IShapeVisitor shapeVisitor)
        {
            shapeVisitor.Apply(this);
        }
    }
}