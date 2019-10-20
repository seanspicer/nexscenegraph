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