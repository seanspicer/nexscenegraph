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

using System;
using System.Numerics;

namespace Veldrid.SceneGraph
{
    public interface IPrimitiveFunctor
    {
        IDrawable Drawable { get; }

        void Draw(
            PrimitiveTopology topology,
            uint indexCount,
            uint instanceCount,
            uint indexStart,
            int vertexOffset,
            uint instanceStart);
    }

    public interface IPrimitiveFunctor<T> : IPrimitiveFunctor where T : struct, IPrimitiveElement
    {
    }

    public abstract class PrimitiveFunctor<T> : IPrimitiveFunctor<T> where T : struct, IPrimitiveElement
    {
        public abstract IDrawable Drawable { get; }

        public abstract void Draw(
            PrimitiveTopology topology,
            uint indexCount,
            uint instanceCount,
            uint indexStart,
            int vertexOffset,
            uint instanceStart);
    }

    public interface IPrimitiveFunctorDelegate
    {
        void Handle(Vector3 v0, Vector3 v1, Vector3 v2, bool treatVertexDataAsTemporary);
    }

    internal class TemplatePrimitiveFunctor<T> : PrimitiveFunctor<T> where T : struct, IPrimitiveElement
    {
        private readonly IGeometry<T> _geometry;
        private readonly IPrimitiveFunctorDelegate _pfd;

        internal TemplatePrimitiveFunctor(IPrimitiveFunctorDelegate pfd, IGeometry<T> geometry)
        {
            _geometry = geometry;
            _pfd = pfd;
        }

        public override IDrawable Drawable => _geometry;

        public override void Draw(PrimitiveTopology topology, uint indexCount, uint instanceCount, uint indexStart,
            int vertexOffset,
            uint instanceStart)
        {
            if (null == _pfd) return;

            switch (topology)
            {
                case PrimitiveTopology.TriangleList:
                {
                    for (var i = indexStart; i < indexStart + indexCount; i += 3)
                        _pfd.Handle(
                            _geometry.VertexData[_geometry.IndexData[i + 0]].VertexPosition,
                            _geometry.VertexData[_geometry.IndexData[i + 1]].VertexPosition,
                            _geometry.VertexData[_geometry.IndexData[i + 2]].VertexPosition,
                            false);
                    break;
                }

                case PrimitiveTopology.LineStrip:
                    // Not implemented
                    break;

                case PrimitiveTopology.LineList:
                    // Not implemented
                    break;
                
                default:
                    throw new NotImplementedException($"TODO: Implement me! PrimitiveTopology = {topology}.");
            }
        }
    }
}