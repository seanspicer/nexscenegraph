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
using System.Numerics;
using Veldrid.SceneGraph;
using Xunit;

namespace Veldrid.SceneGraph.Tests
{
    
    public class BoundingBoxTests
    {
        [Fact]
        public void TestExpandByVector()
        {
            // Construct a bounding box above the plane
            var bb = BoundingBox.Create();
            bb.ExpandBy(new Vector3(-1, -1, 1));
            bb.ExpandBy(new Vector3( 1,  1, 2));
            
            Assert.Equal(-1, bb.XMin);
            Assert.Equal( 1, bb.XMax);
            Assert.Equal(-1, bb.YMin);
            Assert.Equal( 1, bb.YMax);
            Assert.Equal( 1, bb.ZMin);
            Assert.Equal( 2, bb.ZMax);
        }
    }
}