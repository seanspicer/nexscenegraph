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

using System;
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
            
            var vx = new Matrix4x4(
                 0.0f, -v.Z,   v.Y,  0.0f,
                 v.Z,   0.0f, -v.X,  0.0f,
                -v.Y,   v.X,   0.0f, 0.0f,
                 0.0f,  0.0f,  0.0f, 0.0f);

            var vxx = Matrix4x4.Multiply(vx, vx);
            
            var tmp1 = Matrix4x4.Identity + vx;
            var tmp2 = vxx * (float) m;
            var result = tmp1 + tmp2;

            return Matrix4x4.Transpose(result);
        }

        public static Vector3[] ComputePathTangents(Vector3[] trajectory)
        {
            var nVerts = trajectory.Length;
         
            if(nVerts < 2) throw new ArgumentException("Not enough points in trajectory");
            
            var tangents = new Vector3[nVerts];

            tangents[0] = Vector3.Subtract(trajectory[1],trajectory[0]);
            for (var i = 1; i < nVerts - 1; ++i)
            {
                tangents[i] = Vector3.Subtract(trajectory[i + 1],trajectory[i-1]);
            }
            tangents[nVerts-1] = Vector3.Subtract(trajectory[nVerts-1],trajectory[nVerts-2]);

            return tangents;
        }

        public static Vector3[,] ExtrudeShape(Vector2[] shape, Vector3[] path)
        {
            var nSegments = shape.Length;

            var shape3 = new Vector3[nSegments];
            for (var i = 0; i < nSegments; ++i)
            {
                shape3[i] = new Vector3(shape[i], 0.0f);
            }

            var tangents = ComputePathTangents(path);
            
            var extrusion = new Vector3[path.Length, nSegments];
            
            var axialVec = Vector3.UnitZ;
            for (var i = 0; i < path.Length; ++i)
            {
                var unitTangent = Vector3.Normalize(tangents[i]);
                var z = Vector3.Cross(axialVec, unitTangent);

                if (System.Math.Abs(z.Length()) > 1e-6)
                {
                    // Determine the required rotation, and build quaternion
                    var znorm = Vector3.Normalize(z);
                    var q = System.Math.Acos(Vector3.Dot(axialVec, unitTangent) / axialVec.Length());
                    var quat = Quaternion.CreateFromAxisAngle(znorm, (float)q);

                    // Transform shape by quaternion.
                    for (var j = 0; j < shape3.Length; ++j)
                    {
                        shape3[j] = Vector3.Transform(shape3[j], quat);
                    }

                    axialVec = unitTangent;
                }

                for (var j = 0; j < shape3.Length; ++j)
                {
                    extrusion[i, j] = path[i] + shape3[j];
                }
            }

            return extrusion;
        }
    }
}