using System.Numerics;
using NUnit.Framework;

namespace Veldrid.SceneGraph.Tests
{
    [TestFixture]
    public class GroupShould
    {
        [TestCase]
        public void ComputeBoundingBoxCorrectlyForSplitDrawables()
        {
            var xy_pos = TestGeometryHelpers.GetXyPlaneGeometryPosZ();
            var xy_neg = TestGeometryHelpers.GetXyPlaneGeometryNegZ();

            var geode_pos = Geode.Create();
            geode_pos.AddDrawable(xy_pos);

            var geode_neg = Geode.Create();
            geode_neg.AddDrawable(xy_neg);

            var group = Group.Create();
            group.AddChild(geode_pos);
            group.AddChild(geode_neg);

            var boundingSphere = group.GetBound();

            Assert.That(boundingSphere.Center, Is.EqualTo(Vector3.Zero));
            Assert.That(boundingSphere.Radius, Is.EqualTo(2.41421366).Within(1e-6));
        }
    }
}