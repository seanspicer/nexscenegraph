using System.Collections.Generic;

namespace Veldrid.SceneGraph.RenderGraph
{
    public interface IRenderGroupState
    {
        List<RenderGroupElement> Elements { get; }
        RenderInfo GetPipelineAndResources(GraphicsDevice graphicsDevice, ResourceFactory resourceFactory, ResourceLayout vpLayout);
        void ReleaseUnmanagedResources();
    }
}