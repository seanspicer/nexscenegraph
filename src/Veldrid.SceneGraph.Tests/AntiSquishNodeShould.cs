

using System.Numerics;
using NUnit.Framework;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Tests
{
    internal class MyAntiSquish : AntiSquish
    {
        private Matrix4x4 _myLocalToWorld;
        
        internal MyAntiSquish(Matrix4x4 localToWorld)
        {
            _myLocalToWorld = localToWorld;
        }
        
        internal bool UnsquishTest(ref Matrix4x4 matrixToUnsquish)
        {
            return ComputeUnsquishedMatrix(ref matrixToUnsquish);
        }

        protected override Matrix4x4 GetLocalToWorld(NodePath np)
        {
            return _myLocalToWorld;
        }
    }
    
    [TestFixture]
    public class AntiSquishNodeShould
    {
        
        [TestCase]
        public void ComputeAntiSquishWithoutAffectingTranslation()
        {
            var localToWorld = Matrix4x4.CreateTranslation(10, 0, 0);
            
            var antiSquish = new MyAntiSquish(localToWorld);

            var unsquished = Matrix4x4.Identity;
            
            var result = antiSquish.UnsquishTest(ref unsquished);

            Assert.That(result, Is.EqualTo(true));
            Assert.That(unsquished.M11, Is.EqualTo(localToWorld.M11));
            Assert.That(unsquished.M12, Is.EqualTo(localToWorld.M12));
            Assert.That(unsquished.M13, Is.EqualTo(localToWorld.M13));
            Assert.That(unsquished.M14, Is.EqualTo(localToWorld.M14));
        
            Assert.That(unsquished.M21, Is.EqualTo(localToWorld.M21));
            Assert.That(unsquished.M22, Is.EqualTo(localToWorld.M22));
            Assert.That(unsquished.M23, Is.EqualTo(localToWorld.M23));
            Assert.That(unsquished.M24, Is.EqualTo(localToWorld.M24));
            
            Assert.That(unsquished.M31, Is.EqualTo(localToWorld.M31));
            Assert.That(unsquished.M32, Is.EqualTo(localToWorld.M32));
            Assert.That(unsquished.M33, Is.EqualTo(localToWorld.M33));
            Assert.That(unsquished.M34, Is.EqualTo(localToWorld.M34));
            
            Assert.That(unsquished.M41, Is.EqualTo(localToWorld.M41));
            Assert.That(unsquished.M42, Is.EqualTo(localToWorld.M42));
            Assert.That(unsquished.M43, Is.EqualTo(localToWorld.M43));
            Assert.That(unsquished.M44, Is.EqualTo(localToWorld.M44));
        }
        
        [TestCase]
        public void ComputeAntiSquishWithoutAffectingRotation()
        {
            var tol = 1e-6;
            
            var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)System.Math.PI/2);
            var localToWorld = Matrix4x4.CreateFromQuaternion(rotation);
            
            var antiSquish = new MyAntiSquish(localToWorld);

            var unsquished = Matrix4x4.Identity;
            
            var result = antiSquish.UnsquishTest(ref unsquished);

            Assert.That(result, Is.EqualTo(true));
            Assert.That(unsquished.M11, Is.EqualTo(localToWorld.M11).Within(tol));
            Assert.That(unsquished.M12, Is.EqualTo(localToWorld.M12).Within(tol));
            Assert.That(unsquished.M13, Is.EqualTo(localToWorld.M13).Within(tol));
            Assert.That(unsquished.M14, Is.EqualTo(localToWorld.M14).Within(tol));
        
            Assert.That(unsquished.M21, Is.EqualTo(localToWorld.M21).Within(tol));
            Assert.That(unsquished.M22, Is.EqualTo(localToWorld.M22).Within(tol));
            Assert.That(unsquished.M23, Is.EqualTo(localToWorld.M23).Within(tol));
            Assert.That(unsquished.M24, Is.EqualTo(localToWorld.M24).Within(tol));
            
            Assert.That(unsquished.M31, Is.EqualTo(localToWorld.M31).Within(tol));
            Assert.That(unsquished.M32, Is.EqualTo(localToWorld.M32).Within(tol));
            Assert.That(unsquished.M33, Is.EqualTo(localToWorld.M33).Within(tol));
            Assert.That(unsquished.M34, Is.EqualTo(localToWorld.M34).Within(tol));
            
            Assert.That(unsquished.M41, Is.EqualTo(localToWorld.M41).Within(tol));
            Assert.That(unsquished.M42, Is.EqualTo(localToWorld.M42).Within(tol));
            Assert.That(unsquished.M43, Is.EqualTo(localToWorld.M43).Within(tol));
            Assert.That(unsquished.M44, Is.EqualTo(localToWorld.M44).Within(tol));
        }
        
        [TestCase]
        public void ComputeAntiSquishWithoutAffectingTranslationOrRotation()
        {
            var tol = 1e-5;
            
            var scale = Matrix4x4.CreateScale(10, 20, 30);
            var rotation = Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)System.Math.PI/2));
            var translation = Matrix4x4.CreateTranslation(10, 0, 0);

            var localToWorld = Matrix4x4.Identity.PostMultiply(rotation).PostMultiply(translation).PostMultiply(scale);
            
            var antiSquish = new MyAntiSquish(localToWorld);

            var unsquished = Matrix4x4.Identity;
            
            var result = antiSquish.UnsquishTest(ref unsquished);

            var avg = (scale.M11 + scale.M22 + scale.M33) / 3.0f;
            
            Assert.That(result, Is.EqualTo(true));
            Assert.That(unsquished.M11, Is.EqualTo(avg).Within(tol));
            Assert.That(unsquished.M12, Is.EqualTo(localToWorld.M12).Within(tol));
            Assert.That(unsquished.M13, Is.EqualTo(localToWorld.M13).Within(tol));
            Assert.That(unsquished.M14, Is.EqualTo(localToWorld.M14).Within(tol));
        
            Assert.That(unsquished.M21, Is.EqualTo(localToWorld.M21).Within(tol));
            Assert.That(unsquished.M22, Is.EqualTo(localToWorld.M22).Within(tol));
            Assert.That(unsquished.M23, Is.EqualTo(avg).Within(tol));
            Assert.That(unsquished.M24, Is.EqualTo(localToWorld.M24).Within(tol));
            
            Assert.That(unsquished.M31, Is.EqualTo(localToWorld.M31).Within(tol));
            Assert.That(unsquished.M32, Is.EqualTo(-avg).Within(tol));
            Assert.That(unsquished.M33, Is.EqualTo(localToWorld.M33).Within(tol));
            Assert.That(unsquished.M34, Is.EqualTo(localToWorld.M34).Within(tol));
            
            Assert.That(unsquished.M41, Is.EqualTo(localToWorld.M41).Within(tol));
            Assert.That(unsquished.M42, Is.EqualTo(localToWorld.M42).Within(tol));
            Assert.That(unsquished.M43, Is.EqualTo(localToWorld.M43).Within(tol));
            Assert.That(unsquished.M44, Is.EqualTo(localToWorld.M44).Within(tol));
        }
    }
}