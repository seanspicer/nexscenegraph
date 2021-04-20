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

namespace Veldrid.SceneGraph.Manipulators.Tests
{
    [TestFixture]
    public class LineProjectorShould
    {
        [TestCase]
        public void PassCanaryTest()
        {
            Assert.That(2 + 2, Is.EqualTo(4));
        }

        [TestCase]
        public void ComputeClosestPointsCorrectly()
        {
            var l1 = LineSegment.Create(Vector3.Zero, Vector3.UnitX);
            var l2 = LineSegment.Create(new Vector3(1f, 1f, 0f), new Vector3(1f, 1f, 1f));

            var canCompute = LineProjector.ComputeClosestPoints(l1, l2, out var p1, out var p2);

            Assert.That(canCompute, Is.True);
            Assert.That(p1, Is.EqualTo(Vector3.UnitX));
            Assert.That(p2, Is.EqualTo(new Vector3(1f, 1f, 0f)));
        }
    }
}