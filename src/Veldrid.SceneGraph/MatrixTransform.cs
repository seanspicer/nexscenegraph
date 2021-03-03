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

using System.Numerics;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph
{
    public interface IMatrixTransform : ITransform
    {
        Matrix4x4 Matrix { get; set; }
        Matrix4x4 Inverse { get; }
        void PreMultiply(Matrix4x4 mat);
        void PostMultiply(Matrix4x4 mat);
    }

    public class MatrixTransform : Transform, IMatrixTransform
    {
        private Matrix4x4 _inverse = Matrix4x4.Identity;

        private bool _inverseDirty = true;
        private Matrix4x4 _matrix = Matrix4x4.Identity;

        protected MatrixTransform(Matrix4x4 matrix)
        {
            Matrix = matrix;
        }

        public Matrix4x4 Matrix
        {
            get => _matrix;
            set
            {
                _matrix = value;
                _inverseDirty = true;
                DirtyBound();
            }
        }

        public Matrix4x4 Inverse
        {
            get
            {
                if (_inverseDirty) _inverseDirty = !Matrix4x4.Invert(Matrix, out _inverse);

                return _inverse;
            }
        }

        public void PreMultiply(Matrix4x4 mat)
        {
            _matrix = _matrix.PreMultiply(mat);
            _inverseDirty = true;
            DirtyBound();
        }

        public void PostMultiply(Matrix4x4 mat)
        {
            _matrix = _matrix.PostMultiply(mat);
            _inverseDirty = true;
            DirtyBound();
        }

        public override bool ComputeLocalToWorldMatrix(ref Matrix4x4 matrix, NodeVisitor visitor)
        {
            if (ReferenceFrame == ReferenceFrameType.Relative)
                // PreMultiply
                matrix = matrix.PreMultiply(_matrix);
            else // absolute
                matrix = _matrix;
            return true;
        }

        public override bool ComputeWorldToLocalMatrix(ref Matrix4x4 matrix, NodeVisitor visitor)
        {
            if (ReferenceFrame == ReferenceFrameType.Relative)
                matrix = matrix.PostMultiply(Inverse);
            else // absolute
                matrix = Inverse;
            return true;
        }

        public static IMatrixTransform Create(Matrix4x4 matrix)
        {
            return new MatrixTransform(matrix);
        }
    }
}