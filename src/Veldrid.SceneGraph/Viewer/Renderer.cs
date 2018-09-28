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
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Veldrid.MetalBindings;
using Veldrid.SceneGraph.RenderGraph;
using Vulkan;

namespace Veldrid.SceneGraph.Viewer
{
    public class Renderer : IGraphicsDeviceOperation
    {
        private CullAndAssembleVisitor _cullAndAssembleVisitor;
        private Camera _camera;
        
        private DeviceBuffer _projectionBuffer;
        private DeviceBuffer _viewBuffer;
        private CommandList _commandList;
        private ResourceLayout _resourceLayout;
        private ResourceSet _resourceSet;
        
        private Polytope CullingFrustum { get; set; } = new Polytope();
        
        private bool _initialized = false;

        private RenderInfo _renderInfo;

        private int _culledObjectCount = 0;
        
        private Stopwatch _stopWatch = new Stopwatch();
        
        public Renderer(Camera camera)
        {
            _camera = camera;
            _cullAndAssembleVisitor = new CullAndAssembleVisitor();
        }

        private void Initialize(GraphicsDevice device, ResourceFactory factory)
        {
            _cullAndAssembleVisitor.GraphicsDevice = device;
            _cullAndAssembleVisitor.ResourceFactory = factory;
            
            _projectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _viewBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            
            
            _resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("View", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                
            ));

            _cullAndAssembleVisitor.ResourceLayout = _resourceLayout;
            //_cullAndAssembleVisitor.OpaqueRenderGroup.Clear();
            
            if (_camera.View.GetType() != typeof(Viewer.View))
            {
                throw new InvalidCastException("Camera View type is not correct");
            }
            //var view = (Viewer.View) _camera.View;
            //view.SceneData?.Accept(_cullAndAssembleVisitor);

            _resourceSet = factory.CreateResourceSet(
                new ResourceSetDescription(_resourceLayout, _projectionBuffer, _viewBuffer));
            
            _commandList = factory.CreateCommandList();
            
            _renderInfo = new RenderInfo();
            _renderInfo.GraphicsDevice = device;
            _renderInfo.ResourceFactory = factory;
            _renderInfo.CommandList = _commandList;
            _renderInfo.ResourceLayout = _resourceLayout;
            _renderInfo.ResourceSet = _resourceSet;
       
            _initialized = true;
        }
        
        private void Draw(GraphicsDevice device, ResourceFactory factory)
        {
            if (!_initialized)
            {
                Initialize(device, factory);
            }
            
            // TEST
            _cullAndAssembleVisitor.Reset();
            var view = (Viewer.View) _camera.View;
            view.SceneData?.Accept(_cullAndAssembleVisitor);
            // TEST
            
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
            if (_cullAndAssembleVisitor.OpaqueRenderGroup.HasDrawableElements())
            {
                DrawOpaqueRenderGroups(device, factory);
            }
            
            // 
            // Draw Transparent Geometry
            //
            if (_cullAndAssembleVisitor.TransparentRenderGroup.HasDrawableElements())
            {
                DrawTransparentRenderGroups(device, factory);
            }
            
            _commandList.End();
            
            _stopWatch.Reset();
            _stopWatch.Start();
            
            device.SubmitCommands(_commandList);

            var gpuTime = _stopWatch.ElapsedMilliseconds;
            Console.WriteLine("GPU = {0}", gpuTime);
        }

        private void DrawOpaqueRenderGroups(GraphicsDevice device, ResourceFactory factory)
        {
            var curModelMatrix = Matrix4x4.Identity;
            
            var opaqueRenderGroupStates = _cullAndAssembleVisitor.OpaqueRenderGroup.GetStateList();
            foreach (var state in opaqueRenderGroupStates)
            {
                var ri = state.GetPipelineAndResources(device, factory, _resourceLayout);
                
                // Set this state's pipeline
                _commandList.SetPipeline(ri.Pipeline);
                
                // Set the resources
                _commandList.SetGraphicsResourceSet(0, _resourceSet);
                
                // Set state-local resources
                _commandList.SetGraphicsResourceSet(1, ri.ResourceSet);
                
                // Iterate over all drawables in this state
                foreach (var renderElement in state.Elements)
                {
                    // TODO - Question: can this be done on a separate thread?
                    if (IsCulled(renderElement.Drawable.GetBoundingBox(), renderElement.ModelMatrix)) continue;
                   
                    // TODO - CASE 1 - use a vkCmdBindDescriptorSets equiv to bind the correct model matrix offset
                    
                    // Set vertex buffer
                    _commandList.SetVertexBuffer(0, renderElement.VertexBuffer.Item2);
                    
                    // Set index buffer
                    _commandList.SetIndexBuffer(renderElement.IndexBuffer.Item2, IndexFormat.UInt16); 
                    
                    // Draw the drawable
                    renderElement.Drawable.Draw(_commandList, renderElement.IndexBuffer.Item3, (int)renderElement.VertexBuffer.Item3);
                }
            }
        }
        
        private void DrawTransparentRenderGroups(GraphicsDevice device, ResourceFactory factory)
        {
            //Console.WriteLine("---- Frame ----");
            
            var curModelMatrix = Matrix4x4.Identity;

            _stopWatch.Reset();
            _stopWatch.Start();

            //
            // First sort the transparent render elements by distance to eye point (if not culled).
            //
            var drawOrderMap = new SortedList<float, List<Tuple<RenderGroupState, RenderGroupElement>>>();
            drawOrderMap.Capacity = _cullAndAssembleVisitor.RenderElementCount;
            var transparentRenderGroupStates = _cullAndAssembleVisitor.TransparentRenderGroup.GetStateList();
            foreach (var state in transparentRenderGroupStates)
            {
                var ri = state.GetPipelineAndResources(device, factory, _resourceLayout);

                // Iterate over all drawables in this state
                foreach (var renderElement in state.Elements)
                {
                    // TODO - Question: can this be done on a separate thread?
                    if (IsCulled(renderElement.Drawable.GetBoundingBox(), renderElement.ModelMatrix)) continue;

                    var ctr = renderElement.Drawable.GetBoundingBox().Center;
                    
                    // Compute distance eye point 
                    var modelView = renderElement.ModelMatrix.PostMultiply(_camera.ViewMatrix);
                    var ctr_w = Vector3.Transform(ctr, modelView);
                    var dist = Vector3.Distance(ctr_w, Vector3.Zero);

                    //Console.WriteLine("DrawElement => {0}, Ctr = {1}, Dist = {2}", renderElement.Drawable.NameString, ctr, dist);
                    
                    // Add this to a map
                    if (!drawOrderMap.TryGetValue(dist, out var renderList))
                    {
                        renderList = new List<Tuple<RenderGroupState, RenderGroupElement>>();
                        drawOrderMap.Add(dist, renderList);
                        
                    }
                    renderList.Add(Tuple.Create(state, renderElement));
                }
            }

            var sortTime = _stopWatch.ElapsedMilliseconds;

            var boundVertexBuffer = -1;
            var boundIndexBuffer = -1;
            
            // Now draw transparent elements, back to front
            RenderGroupState lastState = null;
            foreach (var renderList in drawOrderMap.Reverse())
            {
                foreach (var element in renderList.Value)
                {
                    var state = element.Item1;

                    if (null == lastState || state != lastState)
                    {
                        var ri = state.GetPipelineAndResources(device, factory, _resourceLayout);

                        // Set this state's pipeline
                        _commandList.SetPipeline(ri.Pipeline);

                        // Set the resources
                        _commandList.SetGraphicsResourceSet(0, _resourceSet);

                        // Set state-local resources
                        _commandList.SetGraphicsResourceSet(1, ri.ResourceSet);
                        
                        _commandList.UpdateBuffer(ri.ModelBuffer, 0, Matrix4x4.Identity);
                    }

                    var renderElement = element.Item2;

                    if (boundVertexBuffer != renderElement.VertexBuffer.Item1)
                    {
                        // Set vertex buffer
                        _commandList.SetVertexBuffer(0, renderElement.VertexBuffer.Item2);
                        boundVertexBuffer = renderElement.VertexBuffer.Item1;     
                    }

                    if (boundIndexBuffer != renderElement.IndexBuffer.Item1)
                    {
                        // Set index buffer
                        _commandList.SetIndexBuffer(renderElement.IndexBuffer.Item2, IndexFormat.UInt16);
                        boundIndexBuffer = renderElement.IndexBuffer.Item1;
                    }

                    // Draw the drawable
                    renderElement.Drawable.Draw(_commandList, renderElement.IndexBuffer.Item3, (int)renderElement.VertexBuffer.Item3);

                    //Console.WriteLine("DrawElement => {0}", renderElement.Drawable.NameString);
                    
                    lastState = state;
                }
            }

            var drawTime = _stopWatch.ElapsedMilliseconds;

            _stopWatch.Stop();
            
            Console.WriteLine("SortTime = {0} ms, RecordTime = {1} ms.", sortTime, drawTime-sortTime);
        }
        
        private bool IsCulled(BoundingBox bb, Matrix4x4 modelMatrix)
        {
            var culled = !CullingFrustum.Contains(bb, modelMatrix);

//            if (culled)
//            {
//                _culledObjectCount++;
//                Console.WriteLine("Culled Object {0}", _culledObjectCount);
//            }
            return culled;
        }

        private void UpdateUniforms(GraphicsDevice device, ResourceFactory factory)
        {
            if (!_initialized)
            {
                Initialize(device, factory);
            }
            
            device.UpdateBuffer(_projectionBuffer, 0, _camera.ProjectionMatrix);
            device.UpdateBuffer(_viewBuffer, 0, _camera.ViewMatrix);

            //  TODO - don't need both of these

            var vp = _camera.ViewMatrix.PostMultiply(_camera.ProjectionMatrix);
            _cullAndAssembleVisitor.SetCullingViewProjectionMatrix(vp);
            CullingFrustum.VPMatrix = vp;

        }

        private void SwapBuffers(GraphicsDevice device)
        {
            device.SwapBuffers();
        }

        public void HandleOperation(GraphicsDevice device, ResourceFactory factory)
        {
            UpdateUniforms(device, factory);
            Draw(device, factory);
            SwapBuffers(device);
        }
    }
}