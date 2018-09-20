using System;
using System.Collections.Generic;
using System.Numerics;

namespace Veldrid.SceneGraph.RenderGraph
{
   
    public class RenderGroupState
    {
        public class RenderInfo
        {
            public Pipeline Pipeline;
            public ResourceLayout ResourceLayout;
            public ResourceSet ResourceSet;
            public DeviceBuffer ModelBuffer;
        } 
       
        private PipelineState PipelineState;
        private PrimitiveTopology PrimitiveTopology;
        private VertexLayoutDescription VertexLayout;

        public List<RenderGroupElement> Elements { get; } = new List<RenderGroupElement>();

        private Dictionary<Tuple<GraphicsDevice, ResourceFactory>, RenderInfo> RenderInfoCache;

        public RenderGroupState(PipelineState pso, PrimitiveTopology pt, VertexLayoutDescription vertexLayout)
        {
            PipelineState = pso;
            PrimitiveTopology = pt;
            VertexLayout = vertexLayout;
            RenderInfoCache = new Dictionary<Tuple<GraphicsDevice, ResourceFactory>, RenderInfo>();
        }

        public RenderInfo GetPipelineAndResources(GraphicsDevice graphicsDevice, ResourceFactory resourceFactory, ResourceLayout vpLayout)
        {
            // TODO Cache this by device, factory
            var key = new Tuple<GraphicsDevice, ResourceFactory>(graphicsDevice, resourceFactory);
            if (RenderInfoCache.TryGetValue(key, out var ri)) return ri;

            ri = new RenderInfo();
            
            var resourceLayoutElementDescriptionList = new List<ResourceLayoutElementDescription> { };
            var bindableResourceList = new List<BindableResource>();

            GraphicsPipelineDescription pd = new GraphicsPipelineDescription();
            pd.PrimitiveTopology = PrimitiveTopology;

            ri.ModelBuffer =
                resourceFactory.CreateBuffer(new BufferDescription(64,
                    BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            graphicsDevice.UpdateBuffer(ri.ModelBuffer, 0, Matrix4x4.Identity);

            resourceLayoutElementDescriptionList.Add(
                new ResourceLayoutElementDescription("Model", ResourceKind.UniformBuffer, ShaderStages.Vertex));

            bindableResourceList.Add(ri.ModelBuffer);

            // Process Attached Textures
            foreach (var tex2d in PipelineState.TextureList)
            {
                var deviceTexture =
                    tex2d.ProcessedTexture.CreateDeviceTexture(graphicsDevice, resourceFactory,
                        TextureUsage.Sampled);
                var textureView =
                    resourceFactory.CreateTextureView(deviceTexture);

                resourceLayoutElementDescriptionList.Add(
                    new ResourceLayoutElementDescription(tex2d.TextureName, ResourceKind.TextureReadOnly,
                        ShaderStages.Fragment)
                );
                resourceLayoutElementDescriptionList.Add(
                    new ResourceLayoutElementDescription(tex2d.SamplerName, ResourceKind.Sampler,
                        ShaderStages.Fragment)
                );

                bindableResourceList.Add(textureView);
                bindableResourceList.Add(graphicsDevice.Aniso4xSampler);
            }

            ri.ResourceLayout = resourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(resourceLayoutElementDescriptionList.ToArray()));

            ri.ResourceSet = resourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    ri.ResourceLayout,
                    bindableResourceList.ToArray()
                )
            );

            pd.BlendState = PipelineState.BlendStateDescription;
            pd.DepthStencilState = PipelineState.DepthStencilState;
            pd.RasterizerState = PipelineState.RasterizerStateDescription;

            if (null != PipelineState.VertexShader && null != PipelineState.FragmentShader &&
                null != PipelineState.VertexShaderEntryPoint && null != PipelineState.FragmentShaderEntryPoint)
            {
                var vertexShaderProg =
                    resourceFactory.CreateShader(
                        new ShaderDescription(ShaderStages.Vertex,
                            PipelineState.VertexShader,
                            PipelineState.VertexShaderEntryPoint
                        )
                    );

                var fragmentShaderProg =
                    resourceFactory.CreateShader(
                        new ShaderDescription(ShaderStages.Fragment,
                            PipelineState.FragmentShader,
                            PipelineState.FragmentShaderEntryPoint
                        )
                    );

                pd.ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new VertexLayoutDescription[] {VertexLayout},
                    shaders: new Shader[] {vertexShaderProg, fragmentShaderProg});
            }

            pd.ResourceLayouts = new[] {vpLayout, ri.ResourceLayout};

            pd.Outputs = graphicsDevice.SwapchainFramebuffer.OutputDescription;

            ri.Pipeline = resourceFactory.CreateGraphicsPipeline(pd);

            RenderInfoCache.Add(key, ri);
            
            return ri;
        }
    }
}

