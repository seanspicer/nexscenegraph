using System.Collections.Generic;
using System.Numerics;
using Moq;
using NUnit.Framework;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Tests
{
    public static class TestGeometryHelpers
    {
        public static IDrawable Get3dGeometry()
        {
            var vertices = new Position3Color3[3];
            vertices[0] = new Position3Color3(new Vector3(-1, -1, -1), Vector3.UnitX);
            vertices[1] = new Position3Color3(new Vector3(-1, -1, 1), Vector3.UnitX);
            
            // Use an extra vertex to be sure we don't introduce an "add" bug
            vertices[2] = new Position3Color3(new Vector3(1, 1, 1), Vector3.UnitX);

            var geometry = Geometry<Position3Color3>.Create();
            geometry.IndexData = new[] {0u, 1u, 2u};
            geometry.VertexData = vertices;
            
            var drawElts = DrawElements<Position3Color3>.Create(
                geometry,
                PrimitiveTopology.LineStrip,
                3, 
                1, 
                0, 
                0, 
                0);

            geometry.PrimitiveSets.Add(drawElts);

            return geometry;
        }
        
        public static IDrawable GetPlaneGeometry()
        {
            var vertices = new Position3Color3[3];
            vertices[0] = new Position3Color3(new Vector3(-1, -1, 0), Vector3.UnitX);
            vertices[1] = new Position3Color3(new Vector3(1, 1, 0), Vector3.UnitX);
            
            var geometry = Geometry<Position3Color3>.Create();
            geometry.IndexData = new[] {0u, 1u};
            geometry.VertexData = vertices;
            
            var drawElts = DrawElements<Position3Color3>.Create(
                geometry,
                PrimitiveTopology.LineStrip,
                2, 
                1, 
                0, 
                0, 
                0);

            geometry.PrimitiveSets.Add(drawElts);

            return geometry;
        }
        
        public static IDrawable GetUniaxialGeometry()
        {
            var vertices = new Position3Color3[3];
            vertices[0] = new Position3Color3(new Vector3(-1, 0, 0), Vector3.UnitX);
            vertices[1] = new Position3Color3(new Vector3(1, 0, 0), Vector3.UnitX);
            
            var geometry = Geometry<Position3Color3>.Create();
            geometry.IndexData = new[] {0u, 1u};
            geometry.VertexData = vertices;
            
            var drawElts = DrawElements<Position3Color3>.Create(
                geometry,
                PrimitiveTopology.LineStrip,
                2, 
                1, 
                0, 
                0, 
                0);

            geometry.PrimitiveSets.Add(drawElts);

            return geometry;
        }
    }
    
    [TestFixture]
    public class GeometryShould
    {
        [TestCase]
        public void ComputeBoundingBoxCorrectlyFor3dGeometry()
        {
            var geometry = TestGeometryHelpers.Get3dGeometry();

            var bbox = geometry.GetBoundingBox();
            
            Assert.That(bbox.Center, Is.EqualTo(Vector3.Zero));
            Assert.That(bbox.Radius, Is.EqualTo(System.Math.Sqrt(3)).Within(1e-6));
            Assert.That(bbox.Min, Is.EqualTo(-Vector3.One));
            Assert.That(bbox.Max, Is.EqualTo(Vector3.One));
        }
        
        [TestCase]
        public void ComputeBoundingBoxCorrectlyForPlaneGeometry()
        {
            var geometry = TestGeometryHelpers.GetPlaneGeometry();

            var bbox = geometry.GetBoundingBox();
            
            Assert.That(bbox.Center, Is.EqualTo(Vector3.Zero));
            Assert.That(bbox.Radius, Is.EqualTo(System.Math.Sqrt(2)).Within(1e-6));
            Assert.That(bbox.Min, Is.EqualTo(new Vector3(-1, -1, 0)));
            Assert.That(bbox.Max, Is.EqualTo(new Vector3(1, 1, 0)));
        }
        
        [TestCase]
        public void ComputeBoundingBoxCorrectlyForUniaxialGeometry()
        {
            var geometry = TestGeometryHelpers.GetUniaxialGeometry();

            var bbox = geometry.GetBoundingBox();
            
            Assert.That(bbox.Center, Is.EqualTo(Vector3.Zero));
            Assert.That(bbox.Radius, Is.EqualTo(1).Within(1e-6));
            Assert.That(bbox.Min, Is.EqualTo(new Vector3(-1, 0, 0)));
            Assert.That(bbox.Max, Is.EqualTo(new Vector3(1, 0, 0)));
        }
    }
}