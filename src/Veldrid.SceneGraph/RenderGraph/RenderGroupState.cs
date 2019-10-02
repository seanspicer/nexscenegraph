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
using Veldrid.SPIRV;
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

        public RenderInfo GetPipelineAndResources(GraphicsDevice graphicsDevice, ResourceFactory resourceFactory, ResourceLayout vpLayout, Framebuffer framebuffer)
        {
            // TODO Cache this by device, factory
            var key = new Tuple<GraphicsDevice, ResourceFactory>(graphicsDevice, resourceFactory);
            if (RenderInfoCache.TryGetValue(key, out var ri)) return ri;

            ri = new RenderInfo();
            
            var resourceLayoutElementDescriptionList = new List<ResourceLayoutElementDescription> { };
            var bindableResourceList = new List<BindableResource>();

            GraphicsPipelineDescription pd = new GraphicsPipelineDescription();
            pd.PrimitiveTopology = PrimitiveTopology;
            
            var nDrawables = (uint)Elements.Count;

            var alignment = graphicsDevice.UniformBufferMinOffsetAlignment;
            
            var multiplier = 64u;
            if (alignment > 64u)
            {
                multiplier = alignment;
            }
            
            ri.ModelViewBuffer =
                resourceFactory.CreateBuffer(new BufferDescription(multiplier*nDrawables, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            
            resourceLayoutElementDescriptionList.Add(
                new ResourceLayoutElementDescription("Model", ResourceKind.UniformBuffer, ShaderStages.Vertex, ResourceLayoutElementOptions.DynamicBinding));

            //bindableResourceList.Add(ri.ModelViewBuffer);
            bindableResourceList.Add(new DeviceBufferRange(ri.ModelViewBuffer, 0, multiplier));
            
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

            foreach (var uniform in PipelineState.UniformList)
            {
                uniform.ConfigureDeviceBuffers(graphicsDevice, resourceFactory);
                
                resourceLayoutElementDescriptionList.Add(uniform.ResourceLayoutElementDescription);
                
                bindableResourceList.Add(uniform.DeviceBufferRange);

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
                Shader[] shaders = resourceFactory.CreateFromSpirv(
                    PipelineState.VertexShaderDescription.Value,
                    PipelineState.FragmentShaderDescription.Value,
                    GetOptions(graphicsDevice, framebuffer)
                );
                
                Shader vs = shaders[0];
                Shader fs = shaders[1];

                pd.ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new VertexLayoutDescription[] {VertexLayout},
                    shaders: new Shader[] {vs, fs});
            }

            pd.ResourceLayouts = new[] {vpLayout, ri.ResourceLayout};

            pd.Outputs = framebuffer.OutputDescription;

            ri.Pipeline = resourceFactory.CreateGraphicsPipeline(pd);
            
            RenderInfoCache.Add(key, ri);
            
            return ri;
        }

        private static CrossCompileOptions GetOptions(GraphicsDevice gd, Framebuffer framebuffer)
        {
            SpecializationConstant[] specializations = GetSpecializations(gd, framebuffer);

            bool fixClipZ = (gd.BackendType == GraphicsBackend.OpenGL || gd.BackendType == GraphicsBackend.OpenGLES)
                            && !gd.IsDepthRangeZeroToOne;
            
            bool invertY = false;

            return new CrossCompileOptions(fixClipZ, invertY, specializations);
        }
        
        public static SpecializationConstant[] GetSpecializations(GraphicsDevice gd, Framebuffer framebuffer)
        {
            bool glOrGles = gd.BackendType == GraphicsBackend.OpenGL || gd.BackendType == GraphicsBackend.OpenGLES;

            List<SpecializationConstant> specializations = new List<SpecializationConstant>();
            specializations.Add(new SpecializationConstant(100, gd.IsClipSpaceYInverted));
            specializations.Add(new SpecializationConstant(101, glOrGles)); // TextureCoordinatesInvertedY
            specializations.Add(new SpecializationConstant(102, gd.IsDepthRangeZeroToOne));
            
            PixelFormat swapchainFormat = framebuffer.OutputDescription.ColorAttachments[0].Format;
            bool swapchainIsSrgb = swapchainFormat == PixelFormat.B8_G8_R8_A8_UNorm_SRgb
                                   || swapchainFormat == PixelFormat.R8_G8_B8_A8_UNorm_SRgb;
            specializations.Add(new SpecializationConstant(103, false));

            return specializations.ToArray();
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

