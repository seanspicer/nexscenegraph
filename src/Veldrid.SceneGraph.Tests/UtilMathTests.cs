using System.ComponentModel.DataAnnotations;
using System.Numerics;
using NUnit.Framework;

namespace Veldrid.SceneGraph.Tests
{
    [TestFixture]
    public class UtilMathTests
    {
        [TestCase]
        public void TestCalculateRotationBetweenVectorsYieldsValidRotation()
        {
            var rr = 1.0f / (float)System.Math.Sqrt(2);
            
            var a = Vector3.UnitX;
            var b = new Vector3(rr, rr, 0);

            var r = Veldrid.SceneGraph.Util.Math.CalculateRotationBetweenVectors(a, b);
            
            Assert.That(r, Is.Not.EqualTo(Matrix4x4.Identity));

            var aa = new Vector4(a, 0.0f);
            var bb = new Vector4(b, 0.0f);

            var cc = Vector4.Transform(bb, r);
            
            Assert.That(cc, Is.EqualTo(aa));
        }
    }
}