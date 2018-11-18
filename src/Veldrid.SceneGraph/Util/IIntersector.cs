namespace Veldrid.SceneGraph.Util
{
    public interface IIntersector
    {
        Intersector.IntersectionLimitModes IntersectionLimit { get; set; }
        Intersector Clone(IIntersectionVisitor iv);
        void Intersect(IIntersectionVisitor iv, IDrawable drawable);
        bool Enter(INode node);
        void Leave();
        void Reset();
    }
}