//
// Copyright 2018 Sean Spicer 
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using Veldrid.SceneGraph;

namespace Veldrid.SceneGraph.Tests
{
    [TestFixture]
    public class PlaneTests
    {
        [TestCaseSource(typeof(PlaneTests), "GetData")]
        public void TestIntersectBoundingBoxPlane(Plane planeUnderTest, BoundingBox bbUnderTest, int expected)
        {
            // Intersect should return 1
            Assert.That( expected, Is.EqualTo(planeUnderTest.Intersect(bbUnderTest)));
        }
        
        public static IEnumerable<object[]> GetData()
        {
            var quadrant1 = BoundingBox.Create( 1, 1, 1, 2, 2, 2);
            var quadrant2 = BoundingBox.Create(-1, 1, 1,-2, 2, 2);
            var quadrant3 = BoundingBox.Create(-1,-1, 1,-2,-2, 2);
            var quadrant4 = BoundingBox.Create( 1,-1, 1, 2,-2, 2);
            
            var quadrant5 = BoundingBox.Create( 1, 1,-1, 2, 2,-2);
            var quadrant6 = BoundingBox.Create(-1, 1,-1,-2, 2,-2);
            var quadrant7 = BoundingBox.Create(-1,-1,-1,-2,-2,-2);
            var quadrant8 = BoundingBox.Create( 1,-1,-1, 2,-2,-2);
            
            var unit = BoundingBox.Create(-1,-1,-1, 1, 1, 1);
            
            var allData = new List<object[]>
            {
                // XY (Z=0) Plane Tests
                new object[] {Plane.Create(0, 0, 1, 0), quadrant1, 1},
                new object[] {Plane.Create(0, 0, 1, 0), quadrant2, 1},
                new object[] {Plane.Create(0, 0, 1, 0), quadrant3, 1},
                new object[] {Plane.Create(0, 0, 1, 0), quadrant4, 1},
                
                new object[] {Plane.Create(0, 0, 1, 0), quadrant5,-1},
                new object[] {Plane.Create(0, 0, 1, 0), quadrant6,-1},
                new object[] {Plane.Create(0, 0, 1, 0), quadrant7,-1},
                new object[] {Plane.Create(0, 0, 1, 0), quadrant8,-1},
                
                // -XY (Z=0) Plane Tests
                new object[] {Plane.Create(0, 0,-1, 0), quadrant1,-1},
                new object[] {Plane.Create(0, 0,-1, 0), quadrant2,-1},
                new object[] {Plane.Create(0, 0,-1, 0), quadrant3,-1},
                new object[] {Plane.Create(0, 0,-1, 0), quadrant4,-1},
                
                new object[] {Plane.Create(0, 0,-1, 0), quadrant5, 1},
                new object[] {Plane.Create(0, 0,-1, 0), quadrant6, 1},
                new object[] {Plane.Create(0, 0,-1, 0), quadrant7, 1},
                new object[] {Plane.Create(0, 0,-1, 0), quadrant8, 1},
                
                // XZ (Y=0) Plane Tests
                new object[] {Plane.Create(0, 1, 0, 0), quadrant1, 1},
                new object[] {Plane.Create(0, 1, 0, 0), quadrant2, 1},
                new object[] {Plane.Create(0, 1, 0, 0), quadrant5, 1},
                new object[] {Plane.Create(0, 1, 0, 0), quadrant6, 1},
                
                new object[] {Plane.Create(0, 1, 0, 0), quadrant3,-1},
                new object[] {Plane.Create(0, 1, 0, 0), quadrant4,-1}, 
                new object[] {Plane.Create(0, 1, 0, 0), quadrant7,-1},
                new object[] {Plane.Create(0, 1, 0, 0), quadrant8,-1},
                
                // -XZ (Y=0) Plane Tests
                new object[] {Plane.Create(0,-1, 0, 0), quadrant1,-1},
                new object[] {Plane.Create(0,-1, 0, 0), quadrant2,-1},
                new object[] {Plane.Create(0,-1, 0, 0), quadrant5,-1},
                new object[] {Plane.Create(0,-1, 0, 0), quadrant6,-1},
                
                new object[] {Plane.Create(0,-1, 0, 0), quadrant3, 1},
                new object[] {Plane.Create(0,-1, 0, 0), quadrant4, 1},
                new object[] {Plane.Create(0,-1, 0, 0), quadrant7, 1},
                new object[] {Plane.Create(0,-1, 0, 0), quadrant8, 1},
                
                // YZ (X=0) Plane Tests
                new object[] {Plane.Create(1, 0, 0, 0), quadrant1, 1},
                new object[] {Plane.Create(1, 0, 0, 0), quadrant4, 1},
                new object[] {Plane.Create(1, 0, 0, 0), quadrant5, 1},
                new object[] {Plane.Create(1, 0, 0, 0), quadrant8, 1},
                
                new object[] {Plane.Create(1, 0, 0, 0), quadrant2,-1},
                new object[] {Plane.Create(1, 0, 0, 0), quadrant3,-1}, 
                new object[] {Plane.Create(1, 0, 0, 0), quadrant6,-1},
                new object[] {Plane.Create(1, 0, 0, 0), quadrant7,-1},
                
                // -YZ (X=0) Plane Tests
                new object[] {Plane.Create(-1, 0, 0, 0), quadrant1,-1},
                new object[] {Plane.Create(-1, 0, 0, 0), quadrant4,-1},
                new object[] {Plane.Create(-1, 0, 0, 0), quadrant5,-1},
                new object[] {Plane.Create(-1, 0, 0, 0), quadrant8,-1},
                
                new object[] {Plane.Create(-1, 0, 0, 0), quadrant2, 1},
                new object[] {Plane.Create(-1, 0, 0, 0), quadrant3, 1}, 
                new object[] {Plane.Create(-1, 0, 0, 0), quadrant6, 1},
                new object[] {Plane.Create(-1, 0, 0, 0), quadrant7, 1},
                
                // Unit Bounding Box Crossing Tests
                new object[] {Plane.Create(0, 0, 1, 0), unit, 0},
                new object[] {Plane.Create(0, 1, 0, 0), unit, 0},
                new object[] {Plane.Create(1, 0, 0, 0), unit, 0},
            };

            return allData;
        }
    }
}