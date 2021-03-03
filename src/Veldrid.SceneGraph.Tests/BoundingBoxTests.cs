//
// Copyright 2018-2021 Sean Spicer 
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
    public class BoundingBoxTests
    {
        [TestCase]
        public void TestExpandByVector()
        {
            // Construct a bounding box above the plane
            var bb = BoundingBox.Create();
            bb.ExpandBy(new Vector3(-1, -1, 1));
            bb.ExpandBy(new Vector3(1, 1, 2));

            Assert.That(-1, Is.EqualTo(bb.XMin));
            Assert.That(1, Is.EqualTo(bb.XMax));
            Assert.That(-1, Is.EqualTo(bb.YMin));
            Assert.That(1, Is.EqualTo(bb.YMax));
            Assert.That(1, Is.EqualTo(bb.ZMin));
            Assert.That(2, Is.EqualTo(bb.ZMax));
        }
    }
}