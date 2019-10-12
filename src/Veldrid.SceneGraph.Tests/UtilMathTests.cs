using System.ComponentModel.DataAnnotations;
using System.Numerics;
using NUnit.Framework;

namespace Veldrid.SceneGraph.Tests
{
    [TestFixture]
    public class UtilMathTests
    {
        [TestCase(1, 0, 0, 0, 1, 0)]
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
    }
}