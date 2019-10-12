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

using System.Numerics;

namespace Veldrid.SceneGraph.Util
{
    public class Math
    {
        public static float RadiansToDegrees(float angle)
        {
            return angle * 180.0f / (float)System.Math.PI;
        }

        public static Matrix4x4 CalculateRotationBetweenVectors(Vector3 a, Vector3 b)
        {
            var v = Vector3.Cross(a, b);
            var s = v.Length();
            var c = Vector3.Dot(a, b);
            var m = (1 - c) / System.Math.Pow(s, 2);
            
            var Vx = new Matrix4x4(
                 0.0f,  v.Z,   v.Y,  0.0f,
                 v.Z,   0.0f, -v.X,  0.0f,
                -v.Y,   v.X,   0.0f, 0.0f,
                 0.0f,  0.0f,  0.0f, 1.0f);

            //var Vxx = Matrix4x4.Multiply(Vx, Vx);

            var vx2 = (float)System.Math.Pow(v.X, 2);
            var vy2 = (float)System.Math.Pow(v.Y, 2);
            var vz2 = (float)System.Math.Pow(v.Z, 2);
            
            var Vxx = new Matrix4x4(
                0.0f,  vz2,   vy2,  0.0f,
                vz2,   0.0f,  vx2,  0.0f,
                vy2,   vx2,   0.0f, 0.0f,
                0.0f,  0.0f,  0.0f, 1.0f);

            Vxx = Matrix4x4.Multiply(Vx, Vx);
            
            var tmp1 = Matrix4x4.Add(Matrix4x4.Identity, Vxx);
            var tmp2 = Matrix4x4.Multiply(Vxx, (float)m);
            var result = Matrix4x4.Add(tmp1, tmp2);

            return result;
        }
    }
}