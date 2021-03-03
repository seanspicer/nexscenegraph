using System.Linq;
using System.Numerics;

namespace Veldrid.SceneGraph.Util.Shape
{
    public interface IPath : IShape
    {
        Vector3[] PathLocations { get; }

        Matrix4x4 StaticTransform { get; }
    }

    public class Path : Shape, IPath
    {
        internal Path(Vector3[] pathLocations)
        {
            PathLocations = pathLocations;
            StaticTransform = Matrix4x4.Identity;
        }

        internal Path(Vector3[] pathLocations, Matrix4x4 staticTransform)
        {
            StaticTransform = staticTransform;
            PathLocations = pathLocations.Select(x => StaticTransform.PreMultiply(x)).ToArray();
        }

        public Vector3[] PathLocations { get; }

        public Matrix4x4 StaticTransform { get; }

        public override void Accept(IShapeVisitor shapeVisitor)
        {
            shapeVisitor.Apply(this);
        }

        public static IPath Create(Vector3[] pathLocations)
        {
            return new Path(pathLocations);
        }

        public static IPath Create(Vector3[] pathLocations, Matrix4x4 staticTransform)
        {
            return new Path(pathLocations, staticTransform);
        }
    }
}