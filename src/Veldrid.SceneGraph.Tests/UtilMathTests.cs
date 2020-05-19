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
            
            Assert.That(tangents[0], Is.EqualTo(new Vector3( 0.0f, -1.5f, -0.5f)));
            Assert.That(tangents[1], Is.EqualTo(new Vector3( 0.0f, -0.5f,  0.5f)));
            Assert.That(tangents[2], Is.EqualTo(new Vector3( 0.5f,  0.0f,  0.5f)));
            Assert.That(tangents[3], Is.EqualTo(new Vector3( 0.5f, -0.5f, -0.0f)));
            Assert.That(tangents[4], Is.EqualTo(new Vector3(-0.5f, -1.5f,  0.0f)));
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
                new Vector3(91.07588f, 100f, 56.6469078f), 
                new Vector3(91.07928f, 98.61714f, 56.6469078f),
                new Vector3(91.08154f, 97.82282f, 56.6469078f),
                new Vector3(91.08382f, 97.0063248f, 56.6469078f),
                new Vector3(91.08608f, 96.2078247f, 56.6469078f),
                new Vector3(91.08835f, 95.3893661f, 56.6469078f),
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
            expected[0,0] = new Vector3(0.935339332f, 0.806018054f, -0.588348448f);
            expected[0,1] = new Vector3(1.97844648f, -0.064660646f, -0.196116164f);
            expected[0,2] = new Vector3(1.06466067f, -0.806018054f, 0.588348448f);
            expected[0,3] = new Vector3(0.021553576f, 0.064660646f, 0.196116164f);
            
            expected[1,0] = new Vector3(1.18162894f, 1.1816287f, 0.0335519314f);
            expected[1,1] = new Vector3(1.68338192f, 1.68338203f, 1.25686204f);
            expected[1,2] = new Vector3(0.818371058f, 0.818371236f, 1.96644807f);
            expected[1,3] = new Vector3(0.316618085f, 0.316618025f, 0.743137956f);
            
            expected[2,0] = new Vector3(0.806018174f, 0.935339391f, -0.58834815f);
            expected[2,1] = new Vector3(-0.0646606982f, 1.97844625f, -0.196116164f);
            expected[2,2] = new Vector3(-0.806018174f, 1.06466067f, 0.58834815f);
            expected[2,3] = new Vector3(0.0646606982f, 0.0215536952f, 0.196116164f);

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