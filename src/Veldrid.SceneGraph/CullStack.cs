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

namespace Veldrid.SceneGraph
{
    public class CullStack : CullSettings
    {
        public Stack<Matrix4x4> ModelViewStack { get; set; }= new Stack<Matrix4x4>();
        private Stack<Matrix4x4> _projectionStack = new Stack<Matrix4x4>();

        private Stack<CullingSet> _modelViewCullingStack = new Stack<CullingSet>();
        public CullingSet _currentCullingSet;
        public CullingSet CurrentCullingSet
        {
            get => _currentCullingSet;
        }
        
        public CullStack()
        {
            
        }

        public Matrix4x4 GetModelViewMatrix()
        {
            return ModelViewStack.Count == 0 ? Matrix4x4.Identity : ModelViewStack.Peek();
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            return _projectionStack.Count == 0 ? Matrix4x4.Identity : _projectionStack.Peek();
        }

        public bool IsCulled(BoundingBox bb)
        {
            return bb.Valid() && CurrentCullingSet.IsCulled(bb);
        }
    }
}