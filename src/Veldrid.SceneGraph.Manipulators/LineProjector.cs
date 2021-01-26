
using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface ILineProjector : IProjector
    {
        ILineSegment LineSegment { get; }
        
        Vector3 LineStart { get; set; }
        Vector3 LineEnd { get; set; }
        
    }
    
    public class LineProjector : Projector, ILineProjector
    {
        public ILineSegment LineSegment { get; protected set; }

        public Vector3 LineStart
        {
            get => LineSegment.Start;
            set => LineSegment.Start = value;
        }

        public Vector3 LineEnd
        {
            get => LineSegment.End;
            set => LineSegment.End = value;
        }

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