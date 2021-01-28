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
using System.Reactive;

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
        
        public static Matrix4x4 PreMultiplyScale(this Matrix4x4 mat, Vector3 v)
        {
            mat.M11 *= v.X; mat.M12 *= v.X; mat.M13 *= v.X; mat.M14 *= v.X;
            mat.M21 *= v.Y; mat.M22 *= v.Y; mat.M23 *= v.Y; mat.M24 *= v.Y;
            mat.M31 *= v.Z; mat.M32 *= v.Z; mat.M33 *= v.Z; mat.M34 *= v.Z;

            return mat;
        }

        public static Matrix4x4 PostMultiplyScale(this Matrix4x4 mat, Vector3 v)
        {
            mat.M11 *= v.X; mat.M21 *= v.X; mat.M31 *= v.X; mat.M41 *= v.X;
            mat.M12 *= v.Y; mat.M22 *= v.Y; mat.M32 *= v.Y; mat.M42 *= v.Y;
            mat.M13 *= v.Z; mat.M23 *= v.Z; mat.M33 *= v.Z; mat.M43 *= v.Z;

            return mat;
        }

        public static Matrix4x4 PreMultiplyTranslate(this Matrix4x4 mat, Vector3 v)
        {
            var tol = 1e-6;
            Matrix4x4 result = mat;
            
            if (System.Math.Abs(v.X) > tol)
            {
                result.M41 += (v.X * result.M11);
                result.M42 += (v.X * result.M12);
                result.M43 += (v.X * result.M13);
                result.M44 += (v.X * result.M14);
            }

            if (System.Math.Abs(v.Y) > tol)
            {
                result.M41 += (v.Y * result.M21);
                result.M42 += (v.Y * result.M22);
                result.M43 += (v.Y * result.M23);
                result.M44 += (v.Y * result.M24);
            }

            if (System.Math.Abs(v.Z) > tol)
            {
                result.M41 += (v.Z * result.M31);
                result.M42 += (v.Z * result.M32);
                result.M43 += (v.Z * result.M33);
                result.M44 += (v.Z * result.M34);
            }
            
            return result;
        }
        
        public static Matrix4x4 PostMultiplyTranslate(this Matrix4x4 mat, Vector3 v)
        {
            var tol = 1e-6;
            Matrix4x4 result = mat;
            
            if (System.Math.Abs(v.X) > tol)
            {
                result.M11 += (v.X * result.M14);
                result.M21 += (v.X * result.M24);
                result.M31 += (v.X * result.M34);
                result.M41 += (v.X * result.M44);
            }

            if (System.Math.Abs(v.Y) > tol)
            {
                result.M12 += (v.Y * result.M14);
                result.M22 += (v.Y * result.M24);
                result.M32 += (v.Y * result.M34);
                result.M42 += (v.Y * result.M44);
            }

            if (System.Math.Abs(v.Z) > tol)
            {
                result.M13 += (v.Z * result.M14);
                result.M23 += (v.Z * result.M24);
                result.M33 += (v.Z * result.M34);
                result.M43 += (v.Z * result.M44);
            }
            
            return result;
        }

        public static Matrix4x4 PostMultiplyRotate(this Matrix4x4 mat , Quaternion q)
        {
            var r = Matrix4x4.CreateFromQuaternion(q);
            return mat.PostMultiply(r);
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

            zFar  = (mat.M43) / mat.M33;
            zNear   = (mat.M43 - 1.0f) / mat.M33;

            left   = -(1.0f + mat.M41) / mat.M11;
            right  =  (1.0f - mat.M41) / mat.M11;

            bottom = -(1.0f + mat.M42) / mat.M22;
            top    =  (1.0f - mat.M42) / mat.M22;

            return true;
        }

        public static void GetLookAt(this Matrix4x4 mat, out Vector3 eye, out Vector3 center, out Vector3 up,
            float lookDistance)
        { 
            Matrix4x4.Invert(mat, out var inv);
            eye = inv.PreMultiply(Vector3.Zero);
            up = VectorExtensions.Transform3X3(Vector3.UnitY, mat);
            var c = VectorExtensions.Transform3X3(-Vector3.UnitZ, mat);
            c = Vector3.Normalize(c);

            center = eye + c * lookDistance;
            
        }
    }
}