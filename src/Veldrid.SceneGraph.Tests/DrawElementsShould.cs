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
using Moq;
using NUnit.Framework;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Tests
{
    [TestFixture]
    public class DrawElementsShould
    {
        [Test]
        public void ComputeBoundingBoxCorrectlyForDiagonalLine()
        {
            var vertices = new Position3Color3[3];
            vertices[0] = new Position3Color3(new Vector3(-1, -1, -1), Vector3.UnitX);
            vertices[1] = new Position3Color3(new Vector3(-1, -1, 1), Vector3.UnitX);
            
            // Use an extra vertex to be sure we don't introduce an "add" bug
            vertices[2] = new Position3Color3(new Vector3(1, 1, 1), Vector3.UnitX);
            
            var geometryMock = new Mock<IGeometry<Position3Color3>>();
            geometryMock.Setup(x => x.VertexData).Returns(vertices);
            geometryMock.Setup(x => x.IndexData).Returns(new[]{0u,1u,2u});

            var drawElts = DrawElements<Position3Color3>.Create(
                geometryMock.Object,
                PrimitiveTopology.LineStrip,
                3, 
                1, 
                0, 
                0, 
                0);

            var bbox = drawElts.GetBoundingBox();
            
            Assert.That(bbox.Center, Is.EqualTo(Vector3.Zero));
            Assert.That(bbox.Radius, Is.EqualTo(System.Math.Sqrt(3)).Within(1e-6));
            Assert.That(bbox.Min, Is.EqualTo(-Vector3.One));
            Assert.That(bbox.Max, Is.EqualTo(Vector3.One));
        }
        
        [Test]
        public void ComputeBoundingBoxCorrectlyForLineInPlane()
        {
            var vertices = new Position3Color3[2];
            vertices[0] = new Position3Color3(new Vector3(-1, -1, 0), Vector3.UnitX);
            vertices[1] = new Position3Color3(new Vector3(1, 1, 0), Vector3.UnitX);

            var geometryMock = new Mock<IGeometry<Position3Color3>>();
            geometryMock.Setup(x => x.VertexData).Returns(vertices);
            geometryMock.Setup(x => x.IndexData).Returns(new[]{0u,1u});

            var drawElts = DrawElements<Position3Color3>.Create(
                geometryMock.Object,
                PrimitiveTopology.LineStrip,
                2, 
                1, 
                0, 
                0, 
                0);

            var bbox = drawElts.GetBoundingBox();
            
            Assert.That(bbox.Center, Is.EqualTo(Vector3.Zero));
            Assert.That(bbox.Radius, Is.EqualTo(System.Math.Sqrt(2)).Within(1e-6));
            Assert.That(bbox.Min, Is.EqualTo(new Vector3(-1, -1, 0)));
            Assert.That(bbox.Max, Is.EqualTo(new Vector3(1, 1, 0)));
        }
        
        [Test]
        public void ComputeBoundingBoxCorrectlyForUniAxialLine()
        {
            var vertices = new Position3Color3[3];
            vertices[0] = new Position3Color3(new Vector3(-1, 0, 0), Vector3.UnitX);
            vertices[1] = new Position3Color3(new Vector3(1, 0, 0), Vector3.UnitX);
            
            var geometryMock = new Mock<IGeometry<Position3Color3>>();
            geometryMock.Setup(x => x.VertexData).Returns(vertices);
            geometryMock.Setup(x => x.IndexData).Returns(new[]{0u,1u});

            var drawElts = DrawElements<Position3Color3>.Create(
                geometryMock.Object,
                PrimitiveTopology.LineStrip,
                2, 
                1, 
                0, 
                0, 
                0);

            var bbox = drawElts.GetBoundingBox();

            Assert.That(bbox.Center, Is.EqualTo(Vector3.Zero));
            Assert.That(bbox.Radius, Is.EqualTo(1).Within(1e-6));
            Assert.That(bbox.Min, Is.EqualTo(new Vector3(-1, 0, 0)));
            Assert.That(bbox.Max, Is.EqualTo(new Vector3(1, 0, 0)));
        }
    }
}