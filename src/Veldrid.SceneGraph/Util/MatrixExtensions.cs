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
using Vulkan;

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

        public static bool GetFrustum(this Matrix4x4 mat, 
                ref float left, ref float right, 
                ref float bottom, ref float top,
                ref float zNear, ref float zFar)
        {
            const double tol = 1e-6;
            
            if (System.Math.Abs(mat.M14) > tol || 
                System.Math.Abs(mat.M24) > tol || 
                System.Math.Abs(mat.M34 - (-1.0f)) > tol || 
                System.Math.Abs(mat.M44) > tol)
                return false;

            var tempNear = mat.M43 / (mat.M33-1.0);
            var tempFar = mat.M43 / (1.0+mat.M33);

            left  = (float) tempNear * (mat.M31 - 1.0f) / mat.M11;
            right = (float) tempNear * (1.0f + mat.M31) / mat.M11;

            top    = (float) tempNear * (1.0f + mat.M32) / mat.M22;
            bottom = (float) tempNear * (mat.M32 - 1.0f) / mat.M22;

            zNear = (float) tempNear;
            zFar = (float) tempFar;

            return true;
        }

        public static bool GetOrtho(this Matrix4x4 mat,
            ref float left, ref float right,
            ref float bottom, ref float top,
            ref float zNear, ref float zFar)
        {
            const double tol = 1e-6;
            
            if (System.Math.Abs(mat.M14) > tol || 
                System.Math.Abs(mat.M24) > tol ||
                System.Math.Abs(mat.M34) > tol ||
                System.Math.Abs(mat.M44 - 1.0) > tol) 
                return false;

            zNear  = (mat.M43) / mat.M33;
            zFar   = (mat.M43 - 1.0f) / mat.M33;

            left   = -(1.0f + mat.M41) / mat.M11;
            right  =  (1.0f - mat.M41) / mat.M11;

            bottom = -(1.0f + mat.M42) / mat.M22;
            top    =  (1.0f - mat.M42) / mat.M22;

            return true;
        }
    }
}