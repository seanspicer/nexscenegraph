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
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Microsoft.Extensions.Logging;
using Veldrid.SceneGraph.Logging;
using Veldrid.SceneGraph.RenderGraph;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Viewer
{
    internal class Renderer : IGraphicsDeviceOperation
    {
        private readonly ICullVisitor _cullVisitor;

        private readonly IUpdateVisitor _updateVisitor;

        private readonly ICamera _camera;
        private CommandList _commandList;

        private IList<CommandBuffer> _commandBuffers = new List<CommandBuffer>();

        private readonly List<Tuple<uint, ResourceSet>> _defaultResourceSets = new List<Tuple<uint, ResourceSet>>();
        private bool _doFrameCapture;
        private Fence _fence;

        private readonly FullScreenQuadRenderer _fullScreenQuadRenderer;

        private uint _hostBufferStride = 1u;

        private bool _initialized;

        private readonly ILogger _logger;

        private DeviceBuffer _projectionBuffer;

        private RenderInfo _renderInfo;
        private ResourceLayout _resourceLayout;
        private ResourceSet _resourceSet;

        private readonly Stopwatch _stopWatch = new Stopwatch();
        private DeviceBuffer _viewBuffer;

        private uint _maxFramesInFlight;
        public Fence[] _fences;
        public Semaphore[] _imageAcquiredSems;
        public Semaphore[] _renderCompleteSems;
        private uint _frameIndex;
        
        public Renderer(ICamera camera)
        {
            _camera = camera;
            _updateVisitor = UpdateVisitor.Create();
            _cullVisitor = CullVisitor.Create();
            //_logger = LogManager.CreateLogger<Renderer>();
            _fullScreenQuadRenderer = new FullScreenQuadRenderer();
            SceneContext = null;
        }

        public SceneContext SceneContext { get; set; }

        public void CaptureNextFrame()
        {
            _doFrameCapture = true;
        }

        public CommandBuffer[] HandleFrame(uint frameIndex, GraphicsDevice device, ResourceFactory factory, Framebuffer fb)
        {
            if (!_initialized) Initialize(device, factory);
            
            if (null == device)
            {
                throw new Exception("Graphics Device is not set!");
            }
            
            if (null == factory)
            {
                throw new Exception("Resource Factory is not set!");
            }
            
            _stopWatch.Reset();
            _stopWatch.Start();
            
            Event();
            
            Update();
            
            Cull(device, factory);
            
            UpdateUniforms(device, factory);
            
            return RecordCommandBuffers(device, factory, fb);
        }
        
        public void HandleOperation(GraphicsDevice device, ResourceFactory factory)
        {
            // TODO - this doesn't work on Metal
            //if (null != _fence) device.WaitForFence(_fence);

            if (!_initialized) Initialize(device, factory);

            _stopWatch.Reset();
            _stopWatch.Start();

            Event();

            var postEvent = _stopWatch.ElapsedMilliseconds;

            var devicePtr = IntPtr.Zero;
            if (_doFrameCapture)
            {
                RenderDocManager.Instance?.TriggerCapture();
                if (device.GetD3D11Info(out var info))
                {
                    devicePtr = info.Device;
                    RenderDocManager.Instance?.StartFrameCapture(devicePtr, IntPtr.Zero);
                }
                else
                {
                    RenderDocManager.Instance?.StartFrameCapture();
                }
            }

            Update();

            var postUpdate = _stopWatch.ElapsedMilliseconds;

            Cull(device, factory);

            var postCull = _stopWatch.ElapsedMilliseconds;

            UpdateUniforms(device, factory);

            var postUpdateUniforms = _stopWatch.ElapsedMilliseconds;

            //RecordCommandBuffers(device, factory);

            var postRecord = _stopWatch.ElapsedMilliseconds;

            DrawCommandBuffers(device, factory);

            var postDraw = _stopWatch.ElapsedMilliseconds;

            //SwapBuffers(device);

            var postSwap = _stopWatch.ElapsedMilliseconds;

            if (_doFrameCapture)
            {
                if (null != RenderDocManager.Instance)
                {
                    var result = RenderDocManager.Instance.EndFrameCapture(devicePtr, IntPtr.Zero);
                    if (false == result) _logger?.LogError("Unable to capture frame");
                }

                _doFrameCapture = false;
            }

            var logString = string.Format(
                "Event = {0} ms, Update = {1} ms, Cull = {2} ms, UpdateUniforms = {3} ms, Record = {4}, Draw = {5} ms, Swap = {6} ms",
                postEvent,
                postUpdate - postEvent,
                postCull - postEvent,
                postUpdateUniforms - postCull,
                postRecord - postUpdateUniforms,
                postDraw - postRecord,
                postSwap - postDraw);

            _logger?.LogTrace(logString);
        }

        public void HandleResize(GraphicsDevice device)
        {
            _fullScreenQuadRenderer.CreateDeviceObjects(device, SceneContext);
        }

        private void Initialize(GraphicsDevice device, ResourceFactory factory)
        {
            if (null == device)
            {
                throw new Exception("Graphics Device is not set!");
            }
            
            if (null == factory)
            {
                throw new Exception("Resource Factory is not set!");
            }

            var alignment = 256u; // TODO - fixme! device.UniformBufferMinOffsetAlignment;
            if (alignment > 64u) _hostBufferStride = alignment / 64u;

            _cullVisitor.GraphicsDevice = device;
            _cullVisitor.ResourceFactory = factory;

            _projectionBuffer =
                factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _viewBuffer =
                factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            // TODO - combine view and projection matrices on host
            _resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("View", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));

            _cullVisitor.ResourceLayout = _resourceLayout;

            _resourceSet = factory.CreateResourceSet(
                new ResourceSetDescription(_resourceLayout, _projectionBuffer, _viewBuffer));

            //_commandList = factory.CreateCommandList();

            _renderInfo = new RenderInfo();
            _renderInfo.GraphicsDevice = device;
            _renderInfo.ResourceFactory = factory;
            _renderInfo.CommandList = _commandList;
            _renderInfo.ResourceLayout = _resourceLayout;
            _renderInfo.ResourceSet = _resourceSet;

            _fence = factory.CreateFence(false);

            _defaultResourceSets.Add(Tuple.Create((uint) 0, _resourceSet));

            _maxFramesInFlight = device.MainSwapchain.BufferCount;
            _frameIndex = _maxFramesInFlight - 1;

            _fences = new Fence[_maxFramesInFlight];
            _imageAcquiredSems = new Semaphore[_maxFramesInFlight];
            _renderCompleteSems = new Semaphore[_maxFramesInFlight];
            for (uint i = 0; i < _maxFramesInFlight; i++)
            {
                _fences[i] = device.ResourceFactory.CreateFence(signaled: true);
                _fences[i].Name = $"Renderer Fence {i}";
                _imageAcquiredSems[i] = device.ResourceFactory.CreateSemaphore();
                _renderCompleteSems[i] = device.ResourceFactory.CreateSemaphore();
            }
            
            _initialized = true;
        }

        private void Event()
        {
            if (_camera.View is View viewerView) viewerView.EventTraversal();
        }

        private void Update()
        {
            if (_camera.View is View viewerView)
            {
                viewerView.SceneData?.Accept(_updateVisitor);

                viewerView.CameraManipulator?.UpdateCamera(_camera);
            }
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

            if (null == _cullVisitor.GraphicsDevice)
            {
                throw new Exception("Graphics Device is not set!");
            }
            
            if (null == _cullVisitor.ResourceFactory)
            {
                throw new Exception("Resource Factory is not set!");
            }
            
            if (_camera.View is View viewerView) viewerView.SceneData?.Accept(_cullVisitor);
        }

        private void Record(GraphicsDevice device, ResourceFactory factory)
        {
            if (!_initialized) Initialize(device, factory);

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
                    DrawTransparentRenderGroups(device, factory, framebuffer);
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
                    DrawTransparentRenderGroups(device, factory, framebuffer);

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

        private CommandBuffer[] RecordCommandBuffers(GraphicsDevice device, ResourceFactory factory, Framebuffer framebuffer)
        {
            foreach (var buffer in _commandBuffers)
            {
                buffer.Dispose();
            }

            _commandBuffers.Clear();
            
            _commandBuffers = new List<CommandBuffer>();
            
            if (!_initialized) Initialize(device, factory);

            CommandBuffer cb = factory.CreateCommandBuffer(CommandBufferFlags.Reusable);
            cb.Name = $"Veldrid.SceneGraph Command Buffer";

            if (true) //SceneContext.MainSceneColorTexture.SampleCount == TextureSampleCount.Count1)
            {
                //var framebuffer = SceneContext.OutputFramebuffer;

                cb.BeginRenderPass(
                    framebuffer, 
                    LoadAction.Clear, 
                    StoreAction.Store, 
                    _camera.ClearColor, 
                    1f);
                //
                // Draw Opaque Geometry
                // 
                DrawOpaqueRenderGroups(device, factory, framebuffer, cb);

                // 
                // Draw Transparent Geometry
                //
                if (_cullVisitor.TransparentRenderGroup.HasDrawableElements())
                    DrawTransparentRenderGroups(device, factory, framebuffer, cb);

                cb.EndRenderPass();
                
                _commandBuffers.Add(cb);
            }
            else
            {
                //var framebuffer = SceneContext.MainSceneFramebuffer;

                cb.BeginRenderPass(
                    framebuffer, 
                    LoadAction.Clear, 
                    StoreAction.Store, 
                    _camera.ClearColor, 
                    1f);

                //
                // Draw Opaque Geometry
                // 
                DrawOpaqueRenderGroups(device, factory, framebuffer, cb);

                // 
                // Draw Transparent Geometry
                //
                if (_cullVisitor.TransparentRenderGroup.HasDrawableElements())
                    DrawTransparentRenderGroups(device, factory, framebuffer,cb);

                cb.EndRenderPass();
                
                _commandBuffers.Add(cb);
                
                CommandBuffer blitter = factory.CreateCommandBuffer(CommandBufferFlags.Reusable);
                blitter.Name = $"Veldrid.SceneGraph Blit Buffer";
                
                blitter.BeginRenderPass(
                    SceneContext.OutputFramebuffer,
                    LoadAction.Clear, 
                    StoreAction.Store, 
                    _camera.ClearColor, 
                    1f);
                
                //
                // Resolve the texture
                //
                cb.BlitTexture(
                    SceneContext.MainSceneColorTexture,
                    0, 
                    0, 
                    SceneContext.MainSceneColorTexture.Width, 
                    SceneContext.MainSceneColorTexture.Height,
                    SceneContext.OutputFramebuffer, 
                    0, 
                    0, 
                    SceneContext.MainSceneColorTexture.Width, 
                    SceneContext.MainSceneColorTexture.Height, 
                    true);

                blitter.EndRenderPass();
                _commandBuffers.Add(blitter);
            }

            return _commandBuffers.ToArray();
        }
        
        private void Draw(GraphicsDevice device)
        {
            device.ResetFence(_fence);
            device.SubmitCommands(_commandList, _fence);
            device.WaitForFence(_fence);
            device.WaitForIdle();
        }

        private void DrawCommandBuffers(GraphicsDevice device, ResourceFactory factory)
        {
            var sc = device.MainSwapchain;
            uint nextFrame = (sc.LastAcquiredImage + 1) % _maxFramesInFlight;
            _fences[nextFrame].Wait();
            _fences[nextFrame].Reset();

            AcquireResult acquireResult = device.AcquireNextImage(
                sc,
                _imageAcquiredSems[nextFrame],
                null,
                out _frameIndex);
            if (acquireResult == AcquireResult.OutOfDate)
            {
                sc.Resize(sc.Width, sc.Height);

                nextFrame = 0;
                acquireResult = device.AcquireNextImage(
                    sc,
                    _imageAcquiredSems[nextFrame],
                    null,
                    out _frameIndex);
                if (acquireResult != AcquireResult.Success)
                {
                    throw new VeldridException($"Failed to acquire Swapchain image.");
                }
            } 
            
            var cbs = RecordCommandBuffers(device, factory, sc.Framebuffers[_frameIndex]);
            device.SubmitCommands(
                cbs,
                _imageAcquiredSems[_frameIndex],
                _renderCompleteSems[_frameIndex],
                _fences[_frameIndex]);
            device.Present(sc, _renderCompleteSems[_frameIndex], _frameIndex);
        }

        private void DrawOpaqueRenderGroups(GraphicsDevice device, ResourceFactory factory, Framebuffer framebuffer)
        {
            foreach (var state in _cullVisitor.OpaqueRenderGroup.GetStateList())
            {
                if (state.Elements.Count == 0) continue;

                var ri = state.GetPipelineAndResources(device, factory, _resourceLayout, framebuffer);

                _commandList.SetPipeline(ri.Pipeline);

                var nDrawables = (uint) state.Elements.Count;

                for (var i = 0; i < nDrawables; ++i)
                    ri.ModelViewMatrixBuffer[i * _hostBufferStride] = state.Elements[i].ModelViewMatrix;
                _commandList.UpdateBuffer(ri.ModelViewBuffer, 0, ri.ModelViewMatrixBuffer);

                for (var i = 0; i < nDrawables; ++i)
                {
                    var element = state.Elements[i];

                    for (var vboIdx = 0; vboIdx < element.VertexBuffers.Count; ++vboIdx)
                        _commandList.SetVertexBuffer((uint) vboIdx, element.VertexBuffers[vboIdx]);

                    _commandList.SetIndexBuffer(element.IndexBuffer, IndexFormat.UInt32);

                    _commandList.SetGraphicsResourceSet(0, _resourceSet);

                    _commandList.SetGraphicsResourceSet(1, ri.ResourceSet, ri.OffsetArrayList[i]);

                    foreach (var primitiveSet in element.PrimitiveSets) primitiveSet.Draw(_commandList);
                }
            }
        }

        private void DrawOpaqueRenderGroups(GraphicsDevice device, ResourceFactory factory, Framebuffer framebuffer, CommandBuffer cb)
        {
            foreach (var state in _cullVisitor.OpaqueRenderGroup.GetStateList())
            {
                if (state.Elements.Count == 0) continue;

                var ri = state.GetPipelineAndResources(device, factory, _resourceLayout, framebuffer);

                cb.BindPipeline(ri.Pipeline);

                var nDrawables = (uint) state.Elements.Count;

                for (var i = 0; i < nDrawables; ++i)
                {
                    ri.ModelViewMatrixBuffer[i * _hostBufferStride] = state.Elements[i].ModelViewMatrix;
                }
                    
                device.UpdateBuffer(ri.ModelViewBuffer, 0, ri.ModelViewMatrixBuffer);

                for (var i = 0; i < nDrawables; ++i)
                {
                    var element = state.Elements[i];

                    for (var vboIdx = 0; vboIdx < element.VertexBuffers.Count; ++vboIdx)
                        cb.BindVertexBuffer((uint) vboIdx, element.VertexBuffers[vboIdx]);

                    cb.BindIndexBuffer(element.IndexBuffer, IndexFormat.UInt32);

                    cb.BindGraphicsResourceSet(0, _resourceSet);

                    cb.BindGraphicsResourceSet(1, ri.ResourceSet, ri.OffsetArrayList[i]);

                    foreach (var primitiveSet in element.PrimitiveSets) 
                        primitiveSet.Draw(cb);
                }
            }
        }
        
        private void DrawTransparentRenderGroups(GraphicsDevice device, ResourceFactory factory,
            Framebuffer framebuffer)
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
            var drawOrderMap =
                new SortedList<float, List<Tuple<IRenderGroupState, RenderGroupElement, IPrimitiveSet, uint>>>();
            drawOrderMap.Capacity = _cullVisitor.RenderElementCount;
            var transparentRenderGroupStates = _cullVisitor.TransparentRenderGroup.GetStateList();

            var stateToUniformDict = new Dictionary<IRenderGroupState, Matrix4x4[]>();

            foreach (var state in transparentRenderGroupStates)
            {
                var nDrawables = (uint) state.Elements.Count;
                var modelMatrixViewBuffer = new Matrix4x4[nDrawables * hostBuffStride];

                //var renderInfo = state.GetPipelineAndResources(device, factory, _resourceLayout, framebuffer);

                // Iterate over all elements in this state
                for (var j = 0; j < nDrawables; ++j)
                {
                    var renderElement = state.Elements[j];
                    modelMatrixViewBuffer[j * hostBuffStride] = state.Elements[j].ModelViewMatrix;
                    //renderInfo.ModelViewMatrixBuffer[j*hostBuffStride] = state.Elements[j].ModelViewMatrix;

                    // Iterate over all primitive sets in this state
                    foreach (var pset in renderElement.PrimitiveSets)
                    {
                        var sortDist = 0.0f;

                        // Compute distance eye point 
                        var modelView = renderElement.ModelViewMatrix;
                        if (Matrix4x4.Invert(modelView, out var modelViewInvese))
                        {
                            var eyeLocal = Vector3.Transform(Vector3.Zero, modelViewInvese);
                            sortDist = pset.GetEyePointDistance(eyeLocal);
                        }
                        else
                        {
                            var ctr = pset.GetBoundingBox().Center;
                            var ctrW = Vector3.Transform(ctr, modelView);
                            sortDist = Vector3.Distance(ctrW, Vector3.Zero);
                        }


                        if (!drawOrderMap.TryGetValue(sortDist, out var renderList))
                        {
                            renderList = new List<Tuple<IRenderGroupState, RenderGroupElement, IPrimitiveSet, uint>>();
                            drawOrderMap.Add(sortDist, renderList);
                        }

                        renderList.Add(Tuple.Create(state, renderElement, pset, (uint) j));
                    }
                }

                stateToUniformDict.Add(state, modelMatrixViewBuffer);
            }

            List<DeviceBuffer> boundVertexBufferList = null;
            DeviceBuffer boundIndexBuffer = null;

            // Now draw transparent elements, back to front
            IRenderGroupState lastState = null;
            RenderGraph.RenderInfo ri = null;

            foreach (var renderList in drawOrderMap.Reverse())
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

                var offset = element.Item4 * modelBuffStride;

                // Set state-local resources
                _commandList.SetGraphicsResourceSet(1, ri.ResourceSet, 1, ref offset);

                var renderGroupElement = element.Item2;

                if (boundVertexBufferList != renderGroupElement.VertexBuffers)
                {
                    // Set vertex buffer
                    for (var vboIdx = 0; vboIdx < renderGroupElement.VertexBuffers.Count; ++vboIdx)
                        _commandList.SetVertexBuffer((uint) vboIdx, renderGroupElement.VertexBuffers[vboIdx]);

                    boundVertexBufferList = renderGroupElement.VertexBuffers;
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

        private void DrawTransparentRenderGroups(GraphicsDevice device, ResourceFactory factory,
            Framebuffer framebuffer, CommandBuffer cb)
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
            var drawOrderMap =
                new SortedList<float, List<Tuple<IRenderGroupState, RenderGroupElement, IPrimitiveSet, uint>>>();
            drawOrderMap.Capacity = _cullVisitor.RenderElementCount;
            var transparentRenderGroupStates = _cullVisitor.TransparentRenderGroup.GetStateList();

            var stateToUniformDict = new Dictionary<IRenderGroupState, Matrix4x4[]>();

            foreach (var state in transparentRenderGroupStates)
            {
                var nDrawables = (uint) state.Elements.Count;
                var modelMatrixViewBuffer = new Matrix4x4[nDrawables * hostBuffStride];

                //var renderInfo = state.GetPipelineAndResources(device, factory, _resourceLayout, framebuffer);

                // Iterate over all elements in this state
                for (var j = 0; j < nDrawables; ++j)
                {
                    var renderElement = state.Elements[j];
                    modelMatrixViewBuffer[j * hostBuffStride] = state.Elements[j].ModelViewMatrix;
                    //renderInfo.ModelViewMatrixBuffer[j*hostBuffStride] = state.Elements[j].ModelViewMatrix;

                    // Iterate over all primitive sets in this state
                    foreach (var pset in renderElement.PrimitiveSets)
                    {
                        var sortDist = 0.0f;

                        // Compute distance eye point 
                        var modelView = renderElement.ModelViewMatrix;
                        if (Matrix4x4.Invert(modelView, out var modelViewInvese))
                        {
                            var eyeLocal = Vector3.Transform(Vector3.Zero, modelViewInvese);
                            sortDist = pset.GetEyePointDistance(eyeLocal);
                        }
                        else
                        {
                            var ctr = pset.GetBoundingBox().Center;
                            var ctrW = Vector3.Transform(ctr, modelView);
                            sortDist = Vector3.Distance(ctrW, Vector3.Zero);
                        }


                        if (!drawOrderMap.TryGetValue(sortDist, out var renderList))
                        {
                            renderList = new List<Tuple<IRenderGroupState, RenderGroupElement, IPrimitiveSet, uint>>();
                            drawOrderMap.Add(sortDist, renderList);
                        }

                        renderList.Add(Tuple.Create(state, renderElement, pset, (uint) j));
                    }
                }

                stateToUniformDict.Add(state, modelMatrixViewBuffer);
            }

            List<DeviceBuffer> boundVertexBufferList = null;
            DeviceBuffer boundIndexBuffer = null;

            // Now draw transparent elements, back to front
            IRenderGroupState lastState = null;
            RenderGraph.RenderInfo ri = null;

            foreach (var renderList in drawOrderMap.Reverse())
            foreach (var element in renderList.Value)
            {
                var state = element.Item1;

                if (null == lastState || state != lastState)
                {
                    ri = state.GetPipelineAndResources(device, factory, _resourceLayout, framebuffer);

                    // Set this state's pipeline
                    cb.BindPipeline(ri.Pipeline);

                    device.UpdateBuffer(ri.ModelViewBuffer, 0, stateToUniformDict[state]);

                    // Set the resources
                    cb.BindGraphicsResourceSet(0, _resourceSet);
                }

                var offset = new Span<uint>(new uint[1] {element.Item4 * modelBuffStride});

                // Set state-local resources
                cb.BindGraphicsResourceSet(1, ri.ResourceSet, offset);

                var renderGroupElement = element.Item2;

                if (boundVertexBufferList != renderGroupElement.VertexBuffers)
                {
                    // Set vertex buffer
                    for (var vboIdx = 0; vboIdx < renderGroupElement.VertexBuffers.Count; ++vboIdx)
                        cb.BindVertexBuffer((uint) vboIdx, renderGroupElement.VertexBuffers[vboIdx]);

                    boundVertexBufferList = renderGroupElement.VertexBuffers;
                }

                if (boundIndexBuffer != renderGroupElement.IndexBuffer)
                {
                    // Set index buffer
                    cb.BindIndexBuffer(renderGroupElement.IndexBuffer, IndexFormat.UInt32);
                    boundIndexBuffer = renderGroupElement.IndexBuffer;
                }

                element.Item3.Draw(cb);

                lastState = state;
            }
        }
        
        private void UpdateUniforms(GraphicsDevice device, ResourceFactory factory)
        {
            if (!_initialized) Initialize(device, factory);

            foreach (var state in _cullVisitor.OpaqueRenderGroup.GetStateList())
            {
                var nDrawables = (uint) state.Elements.Count;

                if (nDrawables == 0) continue;
                
                foreach (var uniform in state.PipelineState.UniformList)
                {
                    uniform.UpdateDeviceBuffers(device, factory);
                }

                foreach (var texture in state.PipelineState.TextureList)
                {
                    texture.UpdateDeviceBuffers(device, factory);
                }
            }
            
            foreach (var state in _cullVisitor.TransparentRenderGroup.GetStateList())
            {
                var nDrawables = (uint) state.Elements.Count;

                if (nDrawables == 0) continue;
                

                foreach (var uniform in state.PipelineState.UniformList)
                {
                    uniform.UpdateDeviceBuffers(device, factory);
                }

                foreach (var texture in state.PipelineState.TextureList)
                {
                    texture.UpdateDeviceBuffers(device, factory);
                }
            }

            device.UpdateBuffer(_projectionBuffer, 0, _camera.ProjectionMatrix);

            // TODO - Remove
            device.UpdateBuffer(_viewBuffer, 0, Matrix4x4.Identity);
        }

        private void SwapBuffers(GraphicsDevice device)
        {
            if (SceneContext.OutputFramebuffer == device.SwapchainFramebuffer) device.SwapBuffers();
        }
    }
}