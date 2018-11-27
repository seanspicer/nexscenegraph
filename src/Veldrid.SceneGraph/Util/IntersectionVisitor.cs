//
// Copyright 2018 Sean Spicer 
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

using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Veldrid.SceneGraph.Util
{
    public class IntersectionVisitor : NodeVisitor, IIntersectionVisitor
    {
        private Stack<IIntersector> _intersectorStack = new Stack<IIntersector>();
        private Stack<Matrix4x4> _viewMatrixStack = new Stack<Matrix4x4>();
        private Stack<Matrix4x4> _modelMatrixStack = new Stack<Matrix4x4>();

        public static IIntersectionVisitor Create(
                IIntersector intersector,
                VisitorType type = VisitorType.IntersectionVisitor, 
                TraversalModeType traversalMode = TraversalModeType.TraverseActiveChildren) 
            
        {
            return new IntersectionVisitor(intersector, type, traversalMode);
        }
        
        protected IntersectionVisitor(
            IIntersector intersector,
            VisitorType type = VisitorType.IntersectionVisitor, 
            TraversalModeType traversalMode = TraversalModeType.TraverseActiveChildren) 
            : base(type, traversalMode)
        {
            SetIntersector(intersector);
        }

        public void SetIntersector(IIntersector intersector)
        {
            _intersectorStack.Clear();
            
            if (null != intersector)
            {
                _intersectorStack.Push(intersector);
            }
        }
        
        public Matrix4x4 GetModelMatrix()
        {
            return _modelMatrixStack.Any() ? _modelMatrixStack.Peek() : Matrix4x4.Identity;
        }

        public Matrix4x4 GetViewMatrix()
        {
            return _viewMatrixStack.Any() ? _viewMatrixStack.Peek() : Matrix4x4.Identity;
        }
        
        protected void Intersect(IDrawable drawable)
        {
            _intersectorStack.Peek().Intersect(this, drawable);
        }

        protected bool Enter(INode node)
        {
            return _intersectorStack.Peek().Enter(node);
        }

        protected void Leave()
        {
            _intersectorStack.Peek().Leave();
        }

        protected void PushClone()
        {
            _intersectorStack.Push(_intersectorStack.Last().Clone(this));
        }

        protected void PopClone()
        {
            if (_intersectorStack.Count >= 2)
            {
                _intersectorStack.Pop();
            }
        }
        
        protected void PushViewMatrix(Matrix4x4 viewMatrix)
        {
            PushMatrix(viewMatrix, _viewMatrixStack);
        }

        protected void PopViewMatrix()
        {
            PopMatrix(_viewMatrixStack);
        }

        protected void PushModelMatrix(Matrix4x4 modelMatrix)
        {
            PushMatrix(modelMatrix, _modelMatrixStack);
        }

        protected void PopModelmatrix()
        {
            PopMatrix(_modelMatrixStack);
        }

        private void PushMatrix(Matrix4x4 matrix, Stack<Matrix4x4> stack)
        {
            stack.Push(matrix);
        }

        private void PopMatrix(Stack<Matrix4x4> stack)
        {
            stack.Pop();
        }

        protected void Reset()
        {
            var intersector = _intersectorStack.First();
            intersector.Reset();
            SetIntersector(intersector);
        }
        
        public override void Apply(INode node)
        {
            if (false == Enter(node)) return;

            Traverse(node);

            Leave();
        }

        public override void Apply(IGeode geode)
        {
            if (false == Enter(geode)) return;

            foreach (var drawable in geode.Drawables)
            {
                Intersect(drawable);
            }

            Leave();
        }

        public override void Apply(ITransform transform)
        {
            if (false == Enter(transform)) return;

            var curModel = _modelMatrixStack.Any() ? _modelMatrixStack.Peek() : Matrix4x4.Identity;
            
            
            transform.ComputeLocalToWorldMatrix(ref curModel, this);

            if (transform.ReferenceFrame != Transform.ReferenceFrameType.Relative)
            {
                PushViewMatrix(Matrix4x4.Identity);
            }
            
            PushModelMatrix(curModel);
            
            PushClone();
            
            Traverse(transform);
            
            PopClone();
            
            PopModelmatrix();
            
            if (transform.ReferenceFrame != Transform.ReferenceFrameType.Relative)
            {
                PopViewMatrix();
            }

            Leave();
        }

        public override void Apply(IBillboard billboard)
        {
            // TODO IMPLEMENT FOR BILLBOARDS
            base.Apply(billboard);
        }
    }
}