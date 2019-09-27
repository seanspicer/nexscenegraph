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
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Windows.Controls;
//using Common.Logging;
using Veldrid.MetalBindings;
using Veldrid.SceneGraph.RenderGraph;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Wpf
{
    internal class Renderer : IGraphicsDeviceOperation
    {
        private IUpdateVisitor _updateVisitor;
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

        public Framebuffer Framebuffer { get; set; }
        
        //private ILog _logger;
        
        public Renderer(ICamera camera)
        {
            _camera = camera;
            _updateVisitor = UpdateVisitor.Create();
            _cullVisitor = CullVisitor.Create();
            Framebuffer = null;
            //_logger = LogManager.GetLogger<Renderer>();
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
            
            if (_camera.View.GetType() != typeof(Wpf.View))
            {
                throw new InvalidCastException("Camera View type is not correct");
            }
            var view = (Wpf.View) _camera.View;
            view.SceneData?.Accept(_cullVisitor);

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

            if (null == Framebuffer)
            {
                Framebuffer = device.SwapchainFramebuffer;
            }
            
            _initialized = true;
        }

        private void Update()
        {
            var view = (Wpf.View) _camera.View;
            view.SceneData?.Accept(_updateVisitor);
        }
        
        private void Cull(GraphicsDevice device, ResourceFactory factory)
        {    
            // Reset the visitor
            _cullVisitor.Reset();
            
            // Setup matrices
            _cullVisitor.SetViewMatrix(_camera.ViewMatrix);
            _cullVisitor.SetProjectionMatrix(_camera.ProjectionMatrix);
            
            // Prep
            _cullVisitor.Prepare();

            var view = (Wpf.View) _camera.View;
            view.SceneData?.Accept(_cullVisitor);
        }
        
        private void Record(GraphicsDevice device, ResourceFactory factory)
        {
            if (!_initialized)
            {
                Initialize(device, factory);
            }
            
            // Begin() must be called before commands can be issued.
            _commandList.Begin();

            // We want to render directly to the output window.
            _commandList.SetFramebuffer(Framebuffer);
            
            // TODO Set from Camera color ?
            _commandList.ClearColorTarget(0, RgbaFloat.Grey);
            _commandList.ClearDepthStencil(1f);
            
            //
            // Draw Opaque Geometry
            // 
            DrawOpaqueRenderGroups(device, factory);

            // 
            // Draw Transparent Geometry
            //
            if (_cullVisitor.TransparentRenderGroup.HasDrawableElements())
            {
                DrawTransparentRenderGroups(device, factory);
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

        private void DrawOpaqueRenderGroups(GraphicsDevice device, ResourceFactory factory)
        {
            var alignment = device.UniformBufferMinOffsetAlignment;
            var modelBuffStride = 64u;
            var hostBuffStride = 1u;
            if (alignment > 64u)
            {
                hostBuffStride = alignment / 64u;
                modelBuffStride = alignment;
            }
            
            foreach (var state in _cullVisitor.OpaqueRenderGroup.GetStateList())
            {
                var ri = state.GetPipelineAndResources(device, factory, _resourceLayout, Framebuffer);
                
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
                    var offset = (uint)i*modelBuffStride;
                    
                    _commandList.SetVertexBuffer(0, element.VertexBuffer);
                    
                    _commandList.SetIndexBuffer(element.IndexBuffer, IndexFormat.UInt32);
                    
                    _commandList.SetGraphicsResourceSet(0, _resourceSet);

                    _commandList.SetGraphicsResourceSet(1, ri.ResourceSet, 1, ref offset);
           
                    foreach (var primitiveSet in element.PrimitiveSets)
                    {
                        primitiveSet.Draw(_commandList);
                    }
                }
            }
        }
        
        private void DrawTransparentRenderGroups(GraphicsDevice device, ResourceFactory factory)
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
                        ri = state.GetPipelineAndResources(device, factory, _resourceLayout, Framebuffer);

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
            if (Framebuffer == device.SwapchainFramebuffer)
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

            // Run Update Traversal.
            Update();
            
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
            
//            _logger.Trace(m => m(string.Format("Update = {0} ms, Cull = {1} ms, Record = {2}, Draw = {3} ms, Swap = {4} ms",
//                postUpdate, 
//                postCull-postUpdate,
//                postRecord-postCull,
//                postDraw-postRecord,
//                postSwap-postDraw)));
        }
    }
}