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

namespace Veldrid.SceneGraph.Tests
{
    [TestFixture]
    public class ComputeBoundsVisitorShould
    {
        [Test]
        public void SetBoundingBoxForDrawableCorrectly()
        {
            var drawableMock = new Mock<IDrawable>();
            drawableMock.Setup(x => x.GetBoundingBox()).Returns(BoundingBox.Create(-4*Vector3.One, 4*Vector3.One));

            var sut = ComputeBoundsVisitor.Create();
            sut.Apply(drawableMock.Object);
            
            Assert.That(sut.GetBoundingBox().Center, Is.EqualTo(Vector3.Zero));
            Assert.That(sut.GetBoundingBox().Min, Is.EqualTo(-4*Vector3.One));
            Assert.That(sut.GetBoundingBox().Max, Is.EqualTo(4*Vector3.One));
        }
    }
}