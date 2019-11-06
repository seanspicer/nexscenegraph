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
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Veldrid.SceneGraph.Text;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Viewer;
using Veldrid.Utilities;

namespace Veldrid.SceneGraph.RenderGraph
{
    public class CullVisitor : NodeVisitor, ICullVisitor
    {
        public IRenderGroup OpaqueRenderGroup { get; set; } = RenderGroup.Create();
        public IRenderGroup TransparentRenderGroup { get; set; } = RenderGroup.Create();

        public GraphicsDevice GraphicsDevice { get; set; } = null;
        public ResourceFactory ResourceFactory { get; set; } = null;
        public ResourceLayout ResourceLayout { get; set; } = null;
        
        private Stack<Matrix4x4> ModelMatrixStack { get; set; } = new Stack<Matrix4x4>();

        private readonly Stack<IPipelineState> PipelineStateStack = new Stack<IPipelineState>();

        public bool Valid => null != GraphicsDevice;

        private IPolytope CullingFrustum { get; set; } = Polytope.Create();

        public int RenderElementCount { get; private set; } = 0;

        private Matrix4x4 ViewMatrix => GetCurrentCamera().ViewMatrix;
        private Matrix4x4 ProjectionMatrix => GetCurrentCamera().ProjectionMatrix;

        private ICamera _camera;
        
        public static ICullVisitor Create()
        {
            return new CullVisitor();
        }

        public IMutableCullVisitor ToMutable()
        {
            return new MutableCullVisitor(this);
        }
        
        protected CullVisitor() : 
            base(VisitorType.CullAndAssembleVisitor, TraversalModeType.TraverseActiveChildren)
        {
            ModelMatrixStack.Push(Matrix4x4.Identity);
        }

        internal void Reset()
        {
            ModelMatrixStack.Clear();
            ModelMatrixStack.Push(Matrix4x4.Identity);
            
            PipelineStateStack.Clear();
            OpaqueRenderGroup.Reset();
            TransparentRenderGroup.Reset();
            
            RenderElementCount = 0;
            
        }

        internal void SetCurrentCamera(ICamera camera)
        {
            _camera = camera;
            
        }

        public ICamera GetCurrentCamera()
        {
            return _camera;
        }

        internal void Prepare()
        {
            var vp = ViewMatrix.PostMultiply(ProjectionMatrix);
            CullingFrustum.SetViewProjectionMatrix(vp);
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            return ProjectionMatrix;
        }
        
        public Matrix4x4 GetModelViewMatrix()
        {
            return ModelMatrixStack.Peek().PostMultiply(ViewMatrix);
        }
        
        public Matrix4x4 GetModelViewProjectionMatrix()
        {
            return ModelMatrixStack.Peek().PostMultiply(ViewMatrix).PostMultiply(ProjectionMatrix);
        }
        
        public Matrix4x4 GetModelViewInverseMatrix()
        {
            var canInvert = Matrix4x4.Invert(GetModelViewMatrix(), out var inverse);
            if (false == canInvert)
            {
                throw new Exception("ModelView Matrix Cannot be Inverted");
            }

            return inverse;
        }

        public Matrix4x4 GetModelViewProjectionInverseMatrix()
        {
            var canInvert = Matrix4x4.Invert(GetModelViewProjectionMatrix(), out var inverse);
            if (false == canInvert)
            {
                throw new Exception("ModelViewProjection Matrix Cannot be Inverted");
            }

            return inverse;
        }
        
        public Vector3 GetEyeLocal()
        {
            var eyeWorld = Vector3.Zero;
            var modelViewInverse = GetModelViewInverseMatrix();
            return Vector3.Transform(eyeWorld, modelViewInverse);
        }
        
        private bool IsCulled(IBoundingBox bb, Matrix4x4 modelMatrix)
        {
            var culled = !CullingFrustum.Contains(bb, modelMatrix);

            return culled;
        }

        public override void Apply(INode node)
        {
            var needsPop = false;
            if (node.HasPipelineState)
            {
                PipelineStateStack.Push(node.PipelineState);
                needsPop = true;
            }
            
            HandleCallbacksAndTraverse(node);

            if (needsPop)
            {
                PipelineStateStack.Pop();
            }
        }

        public override void Apply(ITransform transform)
        {
            var curModel = ModelMatrixStack.Peek();
            transform.ComputeLocalToWorldMatrix(ref curModel, this);
            ModelMatrixStack.Push(curModel);
            
            Apply((Node)transform);

            ModelMatrixStack.Pop();

        }

        public override void Apply(IGeode geode)
        {
            var bb = geode.GetBoundingBox();
            if (IsCulled(bb, ModelMatrixStack.Peek())) return;
            
            var needsPop = false;
            if (geode.HasPipelineState)
            {
                PipelineStateStack.Push(geode.PipelineState);
                needsPop = true;
            }
            
            HandleCallbacksAndTraverse((Node)geode);

            if (needsPop)
            {
                PipelineStateStack.Pop();
            }
            
            
//            IPipelineState pso = null;

//            // Node specific state
//            if (geode.HasPipelineState)
//            {
//                pso = geode.PipelineState;
//            }
//
//            // Shared State
//            else if (PipelineStateStack.Count != 0)
//            {
//                pso = PipelineStateStack.Peek();
//            }
//
//            // Fallback
//            else
//            {
//                pso = PipelineState.Create();
//            }

            
            
//            foreach (var drawable in geode.Drawables)
//            {
//                if (IsCulled(drawable.GetBoundingBox(), ModelMatrixStack.Peek())) continue;
//                
//                var drawablePso = pso;
//                if (drawable.HasPipelineState)
//                {
//                    drawablePso = drawable.PipelineState;
//                }
//
//                //
//                // This allocates / updates vbo/ibos
//                //
//                drawable.ConfigureDeviceBuffers(GraphicsDevice, ResourceFactory);
//
//                var renderElementCache = new Dictionary<IRenderGroupState, RenderGroupElement>();
//                
//                foreach (var pset in drawable.PrimitiveSets)
//                {
//                    if (IsCulled(pset.GetBoundingBox(), ModelMatrixStack.Peek())) continue;
//                    
//                    //            
//                    // Sort into appropriate render group
//                    // 
//                    IRenderGroupState renderGroupState = null;
//                    if (drawablePso.BlendStateDescription.AttachmentStates.Contains(BlendAttachmentDescription.AlphaBlend))
//                    {
//                        renderGroupState = TransparentRenderGroup.GetOrCreateState(GraphicsDevice, drawablePso, pset.PrimitiveTopology, drawable.VertexLayout);
//                    }
//                    else
//                    {
//                        renderGroupState = OpaqueRenderGroup.GetOrCreateState(GraphicsDevice, drawablePso, pset.PrimitiveTopology, drawable.VertexLayout);
//                    }
//
//                    if (false == renderElementCache.TryGetValue(renderGroupState, out var renderElement))
//                    {
//                        renderElement = new RenderGroupElement()
//                        {
//                            ModelViewMatrix = GetModelViewMatrix(),
//                            VertexBuffer = drawable.GetVertexBufferForDevice(GraphicsDevice),
//                            IndexBuffer = drawable.GetIndexBufferForDevice(GraphicsDevice),
//                            PrimitiveSets = new List<IPrimitiveSet>()
//                        };
//                        renderGroupState.Elements.Add(renderElement);
//                        
//                        renderElementCache.Add(renderGroupState, renderElement);
//                    }
//                    renderElement.PrimitiveSets.Add(pset);
//                }
//            }
        }

        /// <summary>
        /// Cull Visitor for billboard 
        /// </summary>
        /// <param name="billboard"></param>
        public override void Apply(IBillboard billboard)
        {
            var bb = billboard.GetBoundingBox();
            if (IsCulled(bb, ModelMatrixStack.Peek())) return;
            
            IPipelineState pso = null;

            // Node specific state
            if (billboard.HasPipelineState)
            {
                pso = billboard.PipelineState;
            }

            // Shared State
            else if (PipelineStateStack.Count != 0)
            {
                pso = PipelineStateStack.Peek();
            }

            // Fallback
            else
            {
                pso = PipelineState.Create();
            }

            var eyeLocal = GetEyeLocal();
            var modelView = GetModelViewMatrix();

            
            for(var i=0; i<billboard.GetNumChildren();++i)
            {
                var drawable = billboard.GetDrawable(i);
                
                // TODO - need to modify is culled to handle billboard matrix offset
                //if (IsCulled(drawable.GetBoundingBox(), ModelMatrixStack.Peek())) continue;

                var billboardMatrix = billboard.ComputeMatrix(modelView, ProjectionMatrix, eyeLocal);
                
                var drawablePso = pso;
                if (drawable.HasPipelineState)
                {
                    drawablePso = drawable.PipelineState;
                }

                //
                // This allocates / updates vbo/ibos
                //
                drawable.ConfigureDeviceBuffers(GraphicsDevice, ResourceFactory);

                var renderElementCache = new Dictionary<IRenderGroupState, RenderGroupElement>();
                
                foreach (var pset in drawable.PrimitiveSets)
                {
                    // TODO - need to modify is culled to handle billboard matrix offset
                    //if (IsCulled(pset.GetBoundingBox(), ModelMatrixStack.Peek())) continue;
                    
                    //            
                    // Sort into appropriate render group
                    // 
                    IRenderGroupState renderGroupState = null;
                    if (drawablePso.BlendStateDescription.AttachmentStates.Contains(BlendAttachmentDescription.AlphaBlend))
                    {
                        renderGroupState = TransparentRenderGroup.GetOrCreateState(GraphicsDevice, drawablePso, pset.PrimitiveTopology, drawable.VertexLayout);
                    }
                    else
                    {
                        renderGroupState = OpaqueRenderGroup.GetOrCreateState(GraphicsDevice, drawablePso, pset.PrimitiveTopology, drawable.VertexLayout);
                    }

                    if (false == renderElementCache.TryGetValue(renderGroupState, out var renderElement))
                    {
                        renderElement = new RenderGroupElement()
                        {
                            ModelViewMatrix = billboardMatrix.PostMultiply(modelView),
                            VertexBuffer = drawable.GetVertexBufferForDevice(GraphicsDevice),
                            IndexBuffer = drawable.GetIndexBufferForDevice(GraphicsDevice),
                            PrimitiveSets = new List<IPrimitiveSet>()
                        };
                        renderGroupState.Elements.Add(renderElement);
                        
                        renderElementCache.Add(renderGroupState, renderElement);
                    }
                    renderElement.PrimitiveSets.Add(pset);
                }
            }
        }

        internal class State : IState
        {
            public Matrix4x4 ModelViewMatrix { get; set; }
            public Matrix4x4 ProjectionMatrix { get; set; }
            public IViewport Viewport { get; set; }
        }

        internal class Viewport : IViewport
        {
            public int Width { get; set; }
            public int Height { get; set; }
        }

        public override void Apply(IDrawable drawable)
        {
            var bb = drawable.GetBoundingBox();
            if (IsCulled(bb, ModelMatrixStack.Peek())) return;

            // Check custom cull callback
            var callback = drawable.GetCullCallback();
            if (null != callback)
            {
                if (callback is IDrawableCullCallback dcb)
                {
                    if (dcb.Cull(this, drawable)) return;
                }
                else
                {
                    callback.Run(drawable, this);
                }
            }
            
            IPipelineState pso = null;

            // Node specific state
            if (drawable.HasPipelineState)
            {
                pso = drawable.PipelineState;
            }

            // Shared State
            else if (PipelineStateStack.Count != 0)
            {
                pso = PipelineStateStack.Peek();
            }

            // Fallback
            else
            {
                pso = PipelineState.Create();
            }

            //
            // This allocates / updates vbo/ibos
            //
            drawable.ConfigureDeviceBuffers(GraphicsDevice, ResourceFactory);

            var renderElementCache = new Dictionary<IRenderGroupState, RenderGroupElement>();

            var state = new State();
            state.ModelViewMatrix = GetModelViewMatrix();
            state.ProjectionMatrix = ProjectionMatrix;
            
            var viewport = new Viewport();
            viewport.Width = (int) DisplaySettings.Instance.ScreenWidth;
            viewport.Height = (int) DisplaySettings.Instance.ScreenHeight;

            state.Viewport = viewport;
            
            var matrix = Matrix4x4.Identity;
            var matrixNeedsToUpdate = drawable.ComputeMatrix(ref matrix, state);
            
            foreach (var pset in drawable.PrimitiveSets)
            {
                if (IsCulled(pset.GetBoundingBox(), ModelMatrixStack.Peek())) continue;
                
                //            
                // Sort into appropriate render group
                // 
                IRenderGroupState renderGroupState = null;
                if (pso.BlendStateDescription.AttachmentStates.Contains(BlendAttachmentDescription.AlphaBlend))
                {
                    renderGroupState = TransparentRenderGroup.GetOrCreateState(GraphicsDevice, pso, pset.PrimitiveTopology, drawable.VertexLayout);
                }
                else
                {
                    renderGroupState = OpaqueRenderGroup.GetOrCreateState(GraphicsDevice, pso, pset.PrimitiveTopology, drawable.VertexLayout);
                }

                if (false == renderElementCache.TryGetValue(renderGroupState, out var renderElement))
                {
                    var modMatrix = Matrix4x4.Identity;
                    if (matrixNeedsToUpdate)
                    {
                        modMatrix = matrix;
                    } 
                    
                    renderElement = new RenderGroupElement()
                    {
                        ModelViewMatrix = modMatrix.PostMultiply(GetModelViewMatrix()),
                        VertexBuffer = drawable.GetVertexBufferForDevice(GraphicsDevice),
                        IndexBuffer = drawable.GetIndexBufferForDevice(GraphicsDevice),
                        PrimitiveSets = new List<IPrimitiveSet>()
                    };
                    renderGroupState.Elements.Add(renderElement);
                    
                    renderElementCache.Add(renderGroupState, renderElement);
                }
                renderElement.PrimitiveSets.Add(pset);
            }
        }
        
        protected void HandleCallbacks(IPipelineState state)
        {
            // TODO Handle state updates.
        }
        
        protected void HandleCallbacksAndTraverse(INode node)
        {
            if (node.HasPipelineState)
            {
                HandleCallbacks(node.PipelineState);
            }

            var callback = node.GetCullCallback();
            if (null != callback)
            {
                callback.Run(node, this);
            }
            else
            {
                Traverse(node);
            }
        }
    }

    internal class MutableCullVisitor : IMutableCullVisitor
    {
        private CullVisitor _cullVisitor;

        internal MutableCullVisitor(CullVisitor cullVisitor)
        {
            _cullVisitor = cullVisitor;
        }
        
        public void Dispose()
        {
        }

        public void SetCurrentCamera(ICamera camera)
        {
            _cullVisitor.SetCurrentCamera(camera);
        }

        public void Reset()
        {
            _cullVisitor.Reset();
        }

        public void Prepare()
        {
            _cullVisitor.Prepare();
        }
    }
}