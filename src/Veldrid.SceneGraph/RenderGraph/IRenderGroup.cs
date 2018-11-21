using System.Collections.Generic;

namespace Veldrid.SceneGraph.RenderGraph
{
    public interface IRenderGroup
    {
        bool HasDrawableElements();
        void Reset();
        IEnumerable<IRenderGroupState> GetStateList();
        IRenderGroupState GetOrCreateState(IPipelineState pso, PrimitiveTopology pt, VertexLayoutDescription vl);
    }
}