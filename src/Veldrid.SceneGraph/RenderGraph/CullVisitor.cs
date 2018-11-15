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
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using AssetProcessor;
using Veldrid.SceneGraph.Text;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Viewer;
using Veldrid.Utilities;

namespace Veldrid.SceneGraph.RenderGraph
{
    public class CullVisitor : NodeVisitor
    {
        public RenderGroup OpaqueRenderGroup { get; set; } = new RenderGroup();
        public RenderGroup TransparentRenderGroup { get; set; } = new RenderGroup();

        public GraphicsDevice GraphicsDevice { get; set; } = null;
        public ResourceFactory ResourceFactory { get; set; } = null;
        public ResourceLayout ResourceLayout { get; set; } = null;

        public Stack<Matrix4x4> ModelMatrixStack { get; set; } = new Stack<Matrix4x4>();

        public Stack<PipelineState> PipelineStateStack = new Stack<PipelineState>();

        private int _currVertexBufferIndex = 0;
        private uint _currVertexBufferOffset = 0;
        public List<DeviceBuffer> VertexBufferList { get; } = new List<DeviceBuffer>();
        
        private int _currIndexBufferIndex = 0;
        private uint _currIndexBufferOffset = 0;
        public List<DeviceBuffer> IndexBufferList { get; } = new List<DeviceBuffer>();

        public bool Valid => null != GraphicsDevice;
        
        private Polytope CullingFrustum { get; set; } = new Polytope();

        public int RenderElementCount { get; private set; } = 0;

        private Matrix4x4 ViewMatrix { get; set; } = Matrix4x4.Identity;
        private Matrix4x4 ProjectionMatrix { get; set; } = Matrix4x4.Identity;
       
        
        public CullVisitor() : 
            base(VisitorType.CullAndAssembleVisitor, TraversalModeType.TraverseActiveChildren)
        {
            ModelMatrixStack.Push(Matrix4x4.Identity);
        }

        public void Reset()
        {
            ModelMatrixStack.Clear();
            ModelMatrixStack.Push(Matrix4x4.Identity);
            
            PipelineStateStack.Clear();

            _currIndexBufferIndex = 0;
            _currVertexBufferOffset = 0;

            _currVertexBufferIndex = 0;
            _currIndexBufferOffset = 0;
            
            OpaqueRenderGroup.Reset();
            TransparentRenderGroup.Reset();
            
            RenderElementCount = 0;
            
        }

        public void SetViewMatrix(Matrix4x4 viewMatrix)
        {
            ViewMatrix = viewMatrix;
        }
        
        public void SetProjectionMatrix(Matrix4x4 projectionMatrix)
        {
            ProjectionMatrix = projectionMatrix;
        }

        public void Prepare()
        {
            var vp = ViewMatrix.PostMultiply(ProjectionMatrix);
            CullingFrustum.VPMatrix = vp;
        }

        private Matrix4x4 GetModelViewMatrix()
        {
            return ModelMatrixStack.Peek().PostMultiply(ViewMatrix);
        }
        
        private Matrix4x4 GetModelViewInverseMatrix()
        {
            var canInvert = Matrix4x4.Invert(ModelMatrixStack.Peek().PostMultiply(ViewMatrix), out var inverse);
            if (false == canInvert)
            {
                throw new Exception("ModelView Matrix Cannot be Inverted");
            }

            return inverse;
        }

        private Vector3 GetEyeLocal()
        {
            var eyeWorld = Vector3.Zero;
            var modelViewInverse = GetModelViewInverseMatrix();
            return Vector3.Transform(eyeWorld, modelViewInverse);
        }
        
        private bool IsCulled(BoundingBox bb, Matrix4x4 modelMatrix)
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
            
            Traverse(node);

            if (needsPop)
            {
                PipelineStateStack.Pop();
            }
        }

        public override void Apply(Transform transform)
        {
            var curModel = ModelMatrixStack.Peek();
            transform.ComputeLocalToWorldMatrix(ref curModel, this);
            ModelMatrixStack.Push(curModel);
            
            Apply((Node)transform);

            ModelMatrixStack.Pop();

        }

        public override void Apply(Geode geode)
        {
            var bb = geode.GetBoundingBox();
            if (IsCulled(bb, ModelMatrixStack.Peek())) return;
            
            PipelineState pso = null;

            // Node specific state
            if (geode.HasPipelineState)
            {
                pso = geode.PipelineState;
            }

            // Shared State
            else if (PipelineStateStack.Count != 0)
            {
                pso = PipelineStateStack.Peek();
            }

            // Fallback
            else
            {
                pso = new PipelineState();
            }

            foreach (var drawable in geode.Drawables)
            {
                if (IsCulled(drawable.GetBoundingBox(), ModelMatrixStack.Peek())) continue;
                
                var drawablePso = pso;
                if (drawable.HasPipelineState)
                {
                    drawablePso = drawable.PipelineState;
                }

                //
                // This allocates / updates vbo/ibos
                //
                drawable.ConfigureDeviceBuffers(GraphicsDevice, ResourceFactory);

                var renderElementCache = new Dictionary<RenderGroupState, RenderGroupElement>();
                
                foreach (var pset in drawable.PrimitiveSets)
                {
                    if (IsCulled(pset.GetBoundingBox(), ModelMatrixStack.Peek())) continue;
                    
                    //            
                    // Sort into appropriate render group
                    // 
                    RenderGroupState renderGroupState = null;
                    if (drawablePso.BlendStateDescription.AttachmentStates.Contains(BlendAttachmentDescription.AlphaBlend))
                    {
                        renderGroupState = TransparentRenderGroup.GetOrCreateState(drawablePso, pset.PrimitiveTopology, drawable.VertexLayout);
                    }
                    else
                    {
                        renderGroupState = OpaqueRenderGroup.GetOrCreateState(drawablePso, pset.PrimitiveTopology, drawable.VertexLayout);
                    }

                    if (false == renderElementCache.TryGetValue(renderGroupState, out var renderElement))
                    {
                        renderElement = new RenderGroupElement()
                        {
                            ModelViewMatrix = GetModelViewMatrix(),
                            VertexBuffer = drawable.GetVertexBufferForDevice(GraphicsDevice),
                            IndexBuffer = drawable.GetIndexBufferForDevice(GraphicsDevice),
                            PrimitiveSets = new List<PrimitiveSet>()
                        };
                        renderGroupState.Elements.Add(renderElement);
                        
                        renderElementCache.Add(renderGroupState, renderElement);
                    }
                    renderElement.PrimitiveSets.Add(pset);
                }
            }
        }

        /// <summary>
        /// Cull Visitor for billboard 
        /// </summary>
        /// <param name="billboard"></param>
        public override void Apply(Billboard billboard)
        {
            var bb = billboard.GetBoundingBox();
            if (IsCulled(bb, ModelMatrixStack.Peek())) return;
            
            PipelineState pso = null;

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
                pso = new PipelineState();
            }

            var eyeLocal = GetEyeLocal();
            var modelView = GetModelViewMatrix();

            foreach (var drawable in billboard.Drawables)
            {
                // TODO - need to modify is culled to handle billboard matrix offset
                //if (IsCulled(drawable.GetBoundingBox(), ModelMatrixStack.Peek())) continue;

                var billboardMatrix = billboard.ComputeMatrix(modelView, eyeLocal);
                
                var drawablePso = pso;
                if (drawable.HasPipelineState)
                {
                    drawablePso = drawable.PipelineState;
                }

                //
                // This allocates / updates vbo/ibos
                //
                drawable.ConfigureDeviceBuffers(GraphicsDevice, ResourceFactory);

                var renderElementCache = new Dictionary<RenderGroupState, RenderGroupElement>();
                
                foreach (var pset in drawable.PrimitiveSets)
                {
                    // TODO - need to modify is culled to handle billboard matrix offset
                    //if (IsCulled(pset.GetBoundingBox(), ModelMatrixStack.Peek())) continue;
                    
                    //            
                    // Sort into appropriate render group
                    // 
                    RenderGroupState renderGroupState = null;
                    if (drawablePso.BlendStateDescription.AttachmentStates.Contains(BlendAttachmentDescription.AlphaBlend))
                    {
                        renderGroupState = TransparentRenderGroup.GetOrCreateState(drawablePso, pset.PrimitiveTopology, drawable.VertexLayout);
                    }
                    else
                    {
                        renderGroupState = OpaqueRenderGroup.GetOrCreateState(drawablePso, pset.PrimitiveTopology, drawable.VertexLayout);
                    }

                    if (false == renderElementCache.TryGetValue(renderGroupState, out var renderElement))
                    {
                        renderElement = new RenderGroupElement()
                        {
                            ModelViewMatrix = billboardMatrix.PostMultiply(modelView),
                            VertexBuffer = drawable.GetVertexBufferForDevice(GraphicsDevice),
                            IndexBuffer = drawable.GetIndexBufferForDevice(GraphicsDevice),
                            PrimitiveSets = new List<PrimitiveSet>()
                        };
                        renderGroupState.Elements.Add(renderElement);
                        
                        renderElementCache.Add(renderGroupState, renderElement);
                    }
                    renderElement.PrimitiveSets.Add(pset);
                }
            }
        }
    }
}