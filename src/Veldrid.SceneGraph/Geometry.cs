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
    public enum PrimitiveTopolgy : byte
    {
        LineList, 
        LineStrip, 
        PointList,
        TriangleList,
        TriangleStrip
    }
    
    public class Geometry<T> : Node 
        where T : struct 
    {
        public byte[] VertexShader { get; set; }
        public byte[] FragmentShader { get; set; }

        public T[] VertexData { get; set; }
        public int SizeOfVertexData => Marshal.SizeOf(default(T));
        
        public ushort[] IndexData { get; set; }

        public PrimitiveTopolgy Topology { get; set; }

        public VertexLayoutDescription VertexLayout { get; set; }
            
        public Geometry()
        {
            Topology = PrimitiveTopolgy.TriangleList;
        }
        
        // Required for double-dispatch
        public override void Accept(NodeVisitor visitor)
        {
            visitor.Apply(this);
        }
    }
}