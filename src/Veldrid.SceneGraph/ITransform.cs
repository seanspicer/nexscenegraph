using System.Numerics;

namespace Veldrid.SceneGraph
{
    public interface ITransform : IGroup
    {
        Transform.ReferenceFrameType ReferenceFrame { get; set; }
        bool ComputeLocalToWorldMatrix(ref Matrix4x4 matrix, NodeVisitor visitor);
        bool ComputeWorldToLocalMatrix(ref Matrix4x4 matrix, NodeVisitor visitor);
    }
}