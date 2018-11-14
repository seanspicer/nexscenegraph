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

using System.Numerics;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Viewer;

namespace Veldrid.SceneGraph
{
    public class MatrixTransform : Transform
    {
        private Matrix4x4 _matrix = Matrix4x4.Identity;

        public Matrix4x4 Matrix
        {
            get => _matrix;
            set { 
                _matrix = value;
                _inverseDirty = true;
                DirtyBound();
            }
        }

        private bool _inverseDirty = true;
        private Matrix4x4 _inverse = Matrix4x4.Identity;
        public Matrix4x4 Inverse
        {
            get
            {
                if (_inverseDirty)
                {
                    _inverseDirty = !Matrix4x4.Invert(Matrix, out _inverse);
                    
                }

                return _inverse;
            }
        }

        public virtual void PreMultiply(Matrix4x4 mat)
        {
            _matrix = _matrix.PreMultiply(mat);
            _inverseDirty = true;
            DirtyBound();
        }
        
        public virtual void PostMultiply(Matrix4x4 mat)
        {
            _matrix = _matrix.PostMultiply(mat);
            _inverseDirty = true;
            DirtyBound();
        }
        
        public override bool ComputeLocalToWorldMatrix(ref Matrix4x4 matrix, NodeVisitor visitor)
        {
            if (ReferenceFrame == ReferenceFrameType.Relative)
            {
                // PreMultiply
                matrix = matrix.PreMultiply(_matrix);
            }
            else // absolute
            {
                matrix = _matrix;
            }
            return true;
        }

        public override bool ComputeWorldToLocalMatrix(ref Matrix4x4 matrix, NodeVisitor visitor)
        {
            if (ReferenceFrame == ReferenceFrameType.Relative)
            {
                matrix = matrix.PostMultiply(Inverse);
            }
            else // absolute
            {
                matrix = Inverse;
            }
            return true;
        }
    }
}