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
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph
{
    public interface IComputeBoundsVisitor : INodeVisitor
    {
        void Reset();

        IBoundingBox GetBoundingBox();

        void GetPolytope(ref IPolytope polytope, float margin = 0.1f);

        void GetBase(ref IPolytope polytope, float margin = 0.1f);
    }

    public class ComputeBoundsVisitor : NodeVisitor, IComputeBoundsVisitor
    {
        protected IBoundingBox _bb;

        protected Stack<Matrix4x4> _matrixStack = new Stack<Matrix4x4>();

        protected ComputeBoundsVisitor(
            TraversalModeType traversalMode = TraversalModeType.TraverseAllChildren)
            : base(traversalMode)
        {
            _bb = BoundingBox.Create();
            Reset();
        }

        public virtual void Reset()
        {
            _matrixStack.Clear();
            _bb.Init();
        }

        public IBoundingBox GetBoundingBox()
        {
            return _bb;
        }

        public void GetPolytope(ref IPolytope polytope, float margin)
        {
            var delta = _bb.Radius * margin;
            polytope.Add(Plane.Create(0.0f, 0.0f, 1.0f, -(_bb.ZMin - delta)));
            polytope.Add(Plane.Create(0.0f, 0.0f, -1.0f, _bb.ZMax + delta));

            polytope.Add(Plane.Create(1.0f, 0.0f, 0.0f, -(_bb.XMin - delta)));
            polytope.Add(Plane.Create(-1.0f, 0.0f, 0.0f, _bb.XMax + delta));

            polytope.Add(Plane.Create(0.0f, 1.0f, 0.0f, -(_bb.YMin - delta)));
            polytope.Add(Plane.Create(0.0f, -1.0f, 0.0f, _bb.YMax + delta));
        }

        public void GetBase(ref IPolytope polytope, float margin)
        {
            var delta = _bb.Radius * margin;
            polytope.Add(Plane.Create(0.0f, 0.0f, 1.0f, -(_bb.ZMin - delta)));
        }

        public override void Apply(IDrawable drawable)
        {
            ApplyBoundingBox(drawable.GetBoundingBox());
        }

        public override void Apply(ITransform transform)
        {
            var matrix = Matrix4x4.Identity;
            if (_matrixStack.Count > 0) matrix = _matrixStack.Peek();

            transform.ComputeLocalToWorldMatrix(ref matrix, this);

            PushMatrix(matrix);

            Traverse(transform);

            PopMatrix();
        }

        public static IComputeBoundsVisitor Create()
        {
            return new ComputeBoundsVisitor();
        }

        public void PushMatrix(Matrix4x4 matrix)
        {
            _matrixStack.Push(matrix);
        }

        public void PopMatrix()
        {
            _matrixStack.Pop();
        }

        private void ApplyBoundingBox(IBoundingBox bbox)
        {
            if (0 == _matrixStack.Count)
            {
                _bb.ExpandBy(bbox);
            }
            else if (bbox.Valid())
            {
                var matrix = _matrixStack.Peek();
                _bb.ExpandBy(matrix.PreMultiply(bbox.Corner(0)));
                _bb.ExpandBy(matrix.PreMultiply(bbox.Corner(1)));
                _bb.ExpandBy(matrix.PreMultiply(bbox.Corner(2)));
                _bb.ExpandBy(matrix.PreMultiply(bbox.Corner(3)));
                _bb.ExpandBy(matrix.PreMultiply(bbox.Corner(4)));
                _bb.ExpandBy(matrix.PreMultiply(bbox.Corner(5)));
                _bb.ExpandBy(matrix.PreMultiply(bbox.Corner(6)));
                _bb.ExpandBy(matrix.PreMultiply(bbox.Corner(7)));
            }
            else
            {
                throw new ArgumentException("Invalid BoundingBox");
            }
        }
    }
}