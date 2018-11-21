using System.Numerics;

namespace Veldrid.SceneGraph.Util
{
    public interface IIntersectionVisitor : INodeVisitor
    {
        Matrix4x4 GetModelMatrix();
    }
}