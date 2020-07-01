using System.Numerics;
using NUnit.Framework;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Tests
{
    [TestFixture]
    public class CylinderShould
    {
        [TestCase]
        public void BeExpectedSize()
        {
            var cylinder = CreateCylinder();
            var boundingSphere = cylinder.GetBound();
            
            Assert.That(boundingSphere.Center, Is.EqualTo(Vector3.Zero));
            var expectedRadius = System.Math.Sqrt(0.5*0.5 + 0.5*0.5 + 0.5*0.5);
            Assert.That(boundingSphere.Radius, Is.EqualTo(expectedRadius).Within(1e-6));
        }
        
        private IGeode CreateCylinder()
        {
            var cylinderShape = Cylinder.Create(Vector3.Zero, 0.5f, 1.0f);
            var cylinderHints = TessellationHints.Create();
            cylinderHints.CreateBackFace = false;
            cylinderHints.SetDetailRatio(1.6f);
    
            var cylinderDrawable =
                ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    cylinderShape,
                    cylinderHints,
                    new Vector3[] {new Vector3(1.0f, 0.0f, 0.0f)});
    
            var cylinder = Geode.Create();
            cylinder.AddDrawable(cylinderDrawable);
            return cylinder;
        }
    }
}