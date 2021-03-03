using System.Numerics;
using NUnit.Framework;

namespace Veldrid.SceneGraph.Tests
{
    [TestFixture]
    public class GeodeShould
    {
        [TestCase]
        public void ComputeBoundingSphereCorrectlyFor3dGeometry()
        {
            var geom = TestGeometryHelpers.Get3dGeometry();
            var geode = Geode.Create();
            geode.AddDrawable(geom);

            var bSphere = geode.ComputeBound();

            Assert.That(bSphere.Center, Is.EqualTo(Vector3.Zero));
            Assert.That(bSphere.Radius, Is.EqualTo(System.Math.Sqrt(3)).Within(1e-6));
        }

        [TestCase]
        public void ComputeBoundingBoxCorrectlyFor3dGeometry()
        {
            var geom = TestGeometryHelpers.Get3dGeometry();
            var geode = Geode.Create();
            geode.AddDrawable(geom);

            var bbox = geode.GetBoundingBox();

            Assert.That(bbox.Center, Is.EqualTo(Vector3.Zero));
            Assert.That(bbox.Radius, Is.EqualTo(System.Math.Sqrt(3)).Within(1e-6));
            Assert.That(bbox.Min, Is.EqualTo(-Vector3.One));
            Assert.That(bbox.Max, Is.EqualTo(Vector3.One));
        }

        [TestCase]
        public void ComputeBoundingSphereCorrectlyForPlaneGeometry()
        {
            var geom = TestGeometryHelpers.GetPlaneGeometry();
            var geode = Geode.Create();
            geode.AddDrawable(geom);

            var bSphere = geode.ComputeBound();

            Assert.That(bSphere.Center, Is.EqualTo(Vector3.Zero));
            Assert.That(bSphere.Radius, Is.EqualTo(System.Math.Sqrt(2)).Within(1e-6));
        }

        [TestCase]
        public void ComputeBoundingBoxCorrectlyForPlaneGeometry()
        {
            var geom = TestGeometryHelpers.GetPlaneGeometry();
            var geode = Geode.Create();
            geode.AddDrawable(geom);

            var bbox = geode.GetBoundingBox();

            Assert.That(bbox.Center, Is.EqualTo(Vector3.Zero));
            Assert.That(bbox.Radius, Is.EqualTo(System.Math.Sqrt(2)).Within(1e-6));
            Assert.That(bbox.Min, Is.EqualTo(new Vector3(-1, -1, 0)));
            Assert.That(bbox.Max, Is.EqualTo(new Vector3(1, 1, 0)));
        }

        [TestCase]
        public void ComputeBoundingSphereCorrectlyForUniaxialGeometry()
        {
            var geom = TestGeometryHelpers.GetUniaxialGeometry();
            var geode = Geode.Create();
            geode.AddDrawable(geom);

            var bSphere = geode.ComputeBound();

            Assert.That(bSphere.Center, Is.EqualTo(Vector3.Zero));
            Assert.That(bSphere.Radius, Is.EqualTo(1).Within(1e-6));
        }

        [TestCase]
        public void ComputeBoundingBoxCorrectlyForUniaxialGeometry()
        {
            var geom = TestGeometryHelpers.GetUniaxialGeometry();
            var geode = Geode.Create();
            geode.AddDrawable(geom);

            var bbox = geode.GetBoundingBox();

            Assert.That(bbox.Center, Is.EqualTo(Vector3.Zero));
            Assert.That(bbox.Radius, Is.EqualTo(1).Within(1e-6));
            Assert.That(bbox.Min, Is.EqualTo(new Vector3(-1, 0, 0)));
            Assert.That(bbox.Max, Is.EqualTo(new Vector3(1, 0, 0)));
        }

        [TestCase]
        public void ComputeBoundingBoxCorrectlyForSplitDrawables()
        {
            var xy_pos = TestGeometryHelpers.GetXyPlaneGeometryPosZ();
            var xy_neg = TestGeometryHelpers.GetXyPlaneGeometryNegZ();

            var geode = Geode.Create();
            geode.AddDrawable(xy_pos);
            geode.AddDrawable(xy_neg);

            var bbox = geode.GetBoundingBox();

            Assert.That(bbox.Center, Is.EqualTo(Vector3.Zero));
            Assert.That(bbox.Radius, Is.EqualTo(System.Math.Sqrt(3)).Within(1e-6));
            Assert.That(bbox.Min, Is.EqualTo(-Vector3.One));
            Assert.That(bbox.Max, Is.EqualTo(Vector3.One));
        }

        [TestCase]
        public void ComputeBoundingSphereCorrectlyForSplitDrawables()
        {
            var xy_pos = TestGeometryHelpers.GetXyPlaneGeometryPosZ();
            var xy_neg = TestGeometryHelpers.GetXyPlaneGeometryNegZ();

            var geode = Geode.Create();
            geode.AddDrawable(xy_pos);
            geode.AddDrawable(xy_neg);

            var boundingSphere = geode.GetBound();

            Assert.That(boundingSphere.Center, Is.EqualTo(Vector3.Zero));
            Assert.That(boundingSphere.Radius, Is.EqualTo(System.Math.Sqrt(3)).Within(1e-6));
        }
    }
}