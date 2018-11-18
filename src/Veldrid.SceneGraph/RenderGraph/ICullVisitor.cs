using System.Numerics;

namespace Veldrid.SceneGraph.RenderGraph
{
    public interface ICullVisitor : INodeVisitor
    {
        IRenderGroup OpaqueRenderGroup { get; set; }
        IRenderGroup TransparentRenderGroup { get; set; }
        GraphicsDevice GraphicsDevice { get; set; }
        ResourceFactory ResourceFactory { get; set; }
        ResourceLayout ResourceLayout { get; set; }
        int RenderElementCount { get; }
        void Reset();
        void SetViewMatrix(Matrix4x4 viewMatrix);
        void SetProjectionMatrix(Matrix4x4 projectionMatrix);
        void Prepare();
    }
}