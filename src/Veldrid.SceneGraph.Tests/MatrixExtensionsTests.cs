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

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.SceneGraph.Util;
using Xunit;

namespace Veldrid.SceneGraph.Tests
{
    public class MatrixExtensionsTests
    {
        [Fact]
        public void PassCanaryTest()
        {
            Assert.Equal(4, 2 + 2);
        }

        [Fact]
        public void PreMultiplyWithIdentityReturnsSame()
        {
            var v = new Vector3(1, 2, 3);
            
            Assert.Equal(v, Matrix4x4.Identity.PreMultiply(v));
        }
        
        [Fact]
        public void PostMultiplyWithIdentityReturnsSame()
        {
            var v = new Vector3(1, 2, 3);
            
            Assert.Equal(v, Matrix4x4.Identity.PostMultiply(v));
        }

        [Fact]
        public void PreMultiplyIsSameAsPostMultiplyInverse()
        {
            var m = Matrix4x4.CreateRotationX(0.56f);
            var v = new Vector3(1, 2, 3);

            Matrix4x4 inv;
            Matrix4x4.Invert(m, out inv);

            var v1 = m.PreMultiply(v);
            var v2 = inv.PostMultiply(v);
            
            Assert.Equal(v1, v2);
        }

        [Fact]
        public void PreMultiplySameAsTransform()
        {
            var m = Matrix4x4.CreateRotationX(0.56f);
            var v = new Vector3(1, 2, 3);

            var v1 = m.PreMultiply(v);
            var v2 = Vector3.Transform(v, m);

            Assert.Equal(v1, v2);
        }

        [Fact]
        public void TestStackOrder()
        {
            var s = new Stack<int>();
            s.Push(1);
            s.Push(2);
            s.Push(3);
            
            Assert.Equal(1, s.Last());
            Assert.Equal(3, s.First());
            Assert.Equal(3, s.Peek());
        }
    }
}