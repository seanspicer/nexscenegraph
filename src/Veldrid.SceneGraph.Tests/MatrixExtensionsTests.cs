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