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
    public class Geometry<T> : Drawable 
        where T : struct, IPrimitiveElement
    {
        public byte[] VertexShader { get; set; }
        public string VertexShaderEntryPoint { get; set; }
        public byte[] FragmentShader { get; set; }
        public string FragmentShaderEntryPoint { get; set; }
        public byte[] TextureBytes { get; set; }
        

        public T[] VertexData { get; set; }
        public int SizeOfVertexData => Marshal.SizeOf(default(T));
        
        public ushort[] IndexData { get; set; }

        public PrimitiveTopology PrimitiveTopology { get; set; }

        public VertexLayoutDescription VertexLayout { get; set; }
            
        private uint NumIndices { get; set; }
        
        private bool _dirtyFlag = true;
        
        public Geometry()
        {
            PrimitiveTopology = PrimitiveTopology.TriangleList;
            _stateSet = new StateSet();
        }
        
        // Required for double-dispatch
        public override void Accept(NodeVisitor visitor)
        {
            visitor.Apply(this);
        }

        protected override void DrawImplementation(RenderInfo renderInfo)
        {
            if (_dirtyFlag)
            {                
                var vbDescription = new BufferDescription(
                    (uint) (VertexData.Length * SizeOfVertexData),
                    BufferUsage.VertexBuffer);
    
                renderInfo.VertexBuffer = renderInfo.ResourceFactory.CreateBuffer(vbDescription);
                renderInfo.GraphicsDevice.UpdateBuffer(renderInfo.VertexBuffer, 0, VertexData);
                
                var ibDescription = new BufferDescription(
                    (uint) IndexData.Length * sizeof(ushort),
                    BufferUsage.IndexBuffer);
    
                NumIndices = (uint) IndexData.Length;
                renderInfo.IndexBuffer = renderInfo.ResourceFactory.CreateBuffer(ibDescription);
                renderInfo.GraphicsDevice.UpdateBuffer(renderInfo.IndexBuffer, 0, IndexData);
    
                _dirtyFlag = false;
            }
            

            
            // Set all relevant state to draw our quad.
            renderInfo.CommandList.SetVertexBuffer(0, renderInfo.VertexBuffer);
            renderInfo.CommandList.SetIndexBuffer(renderInfo.IndexBuffer, IndexFormat.UInt16); 
            
            // Issue a Draw command for a single instance with 4 indices.
            renderInfo.CommandList.DrawIndexed(
                indexCount: NumIndices,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);
        }

        protected override BoundingBox ComputeBoundingBox()
        {
            var bb = new BoundingBox();
            foreach (var elt in VertexData)
            {
                bb.ExpandBy(elt.VertexPosition);
            }

            return bb;
        }
    }
}