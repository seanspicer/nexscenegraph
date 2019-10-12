using System.Numerics;

namespace Veldrid.SceneGraph.Util.Shape
{
    public interface IPath : IShape
    {
        Vector3[] PathLocations { get; }
    }
    
    public class Path : IPath
    {
        private Vector3[] _pathLocations;

        public Vector3[] PathLocations => _pathLocations;
        
        public static IPath Create(Vector3[] pathLocations)
        {
            return new Path(pathLocations);
        }

        internal Path(Vector3[] pathLocations)
        {
            _pathLocations = pathLocations;
        }
        
        public void Accept(IShapeVisitor shapeVisitor)
        {
            shapeVisitor.Apply(this);
        }
    }
}