using System.Numerics;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph
{
    public interface ILineSegment
    {
        public Vector3 Start { get; set; }
        public Vector3 End { get; set; }
        void PostMultiply(ILineSegment seg, Matrix4x4 matrix);
        void PreMultiply(Matrix4x4 matrix, ILineSegment seg);
    }

    public class LineSegment : ILineSegment
    {
        protected LineSegment(Vector3 start, Vector3 end)
        {
            Start = start;
            End = end;
        }

        public Vector3 Start { get; set; }
        public Vector3 End { get; set; }

        public void PostMultiply(ILineSegment seg, Matrix4x4 matrix)
        {
            Start = matrix.PreMultiply(seg.Start);
            End = matrix.PreMultiply(seg.End);
        }

        public void PreMultiply(Matrix4x4 matrix, ILineSegment seg)
        {
            Start = matrix.PostMultiply(seg.Start);
            End = matrix.PostMultiply(seg.End);
        }

        public static ILineSegment Create()
        {
            return new LineSegment(Vector3.Zero, Vector3.Zero);
        }

        public static ILineSegment Create(Vector3 start, Vector3 end)
        {
            return new LineSegment(start, end);
        }
    }
}