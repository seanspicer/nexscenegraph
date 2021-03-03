using System.Numerics;

namespace Veldrid.SceneGraph.Util.Shape
{
    public interface IBox : IShape
    {
        Vector3 HalfLengths { get; }
    }

    public class Box : Shape, IBox
    {
        internal Box(Vector3 center, Vector3 halfLengths, Quaternion quaternion)
        {
            Center = center;
            HalfLengths = halfLengths;
            Rotation = quaternion;
        }

        public Vector3 HalfLengths { get; }

        public override void Accept(IShapeVisitor shapeVisitor)
        {
            shapeVisitor.Apply(this);
        }

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
            return Create(new Vector3(0f, 0f, 0f), new Vector3(0.5f, 0.5f, 0.5f));
        }
    }
}