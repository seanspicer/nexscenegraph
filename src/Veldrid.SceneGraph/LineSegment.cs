

using System.Numerics;

namespace Veldrid.SceneGraph
{
    public interface ILineSegment
    {
        public Vector3 Start { get; set; }
        public Vector3 End { get; set; }
    }
    
    public class LineSegment : ILineSegment
    {
        public Vector3 Start { get; set; }
        public Vector3 End { get; set; }
        
        public static ILineSegment Create(Vector3 start, Vector3 end)
        {
            return new LineSegment(start, end);
        }

        protected LineSegment(Vector3 start, Vector3 end)
        {
            Start = start;
            End = end;
        }
    }
}