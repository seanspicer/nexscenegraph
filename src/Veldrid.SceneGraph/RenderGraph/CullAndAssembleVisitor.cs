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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using AssetProcessor;
using Veldrid.SceneGraph.Text;
using Veldrid.Utilities;

namespace Veldrid.SceneGraph.RenderGraph
{
    public class CullAndAssembleVisitor : NodeVisitor
    {
        public RenderGroup OpaqueRenderGroup { get; set; } = new RenderGroup();

        public GraphicsDevice GraphicsDevice { get; set; } = null;
        public ResourceFactory ResourceFactory { get; set; } = null;
        public ResourceLayout ResourceLayout { get; set; } = null;
        
        public Stack<Matrix4x4> ModelMatrixStack { get; set; } = new Stack<Matrix4x4>();

        public Stack<PipelineState> PipelineStateStack = new Stack<PipelineState>();

        public bool Valid => null != GraphicsDevice;
        
        private Polytope CullingFrustum { get; set; } = new Polytope();

        private HashSet<RenderGroupState> PipelineCache { get; set; } = new HashSet<RenderGroupState>();

        public CullAndAssembleVisitor() : 
            base(VisitorType.CullAndAssembleVisitor, TraversalModeType.TraverseActiveChildren)
        {
            ModelMatrixStack.Push(Matrix4x4.Identity);
        }

        public void SetCullingViewProjectionMatrix(Matrix4x4 vp)
        {
            CullingFrustum.SetToViewProjectionFrustum(vp, false, false);
        }

        public override void Apply(Node node)
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
            
            Traverse(transform);

            ModelMatrixStack.Pop();

        }

        public override void Apply<T>(Geometry<T> geometry)
        {

            PipelineState pso = null;

            // Node specific state
            if (geometry.HasPipelineState)
            {
                pso = geometry.PipelineState;
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

            // Setup Vertex and Index Buffers
            var vbDescription = new BufferDescription(
                (uint) (geometry.VertexData.Length * geometry.SizeOfVertexData),
                BufferUsage.VertexBuffer);

            var vertexBuffer = ResourceFactory.CreateBuffer(vbDescription);
            GraphicsDevice.UpdateBuffer(vertexBuffer, 0, geometry.VertexData);
            
            var ibDescription = new BufferDescription(
                (uint) geometry.IndexData.Length * sizeof(ushort),
                BufferUsage.IndexBuffer);

            var indexBuffer = ResourceFactory.CreateBuffer(ibDescription);
            GraphicsDevice.UpdateBuffer(indexBuffer, 0, geometry.IndexData);
            
            // Construct Render Groupe element
            var element = new RenderGroupElement
            {
                Drawable = geometry, 
                ModelMatrix = ModelMatrixStack.Peek(),
                VertexBuffer = vertexBuffer,
                IndexBuffer = indexBuffer
            };

            var renderGroupState = OpaqueRenderGroup.GetOrCreateState(pso, geometry.PrimitiveTopology, geometry.VertexLayout);
            renderGroupState.Elements.Add(element);
        }

        public override void Apply(TextNode textNode)
        {
            PipelineState pso = textNode.PipelineState;
            
            //
            // Setup rendering buffers
            //
                     
            var vbDescription = new BufferDescription(
                (uint) (textNode.VertexData.Length * textNode.SizeOfVertexData),
                BufferUsage.VertexBuffer);

            var vertexBuffer = ResourceFactory.CreateBuffer(vbDescription);
            GraphicsDevice.UpdateBuffer(vertexBuffer, 0, textNode.VertexData);
            
            var ibDescription = new BufferDescription(
                (uint) textNode.IndexData.Length * sizeof(ushort),
                BufferUsage.IndexBuffer);

            var numIndices = (uint) textNode.IndexData.Length;
            var indexBuffer = ResourceFactory.CreateBuffer(ibDescription);
            GraphicsDevice.UpdateBuffer(indexBuffer, 0, textNode.IndexData);

            
            //
            // Setup the texture here
            //
            // TODO: this should probably be done in some sort of update visitor
            //
            pso.TextureList.Add(
                new Texture2D(textNode.BuildTexture(),
                    1,
                    "SurfaceTexture", 
                    "SurfaceSampler"));

            var renderGroupState = OpaqueRenderGroup.GetOrCreateState(pso, textNode.PrimitiveTopology, textNode.VertexLayout);

            var element = new RenderGroupElement
            {
                Drawable = textNode, 
                ModelMatrix = ModelMatrixStack.Peek(),
                VertexBuffer = vertexBuffer,
                IndexBuffer = indexBuffer
                
            };

            renderGroupState.Elements.Add(element);
        }
        
        private bool IsCulled(BoundingBox bb)
        {
            // Is this bounding box culled?
            if (!CullingFrustum.Contains(bb)) return true;

            return false;
        }
    }
}