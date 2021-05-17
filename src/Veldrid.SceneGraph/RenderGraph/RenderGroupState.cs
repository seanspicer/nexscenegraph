//
// Copyright 2018-2021 Sean Spicer 
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
using System.Text;
using Veldrid.SceneGraph.Shaders;
using Veldrid.SPIRV;
using Veldrid.Utilities;

namespace Veldrid.SceneGraph.RenderGraph
{
    public class RenderInfo
    {
        public uint HostBufferStride;
        public DeviceBuffer ModelViewBuffer;
        public Matrix4x4[] ModelViewMatrixBuffer;
        public uint ModelViewMatrixObjSizeInBytes;
        public List<uint[]> OffsetArrayList;
        public Pipeline Pipeline;
        public ResourceLayout ResourceLayout;
        public ResourceSet ResourceSet;

        public RenderInfo()
        {
            OffsetArrayList = new List<uint[]>();
        }
    }

    public interface IRenderGroupState
    {
        List<RenderGroupElement> Elements { get; }
        IPipelineState PipelineState { get; }

        RenderInfo GetPipelineAndResources(
            GraphicsDevice graphicsDevice,
            ResourceFactory resourceFactory,
            ResourceLayout vpLayout,
            Framebuffer framebuffer);

        void ReleaseUnmanagedResources();
    }

    public class RenderGroupState : IRenderGroupState
    {
        private static readonly Dictionary<ShaderSetCacheKey, (Shader, Shader)> s_shaderSets
            = new Dictionary<ShaderSetCacheKey, (Shader, Shader)>();

        private readonly PrimitiveTopology PrimitiveTopology;

        private readonly Dictionary<Tuple<GraphicsDevice, ResourceFactory>, RenderInfo> RenderInfoCache;
        private readonly List<VertexLayoutDescription> VertexLayouts;

        protected RenderGroupState(IPipelineState pso, PrimitiveTopology pt,
            List<VertexLayoutDescription> vertexLayouts)
        {
            PipelineState = pso;
            PrimitiveTopology = pt;
            VertexLayouts = vertexLayouts;
            RenderInfoCache = new Dictionary<Tuple<GraphicsDevice, ResourceFactory>, RenderInfo>();
        }

        public IPipelineState PipelineState { get; }

        public List<RenderGroupElement> Elements { get; } = new List<RenderGroupElement>();

        public RenderInfo GetPipelineAndResources(GraphicsDevice graphicsDevice, ResourceFactory resourceFactory,
            ResourceLayout vpLayout, Framebuffer framebuffer)
        {
            var alignment = graphicsDevice.UniformBufferMinOffsetAlignment;
            var hostBufferStride = 1u;
            var modelViewMatrixObjSizeInBytes = 64u;
            if (alignment > 64u)
            {
                hostBufferStride = alignment / 64u;
                modelViewMatrixObjSizeInBytes = alignment;
            }

            var nDrawables = (uint) Elements.Count;

            // TODO Cache this by device, factory
            var key = new Tuple<GraphicsDevice, ResourceFactory>(graphicsDevice, resourceFactory);
            if (RenderInfoCache.TryGetValue(key, out var ri))
            {
                if (ri.ModelViewBuffer.SizeInBytes == nDrawables * modelViewMatrixObjSizeInBytes)
                    return ri;
                // Number of drawables has changed!
                RenderInfoCache.Remove(key);
            }

            ri = new RenderInfo();
            ri.HostBufferStride = hostBufferStride;
            ri.ModelViewMatrixObjSizeInBytes = modelViewMatrixObjSizeInBytes;
            ri.ModelViewMatrixBuffer = new Matrix4x4[nDrawables * hostBufferStride];

            var resourceLayoutElementDescriptionList = new List<ResourceLayoutElementDescription>();
            var bindableResourceList = new List<BindableResource>();

            var pd = new GraphicsPipelineDescription();
            pd.PrimitiveTopology = PrimitiveTopology;

            var UniformStrides = new List<uint>();
            UniformStrides.Add(modelViewMatrixObjSizeInBytes);

            ri.ModelViewBuffer =
                resourceFactory.CreateBuffer(new BufferDescription(modelViewMatrixObjSizeInBytes * nDrawables,
                    BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            resourceLayoutElementDescriptionList.Add(
                new ResourceLayoutElementDescription("Model", ResourceKind.UniformBuffer, ShaderStages.Vertex,
                    ResourceLayoutElementOptions.DynamicBinding));

            //bindableResourceList.Add(ri.ModelViewBuffer);
            bindableResourceList.Add(
                new DeviceBufferRange(ri.ModelViewBuffer, 0, modelViewMatrixObjSizeInBytes*nDrawables));


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
                var sampler = resourceFactory.CreateSampler(tex2d.SamplerDescription);
                bindableResourceList.Add(sampler);
                
                
            }


            foreach (var uniform in PipelineState.UniformList)
            {
                uniform.ConfigureDeviceBuffers(graphicsDevice, resourceFactory);

                resourceLayoutElementDescriptionList.Add(uniform.ResourceLayoutElementDescription);

                var deviceBufferRange = uniform.GetDeviceBufferRange(graphicsDevice);
                bindableResourceList.Add(deviceBufferRange);

                if (uniform.ResourceLayoutElementDescription.Options == ResourceLayoutElementOptions.DynamicBinding)
                    UniformStrides.Add(deviceBufferRange.SizeInBytes);
            }

            foreach (var vertexBuffer in PipelineState.VertexBufferList)
                vertexBuffer.ConfigureDeviceBuffers(graphicsDevice, resourceFactory);

            ri.ResourceLayout = resourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(resourceLayoutElementDescriptionList.ToArray()));

            ri.ResourceSet = resourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    ri.ResourceLayout,
                    bindableResourceList.ToArray()
                )
            );

            for (var i = 0; i < nDrawables; ++i)
            {
                var element = Elements[i];
                var offsetsList = new List<uint>();

                foreach (var stride in UniformStrides) offsetsList.Add((uint) i * stride);

                var offsets = offsetsList.ToArray();
                ri.OffsetArrayList.Add(offsets);
            }


            pd.BlendState = PipelineState.BlendStateDescription;
            pd.DepthStencilState = PipelineState.DepthStencilState;
            pd.RasterizerState = PipelineState.RasterizerStateDescription;

            // TODO - cache based on the shader description and reuse shader objects
            if (null != PipelineState.ShaderSet)
            {
                (var vs, var fs) = GetShaders(graphicsDevice, framebuffer, PipelineState.ShaderSet);

                pd.ShaderSet = new ShaderSetDescription(
                    VertexLayouts.ToArray(),
                    new[] {vs, fs});
            }

            pd.ResourceLayouts = new[] {vpLayout, ri.ResourceLayout};

            pd.Outputs = framebuffer.OutputDescription;

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

        public static IRenderGroupState Create(IPipelineState pso, PrimitiveTopology pt,
            List<VertexLayoutDescription> vertexLayouts)
        {
            return new RenderGroupState(pso, pt, vertexLayouts);
        }

        private static (Shader vs, Shader fs) LoadSPIRV(
            GraphicsDevice gd,
            ResourceFactory factory,
            Framebuffer fb,
            IShaderSet shaderSet)
        {
            Shader vs = null;
            Shader fs = null;

            // We are in D3D11 land...do this so we can get 
            // the information we need to debug shaders in 
            // HLSL in RenderDoc.
            if (gd.GetD3D11Info(out var info))
            {
                var result = SpirvCompilation.CompileVertexFragment(
                    shaderSet.VertexShaderDescription.ShaderBytes,
                    shaderSet.FragmentShaderDescription.ShaderBytes,
                    CrossCompileTarget.HLSL,
                    GetOptions(gd, fb));

                var vertexShaderDescription = new ShaderDescription(
                    ShaderStages.Vertex,
                    Encoding.ASCII.GetBytes(result.VertexShader),
                    "main", shaderSet.VertexShaderDescription.Debug);

                var fragmentShaderDescription = new ShaderDescription(
                    ShaderStages.Fragment,
                    Encoding.ASCII.GetBytes(result.FragmentShader),
                    "main", shaderSet.FragmentShaderDescription.Debug);

                vs = factory.CreateShader(vertexShaderDescription);
                fs = factory.CreateShader(fragmentShaderDescription);
            }

            // We are in Vulkan or other land
            else
            {
                var shaders = factory.CreateFromSpirv(
                    shaderSet.VertexShaderDescription,
                    shaderSet.FragmentShaderDescription,
                    GetOptions(gd, fb));

                vs = shaders[0];
                fs = shaders[1];
            }

            vs.Name = shaderSet.Name + "-Vertex";
            fs.Name = shaderSet.Name + "-Fragment";

            return (vs, fs);
        }

        internal static (Shader vs, Shader fs) GetShaders(
            GraphicsDevice gd,
            Framebuffer framebuffer,
            IShaderSet shaderSet)
        {
            var constants = GetSpecializations(gd, framebuffer);
            var cacheKey = new ShaderSetCacheKey(gd, shaderSet.Name, constants);
            if (!s_shaderSets.TryGetValue(cacheKey, out (Shader vs, Shader fs) set))
            {
                set = LoadSPIRV(gd, gd.ResourceFactory, framebuffer, shaderSet);
                s_shaderSets.Add(cacheKey, set);
            }

            return set;
        }

        private static CrossCompileOptions GetOptions(GraphicsDevice gd, Framebuffer framebuffer)
        {
            var specializations = GetSpecializations(gd, framebuffer);

            var fixClipZ = (gd.BackendType == GraphicsBackend.OpenGL || gd.BackendType == GraphicsBackend.OpenGLES)
                           && !gd.IsDepthRangeZeroToOne;

            var invertY = false;

            return new CrossCompileOptions(fixClipZ, invertY, specializations);
        }

        private static SpecializationConstant[] GetSpecializations(GraphicsDevice gd, Framebuffer framebuffer)
        {
            var glOrGles = gd.BackendType == GraphicsBackend.OpenGL || gd.BackendType == GraphicsBackend.OpenGLES;

            var specializations = new List<SpecializationConstant>();
            specializations.Add(new SpecializationConstant(100, gd.IsClipSpaceYInverted));
            specializations.Add(new SpecializationConstant(101, glOrGles)); // TextureCoordinatesInvertedY
            specializations.Add(new SpecializationConstant(102, gd.IsDepthRangeZeroToOne));

            var swapchainFormat = framebuffer.OutputDescription.ColorAttachments[0].Format;
            var swapchainIsSrgb = swapchainFormat == PixelFormat.B8_G8_R8_A8_UNorm_SRgb
                                  || swapchainFormat == PixelFormat.R8_G8_B8_A8_UNorm_SRgb;
            specializations.Add(new SpecializationConstant(103, false));

            return specializations.ToArray();
        }
    }
}