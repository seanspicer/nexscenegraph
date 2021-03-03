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

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Tests.Util
{
    [TestFixture]
    public class LineSegmentIntersectorShould
    {
        [TestCase]
        public void PassCanaryTest()
        {
            Assert.That(2 + 2, Is.EqualTo(4));
        }

        [TestCase]
        public void IntersectGeodeCorrectly()
        {
            var vtxArray = new List<Position3Color3>
            {
                new Position3Color3(new Vector3(-1.0f, -1.0f, 0.0f), Vector3.Zero),
                new Position3Color3(new Vector3(1.0f, -1.0f, 0.0f), Vector3.Zero),
                new Position3Color3(new Vector3(1.0f, 1.0f, 0.0f), Vector3.Zero),
                new Position3Color3(new Vector3(-1.0f, 1.0f, 0.0f), Vector3.Zero)
            };

            var idxArray = new List<uint> {0, 1, 2, 2, 3, 0};

            var geom = Geometry<Position3Color3>.Create();
            geom.VertexData = vtxArray.ToArray();
            geom.IndexData = idxArray.ToArray();

            var pset = DrawElements<Position3Color3>.Create(
                geom,
                PrimitiveTopology.TriangleList,
                6,
                1,
                0,
                0,
                0);

            geom.PrimitiveSets.Add(pset);

            var geode = Geode.Create();
            geode.AddDrawable(geom);

            var lineSegmentIntersector =
                LineSegmentIntersector.Create(new Vector3(0.5f, 0.0f, -1.0f), new Vector3(0.5f, 0.0f, 1.0f));

            var intersectionVisitor
                = IntersectionVisitor.Create(lineSegmentIntersector);

            geode.Accept(intersectionVisitor);

            var intersections = lineSegmentIntersector.Intersections;

            Assert.That(intersections.Count(), Is.EqualTo(1));

            var intersection = intersections.First();
            Assert.That(intersection.LocalIntersectionPoint, Is.EqualTo(new Vector3(0.5f, 0.0f, 0.0f)));
            Assert.That(intersection.WorldIntersectionPoint, Is.EqualTo(new Vector3(0.5f, 0.0f, 0.0f)));
        }

        [TestCase]
        public void IntersectDrawableCorrectly()
        {
            var vtxArray = new List<Position3Color3>
            {
                new Position3Color3(new Vector3(-1.0f, -1.0f, 0.0f), Vector3.Zero),
                new Position3Color3(new Vector3(1.0f, -1.0f, 0.0f), Vector3.Zero),
                new Position3Color3(new Vector3(1.0f, 1.0f, 0.0f), Vector3.Zero),
                new Position3Color3(new Vector3(-1.0f, 1.0f, 0.0f), Vector3.Zero)
            };

            var idxArray = new List<uint> {0, 1, 2, 2, 3, 0};

            var geom = Geometry<Position3Color3>.Create();
            geom.VertexData = vtxArray.ToArray();
            geom.IndexData = idxArray.ToArray();

            var pset = DrawElements<Position3Color3>.Create(
                geom,
                PrimitiveTopology.TriangleList,
                6,
                1,
                0,
                0,
                0);

            geom.PrimitiveSets.Add(pset);

            var lineSegmentIntersector =
                LineSegmentIntersector.Create(new Vector3(0.5f, 0.0f, -1.0f), new Vector3(0.5f, 0.0f, 1.0f));

            var intersectionVisitor
                = IntersectionVisitor.Create(lineSegmentIntersector);

            geom.Accept(intersectionVisitor);

            var intersections = lineSegmentIntersector.Intersections;

            Assert.That(intersections.Count(), Is.EqualTo(1));

            var intersection = intersections.First();
            Assert.That(intersection.LocalIntersectionPoint, Is.EqualTo(new Vector3(0.5f, 0.0f, 0.0f)));
            Assert.That(intersection.WorldIntersectionPoint, Is.EqualTo(new Vector3(0.5f, 0.0f, 0.0f)));
        }
    }
}