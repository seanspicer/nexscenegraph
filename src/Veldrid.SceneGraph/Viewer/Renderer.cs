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
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
//using Common.Logging;
using Veldrid.MetalBindings;
using Veldrid.SceneGraph.Logging;
using Veldrid.SceneGraph.RenderGraph;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Viewer
{
    internal class Renderer : IGraphicsDeviceOperation
    {
        
        private ICullVisitor _cullVisitor;
        
        private ICamera _camera;
        
        private DeviceBuffer _projectionBuffer;
        private DeviceBuffer _viewBuffer;
        private CommandList _commandList;
        private ResourceLayout _resourceLayout;
        private ResourceSet _resourceSet;
        private Fence _fence;
        
        private bool _initialized = false;

        private RenderInfo _renderInfo;
        
        private Stopwatch _stopWatch = new Stopwatch();

        private List<Tuple<uint, ResourceSet>> _defaultResourceSets = new List<Tuple<uint, ResourceSet>>();

        private FullScreenQuadRenderer _fullScreenQuadRenderer;

        public SceneContext SceneContext { get; set; }
        
        private ILogger _logger;
        
        public Renderer(ICamera camera)
        {
            _camera = camera;
            _cullVisitor = CullVisitor.Create();
            _logger = LogManager.CreateLogger<Renderer>();
            _fullScreenQuadRenderer = new FullScreenQuadRenderer();
            SceneContext = null;
        }

        private void Initialize(GraphicsDevice device, ResourceFactory factory)
        {
            _cullVisitor.GraphicsDevice = device;
            _cullVisitor.ResourceFactory = factory;
            
            _projectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _viewBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            
            // TODO - combine view and projection matrices on host
            _resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("View", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));

            _cullVisitor.ResourceLayout = _resourceLayout;

            _resourceSet = factory.CreateResourceSet(
                new ResourceSetDescription(_resourceLayout, _projectionBuffer, _viewBuffer));
            
            _commandList = factory.CreateCommandList();
            
            _renderInfo = new RenderInfo();
            _renderInfo.GraphicsDevice = device;
            _renderInfo.ResourceFactory = factory;
            _renderInfo.CommandList = _commandList;
            _renderInfo.ResourceLayout = _resourceLayout;
            _renderInfo.ResourceSet = _resourceSet;

            _fence = factory.CreateFence(false);

            _defaultResourceSets.Add(Tuple.Create((uint)0, _resourceSet));
            
            _initialized = true;
        }

        private void Cull(GraphicsDevice device, ResourceFactory factory)
        {
            using (var mcv = _cullVisitor.ToMutable())
            {
                // Reset the visitor
                mcv.Reset();
            
                // Set the current camera
                mcv.SetCurrentCamera(_camera);
                
                // Prep
                mcv.Prepare();
            }
            
            if (_camera.View is Viewer.View viewerView)
            {
                viewerView.SceneData?.Accept(_cullVisitor);
            }
        }
        
        private void Record(GraphicsDevice device, ResourceFactory factory)
        {
            if (!_initialized)
            {
                Initialize(device, factory);
            }
            
            // Begin() must be called before commands can be issued.
            _commandList.Begin();

            if (SceneContext.MainSceneColorTexture.SampleCount == TextureSampleCount.Count1)
            {

                var framebuffer = SceneContext.OutputFramebuffer;
                
                // We want to render directly to the output window.
                _commandList.SetFramebuffer(framebuffer);

                _commandList.ClearColorTarget(0, _camera.ClearColor);
                _commandList.ClearDepthStencil(1f);

                //
                // Draw Opaque Geometry
                // 
                DrawOpaqueRenderGroups(device, factory, framebuffer);

                // 
                // Draw Transparent Geometry
                //
                if (_cullVisitor.TransparentRenderGroup.HasDrawableElements())
                {
                    DrawTransparentRenderGroups(device, factory, framebuffer);
                }

            }
            else
            {
                
                var framebuffer = SceneContext.MainSceneFramebuffer;

                // We want to render directly to the output window.
                _commandList.SetFramebuffer(SceneContext.MainSceneFramebuffer);

                _commandList.ClearColorTarget(0, _camera.ClearColor);
                _commandList.ClearDepthStencil(1f);

                //
                // Draw Opaque Geometry
                // 
                DrawOpaqueRenderGroups(device, factory, framebuffer);

                // 
                // Draw Transparent Geometry
                //
                if (_cullVisitor.TransparentRenderGroup.HasDrawableElements())
                {
                    DrawTransparentRenderGroups(device, factory, framebuffer);
                }
                
                //
                // Resolve the texture
                //
                _commandList.ResolveTexture(
                    SceneContext.MainSceneColorTexture, 
                    SceneContext.MainSceneResolvedColorTexture); 

                // Draw Resolved Texture to full screen quad
                _commandList.SetFramebuffer(SceneContext.OutputFramebuffer);
            
                _commandList.SetFullViewports();
            
                _fullScreenQuadRenderer.Render(device, _commandList, SceneContext);
            }
            
            _commandList.End();
        }

        private void Draw(GraphicsDevice device)
        {
            device.ResetFence(_fence);
            device.SubmitCommands(_commandList, _fence);
            device.WaitForFence(_fence);
            device.WaitForIdle();
        }

        private void DrawOpaqueRenderGroups(GraphicsDevice device, ResourceFactory factory, Framebuffer framebuffer)
        {
            var alignment = device.UniformBufferMinOffsetAlignment;
            var modelViewMatrixObjSizeInBytes = 64u;
            var hostBuffStride = 1u;
            if (alignment > 64u)
            {
                hostBuffStride = alignment / 64u;
                modelViewMatrixObjSizeInBytes = alignment;
            }
            
            foreach (var state in _cullVisitor.OpaqueRenderGroup.GetStateList())
            {
                if (state.Elements.Count == 0) continue;
                var ri = state.GetPipelineAndResources(device, factory, _resourceLayout, framebuffer);
                
                _commandList.SetPipeline(ri.Pipeline);
                
                var nDrawables = (uint)state.Elements.Count;
                var modelMatrixViewBuffer = new Matrix4x4[nDrawables*hostBuffStride]; // TODO - do we need to allocate this every frame?
                for(var i=0; i<nDrawables; ++i)
                {
                    modelMatrixViewBuffer[i*hostBuffStride] = state.Elements[i].ModelViewMatrix;
                }
                _commandList.UpdateBuffer(ri.ModelViewBuffer, 0, modelMatrixViewBuffer);
                
                for(var i=0; i<nDrawables; ++i)
                {
                    var element = state.Elements[i];
                    var offsetsList = new List<uint>();

                    foreach (var stride in ri.UniformStrides)
                    {
                        offsetsList.Add((uint)i*stride);
                    }
                    
                    var offsets = offsetsList.ToArray();
                    
                    _commandList.SetVertexBuffer(0, element.VertexBuffer);
                    
                    _commandList.SetIndexBuffer(element.IndexBuffer, IndexFormat.UInt32);
                    
                    _commandList.SetGraphicsResourceSet(0, _resourceSet);

                    _commandList.SetGraphicsResourceSet(1, ri.ResourceSet, offsets);
           
                    foreach (var primitiveSet in element.PrimitiveSets)
                    {
                        primitiveSet.Draw(_commandList);
                    }
                }
            }
        }
        
        private void DrawTransparentRenderGroups(GraphicsDevice device, ResourceFactory factory, Framebuffer framebuffer)
        {
            var alignment = device.UniformBufferMinOffsetAlignment;
            var modelBuffStride = 64u;
            var hostBuffStride = 1u;
            if (alignment > 64u)
            {
                hostBuffStride = alignment / 64u;
                modelBuffStride = alignment;
            }
            
            //
            // First sort the transparent render elements by distance to eye point (if not culled).
            //
            var drawOrderMap = new SortedList<float, List<Tuple<IRenderGroupState, RenderGroupElement, IPrimitiveSet,uint>>>();
            drawOrderMap.Capacity = _cullVisitor.RenderElementCount;
            var transparentRenderGroupStates = _cullVisitor.TransparentRenderGroup.GetStateList();
            
            var stateToUniformDict = new Dictionary<IRenderGroupState, Matrix4x4[]>();
            
            foreach (var state in transparentRenderGroupStates)
            {
                var nDrawables = (uint)state.Elements.Count;
                var modelMatrixViewBuffer = new Matrix4x4[nDrawables*hostBuffStride];
                
                // Iterate over all elements in this state
                for(var j=0; j<nDrawables; ++j)
                {
                    var renderElement = state.Elements[j];
                    modelMatrixViewBuffer[j*hostBuffStride] = state.Elements[j].ModelViewMatrix;
                    
                    // Iterate over all primitive sets in this state
                    foreach (var pset in renderElement.PrimitiveSets)
                    {
                        var ctr = pset.GetBoundingBox().Center;

                        // Compute distance eye point 
                        var modelView = renderElement.ModelViewMatrix;
                        var ctr_w = Vector3.Transform(ctr, modelView);
                        var dist = Vector3.Distance(ctr_w, Vector3.Zero);

                        if (!drawOrderMap.TryGetValue(dist, out var renderList))
                        {
                            renderList = new List<Tuple<IRenderGroupState, RenderGroupElement, IPrimitiveSet, uint>>();
                            drawOrderMap.Add(dist, renderList);
                        }

                        renderList.Add(Tuple.Create(state, renderElement, pset, (uint)j));
                    }
                }
                stateToUniformDict.Add(state, modelMatrixViewBuffer);
            }

            DeviceBuffer boundVertexBuffer = null;
            DeviceBuffer boundIndexBuffer = null;
            
            // Now draw transparent elements, back to front
            IRenderGroupState lastState = null;
            RenderGraph.RenderInfo ri = null;

            var currModelViewMatrix = Matrix4x4.Identity;
            
            foreach (var renderList in drawOrderMap.Reverse())
            {
                foreach (var element in renderList.Value)
                {
                    var state = element.Item1;

                    if (null == lastState || state != lastState)
                    {
                        ri = state.GetPipelineAndResources(device, factory, _resourceLayout, framebuffer);

                        // Set this state's pipeline
                        _commandList.SetPipeline(ri.Pipeline);

                        _commandList.UpdateBuffer(ri.ModelViewBuffer, 0, stateToUniformDict[state]);
                        
                        // Set the resources
                        _commandList.SetGraphicsResourceSet(0, _resourceSet);
                    }

                    uint offset = element.Item4*modelBuffStride;
                        
                    // Set state-local resources
                    _commandList.SetGraphicsResourceSet(1, ri.ResourceSet, 1, ref offset);                    
                    
                    var renderGroupElement = element.Item2;

                    if (boundVertexBuffer != renderGroupElement.VertexBuffer)
                    {
                        // Set vertex buffer
                        _commandList.SetVertexBuffer(0, renderGroupElement.VertexBuffer);
                        boundVertexBuffer = renderGroupElement.VertexBuffer;     
                    }

                    if (boundIndexBuffer != renderGroupElement.IndexBuffer)
                    {
                        // Set index buffer
                        _commandList.SetIndexBuffer(renderGroupElement.IndexBuffer, IndexFormat.UInt32);
                        boundIndexBuffer = renderGroupElement.IndexBuffer;
                    }
                    
                    element.Item3.Draw(_commandList);
                   
                    lastState = state;
                }
            }
        }

        private void UpdateUniforms(GraphicsDevice device, ResourceFactory factory)
        {
            if (!_initialized)
            {
                Initialize(device, factory);
            }
            
            device.UpdateBuffer(_projectionBuffer, 0, _camera.ProjectionMatrix);
            
            // TODO - Remove
            device.UpdateBuffer(_viewBuffer, 0, Matrix4x4.Identity);


        }

        private void SwapBuffers(GraphicsDevice device)
        {
            if (SceneContext.OutputFramebuffer == device.SwapchainFramebuffer)
            {
                device.SwapBuffers();
            }
        }

        public void HandleOperation(GraphicsDevice device, ResourceFactory factory)
        {
            // TODO - this doesn't work on Metal
            if (null != _fence)
            {
                device.WaitForFence(_fence);
            }
            
            if (!_initialized)
            {
                Initialize(device, factory);
            }
            
            _stopWatch.Reset();
            _stopWatch.Start();

            UpdateUniforms(device, factory);

            var postUpdate = _stopWatch.ElapsedMilliseconds;
            
            Cull(device, factory);
            
            var postCull = _stopWatch.ElapsedMilliseconds;
            
            Record(device, factory);
            
            var postRecord = _stopWatch.ElapsedMilliseconds;

            Draw(device);

            var postDraw = _stopWatch.ElapsedMilliseconds;
            
            SwapBuffers(device);
            
            var postSwap = _stopWatch.ElapsedMilliseconds;
            
            _logger?.LogTrace(string.Format("UpdateUniforms = {0} ms, Cull = {1} ms, Record = {2}, Draw = {3} ms, Swap = {4} ms",
                postUpdate, 
                postCull-postUpdate,
                postRecord-postCull,
                postDraw-postRecord,
                postSwap-postDraw));
        }

        public void HandleResize(GraphicsDevice device)
        {
            _fullScreenQuadRenderer.CreateDeviceObjects(device, SceneContext);
        }
    }
}