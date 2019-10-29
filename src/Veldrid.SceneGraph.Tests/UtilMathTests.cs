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
using NUnit.Framework;

namespace Veldrid.SceneGraph.Tests
{
    [TestFixture]
    public class UtilMathShould
    {
        [TestCase(1, 0, 0, 0, 1, 0)]
        [TestCase(0, 0, 1, 0, -1, 0)]
        public void TestCalculateRotationBetweenVectorsYieldsValidRotation(float ax, float ay, float az, float bx,
            float by, float bz)
        {
            var rr = 1.0f / (float)System.Math.Sqrt(2);
            
            var a = new Vector3(ax, ay, az);
            var b = new Vector3(bx, by, bz);

            var r = Veldrid.SceneGraph.Util.Math.CalculateRotationBetweenVectors(a, b);
            
            Assert.That(r, Is.Not.EqualTo(Matrix4x4.Identity));

            var aa = new Vector4(a, 0.0f);
            var bb = new Vector4(b, 0.0f);

            var cc = Vector4.Transform(aa, r);

            var tol = 1e-7;
            Assert.That(cc.X, Is.EqualTo(bb.X).Within(tol));
            Assert.That(cc.Y, Is.EqualTo(bb.Y).Within(tol));
            Assert.That(cc.Z, Is.EqualTo(bb.Z).Within(tol));
        }

        [Test]
        public void TestCalculateTangents()
        {
            var trajectory = new Vector3[5]
            {
                Vector3.UnitY,
                Vector3.Zero,
                Vector3.UnitZ,
                Vector3.UnitX + Vector3.UnitZ,
                Vector3.UnitX - Vector3.UnitY + Vector3.UnitZ
            };

            var tangents = Util.Math.ComputePathTangents(trajectory);
            
            Assert.That(tangents[0], Is.EqualTo(-Vector3.UnitY));
            Assert.That(tangents[1], Is.EqualTo(Vector3.UnitZ-Vector3.UnitY));
            Assert.That(tangents[2], Is.EqualTo(Vector3.UnitX+Vector3.UnitZ));
            Assert.That(tangents[3], Is.EqualTo(Vector3.UnitX-Vector3.UnitY));
            Assert.That(tangents[4], Is.EqualTo(-Vector3.UnitY));
        }

        [Test]
        public void ExtrudeCylinderDoesNotThrowWithNearlyLinearPath()
        {
            var nSegments = 4;
            var shape = new Vector2[nSegments];
            for (var i = 0; i < nSegments; ++i)
            {
                var theta = i * 2 * System.Math.PI / nSegments;

                shape[i] = new Vector2((float)(System.Math.Sin(theta)), (float)(System.Math.Cos(theta)));
            }

            var path = new Vector3[6]
            {
                new Vector3(0.0f, 0.0f, -10.0f), 
                new Vector3(0.0f, 0.0f, -9.0f),
                new Vector3(0.0f, 0.0001f, -8.0f),
                new Vector3(0.0f, 0.0001f, -7.0f),
                new Vector3(0.0f, 0.0f, -6.0f),
                new Vector3(0.0f, 0.0f, -5.0f),
            };

            Assert.DoesNotThrow(() =>
            {
                var extrusion = Util.Math.ExtrudeShape(shape, path);
            });
            
            
        }
        
        [Test]
        public void ExtrudeCylinderCorrectly()
        {
            var nSegments = 4;
            var shape = new Vector2[nSegments];
            for (var i = 0; i < nSegments; ++i)
            {
                var theta = i * 2 * System.Math.PI / nSegments;

                shape[i] = new Vector2((float)(System.Math.Sin(theta)), (float)(System.Math.Cos(theta)));
            }

            var path = new Vector3[3]
            {
                Vector3.UnitX,
                Vector3.One,
                Vector3.UnitY
            };

            var extrusion = Util.Math.ExtrudeShape(shape, path);
            
            Assert.That(extrusion.GetLength(0), Is.EqualTo(path.Length));
            Assert.That(extrusion.GetLength(1), Is.EqualTo(nSegments));

            var expected = new Vector3[3,4];
            expected[0,0] = new Vector3(1f, 0.7071f, -0.7071f);
            expected[0,1] = new Vector3(2f, 0f, 0f);
            expected[0,2] = new Vector3(1f, -0.7071f, 0.7071f);
            expected[0,3] = new Vector3(0f, 0f, 0f);
            
            expected[1,0] = new Vector3(1.2357f, 1.2357f, 0.05719f);
            expected[1,1] = new Vector3(1.6667f, 1.6667f, 1.3333f);
            expected[1,2] = new Vector3(0.7643f, 0.7643f, 1.9428f);
            expected[1,3] = new Vector3(0.3333f, 0.3333f, 0.6666f);
            
            expected[2,0] = new Vector3(0.7071f, 1f, -0.7071f);
            expected[2,1] = new Vector3(0f, 2f, 0f);
            expected[2,2] = new Vector3(-0.7071f, 1f, 0.7071f);
            expected[2,3] = new Vector3(0f, 0f, 0f);

            for (var i = 0; i < path.Length; ++i)
            {
                for(var j=0;j<nSegments; ++j)
                {
                    Assert.That(extrusion[i,j].X, Is.EqualTo(expected[i, j].X).Within(1e-4));
                    Assert.That(extrusion[i,j].Y, Is.EqualTo(expected[i, j].Y).Within(1e-4));
                    Assert.That(extrusion[i,j].Z, Is.EqualTo(expected[i, j].Z).Within(1e-4));
                }
            }
        }
    }
}