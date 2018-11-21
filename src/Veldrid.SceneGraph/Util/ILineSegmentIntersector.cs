

namespace Veldrid.SceneGraph.Util
{
    public interface ILineSegmentIntersector : IIntersector
    {
        SortedMultiSet<LineSegmentIntersector.Intersection> Intersections { get; }
    }
}