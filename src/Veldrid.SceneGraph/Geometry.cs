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

        public T[] VertexData { get; set; }
        public int SizeOfVertexData => Marshal.SizeOf(default(T));
        
        public ushort[] IndexData { get; set; }

        public PrimitiveTopology PrimitiveTopology { get; set; }

        public VertexLayoutDescription VertexLayout { get; set; }
            
        public Geometry()
        {
            PrimitiveTopology = PrimitiveTopology.TriangleList;
        }
        
        // Required for double-dispatch
        public override void Accept(NodeVisitor visitor)
        {
            visitor.Apply(this);
        }

        public override void Draw(RenderInfo renderInfo)
        {
            throw new NotImplementedException();
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