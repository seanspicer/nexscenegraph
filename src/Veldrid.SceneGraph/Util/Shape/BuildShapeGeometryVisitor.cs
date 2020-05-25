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
using System.Linq;
using System.Numerics;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Util.Shape
{
    internal class Face
    {
        public List<uint> VertexIndices;
        public List<uint> NormalIndices;
        public List<uint> TexCoordIndices;
        public List<uint> ColorIndices;

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
        private IGeometry<T> _geometry;
        private ITessellationHints _tessellationHints;
        private Vector3 [] _colors;
        
        internal IShapeVisitor Create(IGeometry<T> geometry, ITessellationHints tessellationHints, Vector3 [] colors)
        {
            return new BuildShapeGeometryVisitor<T>(geometry, tessellationHints, colors);
        }

        internal BuildShapeGeometryVisitor(IGeometry<T> geometry, ITessellationHints tessellationHints, Vector3 [] colors)
        {
            _geometry = geometry;
            _tessellationHints = tessellationHints;
            _colors = colors;
        }

        void SetMatrix(Matrix4x4 m)
        {
            throw new System.NotImplementedException();
        }
        
        public void Apply(IShape shape)
        {
            throw new System.NotImplementedException();
        }

        public void Apply(IBox box)
        {
            BuildBoxGeometry<T>.Build(_geometry, _tessellationHints, _colors, box);
        }
        
        public void Apply(ISphere sphere)
        {
            var builder = new BuildSphereGeometry<T>(); 
            builder.Build(_geometry, _tessellationHints, _colors, sphere);
        }

        public void Apply(ICone cone)
        {
            var builder = new BuildConeGeometry<T>();
            builder.Build(_geometry, _tessellationHints, _colors, cone);
        }

        public void Apply(IPath path)
        {
            var builder = new PathGeometryBuilder<T>();
            builder.Build(_geometry, _tessellationHints, _colors, path);
        }
    }
}