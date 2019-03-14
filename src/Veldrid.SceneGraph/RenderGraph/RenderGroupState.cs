//
// Copyright 2018 Sean Spicer 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.Utilities;

namespace Veldrid.SceneGraph.RenderGraph
{
    public class RenderInfo
    {
        public Pipeline Pipeline;
        public ResourceLayout ResourceLayout;
        public ResourceSet ResourceSet;
        public DeviceBuffer ModelViewBuffer;
    }
    
    public class RenderGroupState : IRenderGroupState
    {
        private IPipelineState PipelineState;
        private PrimitiveTopology PrimitiveTopology;
        private VertexLayoutDescription VertexLayout;

        public List<RenderGroupElement> Elements { get; } = new List<RenderGroupElement>();

        private Dictionary<Tuple<GraphicsDevice, ResourceFactory>, RenderInfo> RenderInfoCache;

        public static IRenderGroupState Create(IPipelineState pso, PrimitiveTopology pt, VertexLayoutDescription vertexLayout)
        {
            return new RenderGroupState(pso, pt, vertexLayout);
        }
        
        protected RenderGroupState(IPipelineState pso, PrimitiveTopology pt, VertexLayoutDescription vertexLayout)
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

//
// TODO - CASE 1 - implement this when Veldrid supports dynamic uniform buffers.
//
            var nDrawables = (uint)Elements.Count;
            //ri.ModelBuffer =
            //    resourceFactory.CreateBuffer(new BufferDescription(64*nDrawables, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            
            var modelMatrixViewBuffer = new Matrix4x4[nDrawables];
            for(var i=0; i<nDrawables; ++i)
            {
                modelMatrixViewBuffer[i] = Elements[i].ModelViewMatrix;
            }
            
            // TODO - this shouldn't be allocated here!
            //var modelMatrixBuffer = Matrix4x4.Identity;
            
            ri.ModelViewBuffer =
                resourceFactory.CreateBuffer(new BufferDescription(64*nDrawables, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            
            graphicsDevice.UpdateBuffer(ri.ModelViewBuffer, 0, modelMatrixViewBuffer);
            // TODO - this shouldn't be allocated here!
            
            resourceLayoutElementDescriptionList.Add(
                new ResourceLayoutElementDescription("Model", ResourceKind.UniformBuffer, ShaderStages.Vertex, ResourceLayoutElementOptions.DynamicBinding));

            //bindableResourceList.Add(ri.ModelViewBuffer);
            bindableResourceList.Add(new DeviceBufferRange(ri.ModelViewBuffer, 0, 64*nDrawables));

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

            if (null != PipelineState.VertexShaderDescription && null != PipelineState.FragmentShaderDescription)
            {
                var vertexShaderProg = resourceFactory.CreateShader(PipelineState.VertexShaderDescription.Value);
                var fragmentShaderProg = resourceFactory.CreateShader(PipelineState.FragmentShaderDescription.Value);

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

        public void ReleaseUnmanagedResources()
        {
            foreach (var entry in RenderInfoCache)
            {
                var key = entry.Key;
                var ri = entry.Value;

                var f = key.Item2 as DisposeCollectorResourceFactory;
                f.DisposeCollector.Remove(ri.Pipeline);
                f.DisposeCollector.Remove(ri.ResourceLayout);
                f.DisposeCollector.Remove(ri.ResourceSet);
                f.DisposeCollector.Remove(ri.ModelViewBuffer);
                
                ri.Pipeline.Dispose();
                ri.ResourceLayout.Dispose();
                ri.ResourceSet.Dispose();
                ri.ModelViewBuffer.Dispose();
            }
        }
    }


}

