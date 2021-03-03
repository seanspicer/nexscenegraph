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
using System.Collections.Generic;
using System.Numerics;

namespace Veldrid.SceneGraph.Util.Shape
{
    internal class Face
    {
        public List<uint> ColorIndices;
        public List<uint> NormalIndices;
        public List<uint> TexCoordIndices;
        public List<uint> VertexIndices;

        internal Face()
        {
            VertexIndices = new List<uint>();
            NormalIndices = new List<uint>();
            TexCoordIndices = new List<uint>();
            ColorIndices = new List<uint>();
        }
    }

    public class BuildShapeGeometryVisitor<T> : IShapeVisitor where T : struct, ISettablePrimitiveElement
    {
        private readonly Vector3[] _colors;
        private readonly IGeometry<T> _geometry;
        private readonly uint _instanceCount;
        private readonly ITessellationHints _tessellationHints;

        internal BuildShapeGeometryVisitor(IGeometry<T> geometry, ITessellationHints tessellationHints,
            Vector3[] colors, uint instanceCount)
        {
            _geometry = geometry;
            _tessellationHints = tessellationHints;
            _colors = colors;
            _instanceCount = instanceCount;
        }

        public void Apply(IShape shape)
        {
            throw new NotImplementedException();
        }

        public void Apply(IBox box)
        {
            var builder = new BuildBoxGeometry<T>();
            builder.Build(_geometry, _tessellationHints, _colors, _instanceCount, box);
        }

        public void Apply(ISphere sphere)
        {
            var builder = new BuildSphereGeometry<T>();
            builder.Build(_geometry, _tessellationHints, _colors, _instanceCount, sphere);
        }

        public void Apply(ICone cone)
        {
            var builder = new BuildConeGeometry<T>();
            builder.Build(_geometry, _tessellationHints, _colors, cone);
        }

        public void Apply(ICylinder cylinder)
        {
            var builder = new BuildCylinderGeometry<T>();
            builder.Build(_geometry, _tessellationHints, _colors, cylinder);
        }

        public void Apply(IPath path)
        {
            var builder = new PathGeometryBuilder<T>();
            builder.Build(_geometry, _tessellationHints, _colors, path);
        }

        internal IShapeVisitor Create(IGeometry<T> geometry, ITessellationHints tessellationHints, Vector3[] colors,
            uint instanceCount)
        {
            return new BuildShapeGeometryVisitor<T>(geometry, tessellationHints, colors, instanceCount);
        }

        private void SetMatrix(Matrix4x4 m)
        {
            throw new NotImplementedException();
        }
    }
}