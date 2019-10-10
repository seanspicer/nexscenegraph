namespace Veldrid.SceneGraph.Util.Shape
{
    public interface IShapeVisitor
    {
        void Apply(IShape shape);

        void Apply(IBox box);

        void Apply(ISphere sphere);
    }
}