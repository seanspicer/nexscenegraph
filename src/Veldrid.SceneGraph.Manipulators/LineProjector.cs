
using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface ILineProjector : IProjector
    {
        ILineSegment LineSegment { get; }
        
        Vector3 LineStart { get; }
        Vector3 LineEnd { get; }
        
    }
    
    public class LineProjector : Projector, ILineProjector
    {
        public ILineSegment LineSegment { get; protected set; }
        public Vector3 LineStart => LineSegment.Start;
        public Vector3 LineEnd => LineSegment.End;

        public static ILineProjector Create(ILineSegment lineSegment)
        {
            return new LineProjector(lineSegment);
        }

        protected LineProjector(ILineSegment lineSegment)
        {
            LineSegment = lineSegment;
        }
    }
}