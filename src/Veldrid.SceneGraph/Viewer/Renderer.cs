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
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Veldrid.MetalBindings;
using Veldrid.SceneGraph.RenderGraph;

namespace Veldrid.SceneGraph.Viewer
{
    public class Renderer : IGraphicsDeviceOperation
    {
        private CullVisitor _cullVisitor;
        private Camera _camera;
        
        private DeviceBuffer _projectionBuffer;
        private DeviceBuffer _viewBuffer;
        private CommandList _commandList;
        private ResourceLayout _resourceLayout;
        private ResourceSet _resourceSet;

        private bool _initialized = false;

        private RenderInfo _renderInfo;

        private int _culledObjectCount = 0;
        
        private Stopwatch _stopWatch = new Stopwatch();

        private List<Tuple<uint, ResourceSet>> _defaultResourceSets = new List<Tuple<uint, ResourceSet>>();
        
        public Renderer(Camera camera)
        {
            _camera = camera;
            _cullVisitor = new CullVisitor();
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
            
            if (_camera.View.GetType() != typeof(Viewer.View))
            {
                throw new InvalidCastException("Camera View type is not correct");
            }
            var view = (Viewer.View) _camera.View;
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

            //_fence = factory.CreateFence(false);

            _defaultResourceSets.Add(Tuple.Create((uint)0, _resourceSet));
            
            _initialized = true;
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

            var view = (Viewer.View) _camera.View;
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
            _commandList.SetFramebuffer(device.SwapchainFramebuffer);
            
            // TODO Set from Camera color ?
            _commandList.ClearColorTarget(0, RgbaFloat.Grey);
            _commandList.ClearDepthStencil(1f);

            
            _culledObjectCount = 0;
            
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
            // TODO - this doesn't work on Metal
            //device.ResetFence(_fence);
            
            device.SubmitCommands(_commandList);
            device.WaitForIdle();
        }

        private void DrawOpaqueRenderGroups(GraphicsDevice device, ResourceFactory factory)
        {
            var currModelViewMatrix = Matrix4x4.Identity;
            foreach (var state in _cullVisitor.OpaqueRenderGroup.GetStateList())
            {
                var ri = state.GetPipelineAndResources(device, factory, _resourceLayout);
                
                _commandList.SetPipeline(ri.Pipeline);
                
                foreach (var element in state.Elements)
                {
                    _commandList.SetVertexBuffer(0, element.VertexBuffer);
                    
                    _commandList.SetIndexBuffer(element.IndexBuffer, IndexFormat.UInt16);
                    
                    _commandList.SetGraphicsResourceSet(0, _resourceSet);
                    
                    _commandList.SetGraphicsResourceSet(1, ri.ResourceSet);
                    
                    // TODO Optimize with uniform buffer later on
                    if (element.ModelViewMatrix != currModelViewMatrix)
                    {
                        _commandList.UpdateBuffer(ri.ModelViewBuffer, 0, element.ModelViewMatrix);
                        currModelViewMatrix = element.ModelViewMatrix;
                    }
                    
                    foreach (var primitiveSet in element.PrimitiveSets)
                    {
                        primitiveSet.Draw(_commandList);
                    }
                }
            }
        }
        
        private void DrawTransparentRenderGroups(GraphicsDevice device, ResourceFactory factory)
        {
            //
            // First sort the transparent render elements by distance to eye point (if not culled).
            //
            var drawOrderMap = new SortedList<float, List<Tuple<RenderGroupState, RenderGroupElement, PrimitiveSet>>>();
            drawOrderMap.Capacity = _cullVisitor.RenderElementCount;
            var transparentRenderGroupStates = _cullVisitor.TransparentRenderGroup.GetStateList();
            foreach (var state in transparentRenderGroupStates)
            {
                // Iterate over all elements in this state
                foreach (var renderElement in state.Elements)
                {
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
                            renderList = new List<Tuple<RenderGroupState, RenderGroupElement, PrimitiveSet>>();
                            drawOrderMap.Add(dist, renderList);
                        }

                        renderList.Add(Tuple.Create(state, renderElement, pset));
                    }
                }
            }

            DeviceBuffer boundVertexBuffer = null;
            DeviceBuffer boundIndexBuffer = null;
            
            // Now draw transparent elements, back to front
            RenderGroupState lastState = null;
            RenderGroupState.RenderInfo ri = null;

            var currModelViewMatrix = Matrix4x4.Identity;
            
            foreach (var renderList in drawOrderMap.Reverse())
            {
                foreach (var element in renderList.Value)
                {
                    var state = element.Item1;

                    if (null == lastState || state != lastState)
                    {
                        ri = state.GetPipelineAndResources(device, factory, _resourceLayout);

                        // Set this state's pipeline
                        _commandList.SetPipeline(ri.Pipeline);

                        // Set the resources
                        _commandList.SetGraphicsResourceSet(0, _resourceSet);

                        // Set state-local resources
                        _commandList.SetGraphicsResourceSet(1, ri.ResourceSet);
                    }

                    if (element.Item2.ModelViewMatrix != currModelViewMatrix)
                    {
                        _commandList.UpdateBuffer(ri.ModelViewBuffer, 0, element.Item2.ModelViewMatrix);
                        currModelViewMatrix = element.Item2.ModelViewMatrix;
                    }
                    
                    
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
                        _commandList.SetIndexBuffer(renderGroupElement.IndexBuffer, IndexFormat.UInt16);
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
            device.SwapBuffers();
        }

        public void HandleOperation(GraphicsDevice device, ResourceFactory factory)
        {
            // TODO - this doesn't work on Metal
            //if (null != _fence)
            //{
            //    device.WaitForFence(_fence);
            //}
            
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
            
            Console.WriteLine("Update = {0} ms, Cull = {1} ms, Record = {2}, Draw = {3} ms, Swap = {4} ms",
                postUpdate, 
                postCull-postUpdate,
                postRecord-postCull,
                postDraw-postRecord,
                postSwap-postDraw);
        }
    }
}