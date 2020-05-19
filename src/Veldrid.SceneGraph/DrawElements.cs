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

namespace Veldrid.SceneGraph
{
    public class DrawElements<T> : PrimitiveSet where T : struct, IPrimitiveElement
    {
        private readonly uint _indexCount = 0;
        private readonly uint _instanceCount = 0;
        private readonly uint _indexStart = 0;
        private readonly int _vertexOffset = 0;
        private readonly uint _instanceStart = 0;

        private readonly IGeometry<T> _geometry;

        public static IPrimitiveSet Create(
            IGeometry<T> geometry, 
            PrimitiveTopology primitiveTopology, 
            uint indexCount, 
            uint instanceCount, 
            uint indexStart, 
            int vertexOffset, 
            uint instanceStart)
        {
            return new DrawElements<T>(
                geometry,
                primitiveTopology,
                indexCount,
                instanceCount,
                indexStart, 
                vertexOffset, 
                instanceStart);
        }
         
        protected DrawElements(
            IGeometry<T> geometry, 
            PrimitiveTopology primitiveTopology, 
            uint indexCount, 
            uint instanceCount, 
            uint indexStart, 
            int vertexOffset, 
            uint instanceStart)
            : base(geometry, primitiveTopology) 
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
        
        protected override IBoundingBox ComputeBoundingBox()
        {
            var bb = BoundingBox.Create();
            for(var idx = _indexStart; idx < (_indexStart+_indexCount); ++idx)
            {
                bb.ExpandBy(_geometry.VertexData[_geometry.IndexData[idx]].VertexPosition);
            }

            return bb;
        }
    }
}