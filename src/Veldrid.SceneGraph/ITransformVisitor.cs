namespace Veldrid.SceneGraph
{
    public interface ITransformVisitor
    {
        void Accumulate(NodePath nodePath);
    }
}