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
using System.Runtime.InteropServices.ComTypes;
using AssetProcessor;
using Veldrid.SceneGraph.Text;
using Veldrid.Utilities;

namespace Veldrid.SceneGraph.RenderGraph
{
    public class CullAndAssembleVisitor : NodeVisitor
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
        //private HashSet<RenderGroupState> PipelineCache { get; set; } = new HashSet<RenderGroupState>();

        public CullAndAssembleVisitor() : 
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
            
            OpaqueRenderGroup.Clear();
            TransparentRenderGroup.Clear();
            
            RenderElementCount = 0;
            
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
            
            Apply((Node)transform);

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

            //
            // TODO - this is a 'trick' to Premultiply geometry to avoid performance hits
            //
            var mm = ModelMatrixStack.Peek();
            var vtxData = new T[geometry.VertexData.Length];
            Array.Copy(geometry.VertexData, vtxData, geometry.VertexData.Length);
            for(var i=0; i<vtxData.Length; ++i)
            {
                vtxData[i].VertexPosition = Vector3.Transform(vtxData[i].VertexPosition, mm);
            }
            
            var vtxBufInfo = GetVertexBufferAndOffset((uint) geometry.VertexData.Length, (uint) geometry.SizeOfVertexData);
            GraphicsDevice.UpdateBuffer(vtxBufInfo.Item2, vtxBufInfo.Item3*(uint)geometry.SizeOfVertexData, vtxData);

            var idxBufInfo = GetIndexBufferAndOffset((uint) geometry.IndexData.Length, sizeof(ushort));
            GraphicsDevice.UpdateBuffer(idxBufInfo.Item2, idxBufInfo.Item3*sizeof(ushort), geometry.IndexData);
            
            // Construct Render Group element
            var element = new RenderGroupElement
            {
                Drawable = geometry, 
                ModelMatrix = ModelMatrixStack.Peek(),
                VertexBuffer = vtxBufInfo,
                IndexBuffer = idxBufInfo
            };

            // 
            // Sort into appropriate render group
            // 
            RenderGroupState renderGroupState = null;
            if (pso.BlendStateDescription.AttachmentStates.Contains(BlendAttachmentDescription.AlphaBlend))
            {
                renderGroupState = TransparentRenderGroup.GetOrCreateState(pso, geometry.PrimitiveTopology, geometry.VertexLayout);
            }
            else
            {
                renderGroupState = OpaqueRenderGroup.GetOrCreateState(pso, geometry.PrimitiveTopology, geometry.VertexLayout);
            }
                
            renderGroupState.Elements.Add(element);
            RenderElementCount++;
        }

        public override void Apply(TextNode textNode)
        {
            PipelineState pso = textNode.PipelineState;
            
            //
            // Setup rendering buffers
            //
            var vtxBufInfo = GetVertexBufferAndOffset((uint) textNode.VertexData.Length, (uint) textNode.SizeOfVertexData);
            GraphicsDevice.UpdateBuffer(vtxBufInfo.Item2, vtxBufInfo.Item3* (uint) textNode.SizeOfVertexData, textNode.VertexData);
            
            var idxBufInfo = GetIndexBufferAndOffset((uint) textNode.IndexData.Length, sizeof(ushort));        
            GraphicsDevice.UpdateBuffer(idxBufInfo.Item2, idxBufInfo.Item3*sizeof(ushort), textNode.IndexData);

            
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

            // 
            // Text Nodes always go into the transparent render group
            // 
            var renderGroupState = TransparentRenderGroup.GetOrCreateState(pso, textNode.PrimitiveTopology, textNode.VertexLayout);

            var element = new RenderGroupElement
            {
                Drawable = textNode, 
                ModelMatrix = ModelMatrixStack.Peek(),
                VertexBuffer = vtxBufInfo,
                IndexBuffer = idxBufInfo
                
            };

            renderGroupState.Elements.Add(element);
            RenderElementCount++;
        }

        private Tuple<int, DeviceBuffer, uint> GetVertexBufferAndOffset(uint length, uint vtxSizeInBytes)
        {
            uint sizeInBytes = length * vtxSizeInBytes;
            
            if (VertexBufferList.Count == 0 || _currVertexBufferOffset + sizeInBytes > 128*65536)
            {
                // Need to allocate a new buffer
                if (VertexBufferList.Count <= _currVertexBufferIndex+1)
                {
                    var vbDescription = new BufferDescription(128*65536, BufferUsage.VertexBuffer);
                    var vertexBuffer = ResourceFactory.CreateBuffer(vbDescription);
                    VertexBufferList.Add(vertexBuffer);

                    _currVertexBufferOffset = 0;
                    _currVertexBufferIndex = VertexBufferList.Count - 1;
                }
                else
                {
                    _currVertexBufferIndex++;
                }
            }

            var result = Tuple.Create(_currVertexBufferIndex, VertexBufferList[_currVertexBufferIndex], _currVertexBufferOffset/vtxSizeInBytes);
            
            _currVertexBufferOffset += sizeInBytes;

            return result;
        }
        
        private Tuple<int, DeviceBuffer, uint> GetIndexBufferAndOffset(uint length, uint idxSizeInBytes)
        {
            uint sizeInBytes = length * idxSizeInBytes;
            
            if (IndexBufferList.Count == 0 || _currIndexBufferOffset + sizeInBytes > 128*65536)
            {
                if (IndexBufferList.Count <= _currIndexBufferIndex + 1)
                {
                    var ibDescription = new BufferDescription(128 * 65536, BufferUsage.IndexBuffer);
                    var indexBuffer = ResourceFactory.CreateBuffer(ibDescription);
                    IndexBufferList.Add(indexBuffer);

                    _currIndexBufferOffset = 0;
                    _currIndexBufferIndex = IndexBufferList.Count-1;
                }
                else
                {
                    _currIndexBufferIndex++;
                }

                
            }
            
            
            var result = Tuple.Create(_currIndexBufferIndex, IndexBufferList[_currIndexBufferIndex], _currIndexBufferOffset/idxSizeInBytes);
            _currIndexBufferOffset += sizeInBytes;
            return result;
        }


    }
}