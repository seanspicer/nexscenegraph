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

using System.Collections.Generic;
using System.Numerics;

namespace Veldrid.SceneGraph.Util
{
    public class IntersectionVisitor : NodeVisitor
    {
        private Intersector _intersector;
        private bool _eyePointDirty = true;
        private Stack<Matrix4x4> _viewMatrixStack = new Stack<Matrix4x4>();
        private Stack<Matrix4x4> _modelMatrixStack = new Stack<Matrix4x4>();
        
        public IntersectionVisitor(
            Intersector intersector,
            VisitorType type = VisitorType.IntersectionVisitor, 
            TraversalModeType traversalMode = TraversalModeType.TraverseActiveChildren) 
            : base(type, traversalMode)
        {
            _intersector = intersector;
        }

        protected void Intersect(Drawable drawable)
        {
            _intersector.Intersect(this, drawable);
        }

        protected bool Enter(Node node)
        {
            return _intersector.Enter(node);
        }

        protected void Leave()
        {
            _intersector.Leave();
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
            _eyePointDirty = true;
        }

        private void PopMatrix(Stack<Matrix4x4> stack)
        {
            stack.Pop();
            _eyePointDirty = true;
        }

        protected void Reset()
        {
            _intersector.Reset();
        }
        
        public override void Apply(Node node)
        {
            if (false == Enter(node)) return;

            Traverse(node);

            Leave();
        }

        public override void Apply(Geode geode)
        {
            if (false == Enter(geode)) return;

            foreach (var drawable in geode.Drawables)
            {
                Intersect(drawable);
            }

            Leave();
        }

        public override void Apply(Transform transform)
        {
            if (false == Enter(transform)) return;

            var localToWorld = Matrix4x4.Identity;
            transform.ComputeLocalToWorldMatrix(ref localToWorld, this);

            if (transform.ReferenceFrame != Transform.ReferenceFrameType.Relative)
            {
                PushViewMatrix(Matrix4x4.Identity);
            }
            
            PushModelMatrix(localToWorld);
            
            Traverse(transform);
            
            PopModelmatrix();
            
            if (transform.ReferenceFrame != Transform.ReferenceFrameType.Relative)
            {
                PopViewMatrix();
            }

            Leave();
        }

        public override void Apply(Billboard billboard)
        {
            // TODO IMPLEMENT FOR BILLBOARDS
            base.Apply(billboard);
        }
    }
}