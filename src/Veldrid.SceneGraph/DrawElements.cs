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

namespace Veldrid.SceneGraph
{
    public class DrawElements<T> : PrimitiveSet where T : struct, IPrimitiveElement
    {
        private readonly uint _indexCount = 0;
        private readonly uint _instanceCount = 0;
        private readonly uint _indexStart = 0;
        private readonly int _vertexOffset = 0;
        private readonly uint _instanceStart = 0;

        private readonly Geometry<T> _geometry;

        public DrawElements(Geometry<T> geometry, PrimitiveTopology primitiveTopology, uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
            : base(primitiveTopology) 
        {
            _geometry = geometry;
            _indexCount    = indexCount;
            _instanceCount = instanceCount;
            _indexStart    = indexStart;
            _vertexOffset  = vertexOffset;
            _instanceStart = instanceStart;
        }
        
        public override void Draw(CommandList commandList)
        {
            commandList.DrawIndexed(
                indexCount:    _indexCount,
                instanceCount: _instanceCount,
                indexStart:    _indexStart,
                vertexOffset:  _vertexOffset,
                instanceStart: _instanceStart);
        }
        
        protected override BoundingBox ComputeBoundingBox()
        {
            var bb = new BoundingBox();
            for(var idx = _indexStart; idx < (_indexStart+_indexCount); ++idx)
            {
                bb.ExpandBy(_geometry.VertexData[idx].VertexPosition);
            }

            return bb;
        }
    }
}