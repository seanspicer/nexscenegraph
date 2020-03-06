//
// Copyright 2018-2019 Sean Spicer 
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
using Veldrid.SceneGraph.Shaders;
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
        public List<uint> UniformStrides;

        public RenderInfo()
        {
            UniformStrides = new List<uint>();
        }
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
            
            var modelViewMatrixObjSizeInBytes = 64u;
            if (alignment > 64u)
            {
                modelViewMatrixObjSizeInBytes = alignment;
            }
            
            ri.UniformStrides.Add(modelViewMatrixObjSizeInBytes);
            
            ri.ModelViewBuffer =
                resourceFactory.CreateBuffer(new BufferDescription(modelViewMatrixObjSizeInBytes*nDrawables, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            
            resourceLayoutElementDescriptionList.Add(
                new ResourceLayoutElementDescription("Model", ResourceKind.UniformBuffer, ShaderStages.Vertex, ResourceLayoutElementOptions.DynamicBinding));

            //bindableResourceList.Add(ri.ModelViewBuffer);
            bindableResourceList.Add(new DeviceBufferRange(ri.ModelViewBuffer, 0, modelViewMatrixObjSizeInBytes));
            
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

                if (uniform.ResourceLayoutElementDescription.Options == ResourceLayoutElementOptions.DynamicBinding)
                {
                    ri.UniformStrides.Add(uniform.DeviceBufferRange.SizeInBytes);
                }
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

            // TODO - cache based on the shader description and reuse shader objects
            if (null != PipelineState.ShaderSet)
            {
                (Shader vs, Shader fs) = GetShaders(graphicsDevice, framebuffer, PipelineState.ShaderSet);
                
                pd.ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new VertexLayoutDescription[] {VertexLayout},
                    shaders: new[] { vs, fs });
            }

            pd.ResourceLayouts = new[] {vpLayout, ri.ResourceLayout};

            pd.Outputs = framebuffer.OutputDescription;

            ri.Pipeline = resourceFactory.CreateGraphicsPipeline(pd);
            
            RenderInfoCache.Add(key, ri);
            
            return ri;
        }
        
        private static (Shader vs, Shader fs) LoadSPIRV(
            GraphicsDevice gd,
            ResourceFactory factory,
            Framebuffer fb,
            IShaderSet shaderSet)
        {
            Shader[] shaders = factory.CreateFromSpirv(
                shaderSet.VertexShaderDescription,
                shaderSet.FragmentShaderDescription,
                GetOptions(gd,fb));

            Shader vs = shaders[0];
            Shader fs = shaders[1];

            vs.Name = shaderSet.Name + "-Vertex";
            fs.Name = shaderSet.Name + "-Fragment";

            return (vs, fs);
        }

        private static readonly Dictionary<ShaderSetCacheKey, (Shader, Shader)> s_shaderSets
            = new Dictionary<ShaderSetCacheKey, (Shader, Shader)>();
        
        internal static (Shader vs, Shader fs) GetShaders(
            GraphicsDevice gd,
            Framebuffer framebuffer,
            IShaderSet shaderSet)
        {
            SpecializationConstant[] constants = GetSpecializations(gd, framebuffer);
            ShaderSetCacheKey cacheKey = new ShaderSetCacheKey(gd, shaderSet.Name, constants);
            if (!s_shaderSets.TryGetValue(cacheKey, out (Shader vs, Shader fs) set))
            {
                set = LoadSPIRV(gd, gd.ResourceFactory, framebuffer, shaderSet);
                s_shaderSets.Add(cacheKey, set);
            }

            return set;
        }
        
        private static CrossCompileOptions GetOptions(GraphicsDevice gd, Framebuffer framebuffer)
        {
            SpecializationConstant[] specializations = GetSpecializations(gd, framebuffer);

            bool fixClipZ = (gd.BackendType == GraphicsBackend.OpenGL || gd.BackendType == GraphicsBackend.OpenGLES)
                            && !gd.IsDepthRangeZeroToOne;
            
            bool invertY = false;

            return new CrossCompileOptions(fixClipZ, invertY, specializations);
        }
        
        private static SpecializationConstant[] GetSpecializations(GraphicsDevice gd, Framebuffer framebuffer)
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

