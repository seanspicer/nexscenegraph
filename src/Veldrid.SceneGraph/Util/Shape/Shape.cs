
using System.Numerics;

namespace Veldrid.SceneGraph.Util.Shape
{
    public abstract class Shape : IShape
    {
        public virtual void Accept(IShapeVisitor shapeVisitor)
        {
        }

        public Vector3 Center { get; set; } = Vector3.Zero;
        
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
    }
}