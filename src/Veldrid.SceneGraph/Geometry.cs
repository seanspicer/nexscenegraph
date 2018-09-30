//
// Copyright (c) 2018 Sean Spicer
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace Veldrid.SceneGraph
{
    internal class PrimitiveSetDrawGroup
    {
        public Pipeline Pipeline { get; private set; }
        public List<PrimitiveSet> PrimitveSets { get; } = new List<PrimitiveSet>();

        internal PrimitiveSetDrawGroup(Pipeline pipeline)
        {
            Pipeline = pipeline;
        }
    } 
    
    public class Geometry<T> : Drawable 
        where T : struct, IPrimitiveElement
    {
        public T[] VertexData { get; set; }
        public int SizeOfVertexData => Marshal.SizeOf(default(T));
        
        public ushort[] IndexData { get; set; }

        public VertexLayoutDescription VertexLayout { get; set; }

        public List<PrimitiveSet> PrimitiveSets { get; } = new List<PrimitiveSet>();
            
        private bool _dirtyFlag = true;
        
        private Dictionary<GraphicsDevice, DeviceBuffer> _vertexBufferCache 
            = new Dictionary<GraphicsDevice, DeviceBuffer>();
        
        private Dictionary<GraphicsDevice, DeviceBuffer> _indexBufferCache 
            = new Dictionary<GraphicsDevice, DeviceBuffer>();
        
        private Dictionary<GraphicsDevice, ResourceSet> _resourceSetCache 
            = new Dictionary<GraphicsDevice, ResourceSet>();

        private Dictionary<Tuple<GraphicsDevice, PrimitiveTopology>, Pipeline> _pipelineCache
            = new Dictionary<Tuple<GraphicsDevice, PrimitiveTopology>, Pipeline>();
        
        private Dictionary<GraphicsDevice, List<PrimitiveSetDrawGroup>> _primitiveSetDrawGroupCache 
            = new Dictionary<GraphicsDevice, List<PrimitiveSetDrawGroup>>();
        
        public Geometry()
        {
        }

        public override void ConfigureDeviceBuffers(GraphicsDevice device, ResourceFactory factory)
        {
            var vtxBufferDesc =
                new BufferDescription((uint) (VertexData.Length * SizeOfVertexData), BufferUsage.VertexBuffer);
            var vbo = factory.CreateBuffer(vtxBufferDesc);
            device.UpdateBuffer(vbo, 0, VertexData);

            var idxBufferDesc =
                new BufferDescription((uint) (IndexData.Length * sizeof(ushort)), BufferUsage.IndexBuffer);
            var ibo = factory.CreateBuffer(idxBufferDesc);
            device.UpdateBuffer(ibo, 0, IndexData);

            _vertexBufferCache.Add(device, vbo);
            _indexBufferCache.Add(device, ibo);
        }
        
        public override void ConfigurePipelinesForDevice(GraphicsDevice device, ResourceFactory factory, ResourceLayout parentLayout)
        {
            foreach (var primitiveSet in PrimitiveSets)
            {
                var key = Tuple.Create(device, primitiveSet.PrimitiveTopology);
                if (false == _pipelineCache.TryGetValue(key, out var pipeline))
                {
                    var resourceLayoutElementDescriptionList = new List<ResourceLayoutElementDescription> { };
                    var bindableResourceList = new List<BindableResource>();
                    
                    GraphicsPipelineDescription pd = new GraphicsPipelineDescription();
                    pd.PrimitiveTopology = primitiveSet.PrimitiveTopology;
                    
                    // TODO - this shouldn't be allocated here!
                    var modelMatrixBuffer = Matrix4x4.Identity;
                    var modelBuffer =
                        factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            
                    device.UpdateBuffer(modelBuffer, 0, modelMatrixBuffer);
                    // TODO - this shouldn't be allocated here!
            
                    resourceLayoutElementDescriptionList.Add(
                        new ResourceLayoutElementDescription("Model", ResourceKind.UniformBuffer, ShaderStages.Vertex));

                    bindableResourceList.Add(modelBuffer);

                    // Process Attached Textures
                    foreach (var tex2d in PipelineState.TextureList)
                    {
                        var deviceTexture =
                            tex2d.ProcessedTexture.CreateDeviceTexture(device, factory,
                                TextureUsage.Sampled);
                        var textureView =
                            factory.CreateTextureView(deviceTexture);
        
                        resourceLayoutElementDescriptionList.Add(
                            new ResourceLayoutElementDescription(tex2d.TextureName, ResourceKind.TextureReadOnly,
                                ShaderStages.Fragment)
                        );
                        resourceLayoutElementDescriptionList.Add(
                            new ResourceLayoutElementDescription(tex2d.SamplerName, ResourceKind.Sampler,
                                ShaderStages.Fragment)
                        );
        
                        bindableResourceList.Add(textureView);
                        bindableResourceList.Add(device.Aniso4xSampler);
                    }

                    var resourceLayout = factory.CreateResourceLayout(
                        new ResourceLayoutDescription(resourceLayoutElementDescriptionList.ToArray()));

                    var resourceSet = factory.CreateResourceSet(
                        new ResourceSetDescription(
                            resourceLayout,
                            bindableResourceList.ToArray()
                        )
                    );

                    pd.BlendState = PipelineState.BlendStateDescription;
                    pd.DepthStencilState = PipelineState.DepthStencilState;
                    pd.RasterizerState = PipelineState.RasterizerStateDescription;

                    if (null != PipelineState.VertexShaderDescription && null != PipelineState.FragmentShaderDescription)
                    {
                        var vertexShaderProg = factory.CreateShader(PipelineState.VertexShaderDescription.Value);
                        var fragmentShaderProg = factory.CreateShader(PipelineState.FragmentShaderDescription.Value);
        
                        pd.ShaderSet = new ShaderSetDescription(
                            vertexLayouts: new VertexLayoutDescription[] {VertexLayout},
                            shaders: new Shader[] {vertexShaderProg, fragmentShaderProg});
                    }

                    pd.ResourceLayouts = new[] {parentLayout, resourceLayout};

                    pd.Outputs = device.SwapchainFramebuffer.OutputDescription;

                    pipeline = factory.CreateGraphicsPipeline(pd);

                    _pipelineCache.Add(key, pipeline);
                    _resourceSetCache.Add(device, resourceSet);
                }
            }
        }

        protected override void DrawImplementation(GraphicsDevice device, List<Tuple<uint, ResourceSet>> resourceSets, CommandList commandList)
        {
            PrimitiveTopology? curTopology = null;
            foreach (var primitiveSet in PrimitiveSets)
            {
                if (false == curTopology.HasValue || curTopology.Value != primitiveSet.PrimitiveTopology)
                {
                    if(_pipelineCache.TryGetValue(Tuple.Create(device, primitiveSet.PrimitiveTopology), out var pipeline))
                    {
                        commandList.SetPipeline(pipeline);
                        
                        if (_vertexBufferCache.TryGetValue(device, out var vbo))
                        {
                            commandList.SetVertexBuffer(0, vbo);
                        }
            
                        if (_indexBufferCache.TryGetValue(device, out var ibo))
                        {
                            commandList.SetIndexBuffer(ibo, IndexFormat.UInt16);
                        }

                        foreach (var resourceSetInfo in resourceSets)
                        {
                            commandList.SetGraphicsResourceSet(resourceSetInfo.Item1, resourceSetInfo.Item2);
                        }

                        if (_resourceSetCache.TryGetValue(device, out var resourceSet))
                        {
                            commandList.SetGraphicsResourceSet(1, resourceSet);
                        }
                        
                        curTopology = primitiveSet.PrimitiveTopology;
                    }
                    else
                    {
                        throw new Exception("No pipeline defined for device for given topology on geometry");
                    }
                }
                
                primitiveSet.Draw(commandList);
            }
        }

        protected override BoundingBox ComputeBoundingBox()
        {
            var bb = new BoundingBox();
            foreach(var idx in IndexData)
            {
                bb.ExpandBy(VertexData[idx].VertexPosition);
            }

            return bb;
        }
    }
}