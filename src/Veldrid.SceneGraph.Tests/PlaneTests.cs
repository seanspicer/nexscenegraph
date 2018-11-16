//
// Copyright (c) 2018 Sean Spicer
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.SceneGraph;
using Xunit;

namespace Veldrid.SceneGraph.Tests
{
    public class PlaneTests
    {
        [Theory]
        [MemberData(nameof(GetData))]
        public void TestIntersectBoundingBoxPlane(Plane planeUnderTest, BoundingBox bbUnderTest, int expected)
        {
            // Intersect should return 1
            Assert.Equal( expected, planeUnderTest.Intersect(bbUnderTest));
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