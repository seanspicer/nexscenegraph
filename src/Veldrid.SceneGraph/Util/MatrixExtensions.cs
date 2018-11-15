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

namespace Veldrid.SceneGraph.Util
{
    public static class MatrixExtensions
    {
        // TODO - UNIT TEST
        public static Matrix4x4 PreMultiply(this Matrix4x4 mat, Matrix4x4 other)
        {
            return Matrix4x4.Multiply(other, mat);
        }
        
        // TODO - UNIT TEST
        public static Matrix4x4 PostMultiply(this Matrix4x4 mat, Matrix4x4 other)
        {
            return Matrix4x4.Multiply(mat, other);
        }
        
        // TODO - UNIT TEST
        public static Vector3 PreMultiply(this Matrix4x4 mat, Vector3 v)
        {
            float d = 1.0f/(mat.M14*v.X+mat.M24*v.Y+mat.M34*v.Z+mat.M44) ;
            return new Vector3( (mat.M11*v.X + mat.M21*v.Y + mat.M31*v.Z + mat.M41)*d,
                (mat.M12*v.X + mat.M22*v.Y + mat.M32*v.Z + mat.M42)*d,
                (mat.M13*v.X + mat.M23*v.Y + mat.M33*v.Z + mat.M43)*d);
        }

        // TODO - UNIT TEST
        public static Vector3 PostMultiply(this Matrix4x4 mat, Vector3 v)
        {
            float d = 1.0f/(mat.M41*v.X+mat.M42*v.Y+mat.M43*v.Z+mat.M44) ;
            return new Vector3( (mat.M11*v.X + mat.M12*v.Y + mat.M13*v.Z + mat.M14)*d,
                (mat.M21*v.X + mat.M22*v.Y + mat.M23*v.Z + mat.M24)*d,
                (mat.M31*v.X + mat.M32*v.Y + mat.M33*v.Z + mat.M34)*d) ;
        }
        
        // TODO - UNIT TEST
        public static Matrix4x4 SetTranslation(this Matrix4x4 mat, Vector3 translation)
        {
            mat.M41 = translation.X;
            mat.M42 = translation.Y;
            mat.M43 = translation.Z;
            return mat;
        }
    }
}