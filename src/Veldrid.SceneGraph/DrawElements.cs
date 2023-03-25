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

using System.Numerics;

namespace Veldrid.SceneGraph
{
    public class DrawElements<T> : PrimitiveSet where T : struct, IPrimitiveElement
    {
        private readonly IGeometry<T> _geometry;
        private readonly uint _indexCount;
        private readonly uint _indexStart;
        private readonly uint _instanceCount;
        private readonly uint _instanceStart;
        private readonly int _vertexOffset;

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
            _indexCount = indexCount;
            _instanceCount = instanceCount;
            _indexStart = indexStart;
            _vertexOffset = vertexOffset;
            _instanceStart = instanceStart;
        }
        
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

        public override IPrimitiveSet DeepCopy()
        {
            return new DrawElements<T>(
                this._geometry,
                this.PrimitiveTopology,
                this._indexCount,
                this._instanceCount,
                this._indexStart,
                this._vertexOffset,
                this._instanceStart);
        }
        
        public override void Draw(CommandList commandList)
        {
            commandList.DrawIndexed(
                _indexCount,
                _instanceCount,
                _indexStart,
                _vertexOffset,
                _instanceStart);
        }

        protected override IBoundingBox ComputeBoundingBox()
        {
            var bb = BoundingBox.Create();
            for (var idx = _indexStart; idx < _indexStart + _indexCount; ++idx)
                bb.ExpandBy(_geometry.VertexData[_geometry.IndexData[idx]].VertexPosition);

            return bb;
        }

        protected override float ComputeDistance(Vector3 point)
        {
            var dist = 0.0f;
            var count = 0f;
            for (var idx = _indexStart; idx < _indexStart + _indexCount; ++idx)
            {
                dist += Vector3.Distance(_geometry.VertexData[_geometry.IndexData[idx]].VertexPosition, point);
                count += 1.0f;
            }

            return dist / count;
        }

        public override void Accept(IPrimitiveFunctor functor)
        {
            functor.Draw(PrimitiveTopology,
                _indexCount,
                _instanceCount,
                _indexStart,
                _vertexOffset,
                _instanceStart);
        }


    }
}