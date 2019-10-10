namespace Veldrid.SceneGraph.Util.Shape
{
    public interface IShape
    {
        void Accept(IShapeVisitor shapeVisitor);
    }
}