using System.Collections.Generic;

namespace Veldrid.SceneGraph.RenderGraph
{
    public class RenderGroupState
    {
        private PipelineState PipelineState;
        private Pipeline Pipeline;
        private ResourceLayout ResourceLayout;
        private ResourceSet ResourceSet;
        private DeviceBuffer ModelBuffer;

        public List<RenderGroupElement> Elements { get; } = new List<RenderGroupElement>();
        
        // Todo implement lazy update
        public RenderGroupState(PipelineState pso)
        {
            PipelineState = pso;
        }

        private void Update()
        {
            
        }
    }
}