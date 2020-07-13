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

namespace Veldrid.SceneGraph.Util.Shape
{
    public interface IShapeDrawable<T> : IGeometry<T> where T : struct, ISettablePrimitiveElement
    {
        
    }
    
    public class ShapeDrawable<T> : Geometry<T>, IShapeDrawable<T> where T : struct, ISettablePrimitiveElement
    {
        private IShape _shape;
        private Vector3 [] _colors;
        private ITessellationHints _hints;
        private uint _instanceCount;
        
        public static IShapeDrawable<T> Create(IShape shape, ITessellationHints hints, uint instanceCount=1)
        {
            return new ShapeDrawable<T>(shape, hints, instanceCount);
        }
        
        public static IShapeDrawable<T> Create(IShape shape, ITessellationHints hints, Vector3 [] colors, uint instanceCount=1)
        {
            return new ShapeDrawable<T>(shape, hints, colors, instanceCount);
        }
        
        protected ShapeDrawable(IShape shape, ITessellationHints hints, uint instanceCount)
        {
            SetInstanceCount(instanceCount);
            
            if (hints.ColorsType == ColorsType.ColorOverall)
            {
                SetColors(new [] {Vector3.One} );
            } 
            else if (hints.ColorsType == ColorsType.ColorPerFace)
            {
                var colors = new List<Vector3>();
                if (shape is IBox)
                {
                    for (var f = 0; f < 6; ++f)
                    {
                        colors.Append(Vector3.One);
                    }
                    SetColors(colors.ToArray());
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (hints.ColorsType == ColorsType.ColorPerVertex)
            {
                var colors = new List<Vector3>();
                if (shape is IBox)
                {
                    for (var f = 0; f < 24; ++f)
                    {
                        colors.Append(Vector3.One);
                    }
                    SetColors(colors.ToArray());
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            
            SetShape(shape);
            SetTessellationHints(hints);
            Build();
        }
        
        protected ShapeDrawable(IShape shape, ITessellationHints hints, Vector3 [] colors, uint instanceCount)
        {
            SetInstanceCount(instanceCount);
            SetColors(colors);
            SetShape(shape);
            SetTessellationHints(hints);
            Build();
        }

        private void SetShape(IShape shape)
        {
            _shape = shape;
        }

        private void SetColors(Vector3 [] colors)
        {
            _colors = colors;
        }

        private void SetTessellationHints(ITessellationHints hints)
        {
            _hints = hints;
        }

        private void SetInstanceCount(uint instanceCount)
        {
            _instanceCount = instanceCount;
        }
        
        private void Build()
        {
            var shapeGeometryVisitor = new BuildShapeGeometryVisitor<T>(this, _hints, _colors, _instanceCount);
            _shape.Accept(shapeGeometryVisitor);
        }
    }
}