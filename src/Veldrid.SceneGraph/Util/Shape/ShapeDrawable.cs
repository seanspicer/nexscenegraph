using System.Numerics;

namespace Veldrid.SceneGraph.Util.Shape
{
    public interface IShapeDrawable<T> : IGeometry<T> where T : struct, ISettablePrimitiveElement
    {
        
    }
    
    public class ShapeDrawable<T> : Geometry<T>, IShapeDrawable<T> where T : struct, ISettablePrimitiveElement
    {
        private IShape _shape;
        private Vector3 _color;
        private ITessellationHints _hints;
        
        public static IShapeDrawable<T> Create(IShape shape, ITessellationHints hints)
        {
            return new ShapeDrawable<T>(shape, hints);
        }
        
        public static IShapeDrawable<T> Create(IShape shape, ITessellationHints hints, Vector3 color)
        {
            return new ShapeDrawable<T>(shape, hints, color);
        }
        
        internal ShapeDrawable(IShape shape, ITessellationHints hints)
        {
            SetColor(Vector3.One);
            SetShape(shape);
            SetTessellationHints(hints);
            Build();
        }
        
        internal ShapeDrawable(IShape shape, ITessellationHints hints, Vector3 color)
        {
            SetColor(color);
            SetShape(shape);
            SetTessellationHints(hints);
            Build();
        }

        private void SetShape(IShape shape)
        {
            _shape = shape;
        }

        private void SetColor(Vector3 color)
        {
            _color = color;
        }

        private void SetTessellationHints(ITessellationHints hints)
        {
            _hints = hints;
        }
        
        private void Build()
        {
            var shapeGeometryVisitor = new BuildShapeGeometryVisitor<T>(this, _hints, _color);
            _shape.Accept(shapeGeometryVisitor);
        }
    }
}