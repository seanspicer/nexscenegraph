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

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Tests
{
    [TestFixture]
    public class MatrixExtensionsTests
    {
        [TestCase]
        public void PassCanaryTest()
        {
            Assert.That(4, Is.EqualTo(2 + 2));
        }

        [TestCase]
        public void PreMultiplyWithIdentityReturnsSame()
        {
            var v = new Vector3(1, 2, 3);

            Assert.That(v, Is.EqualTo(Matrix4x4.Identity.PreMultiply(v)));
        }

        [TestCase]
        public void PostMultiplyWithIdentityReturnsSame()
        {
            var v = new Vector3(1, 2, 3);

            Assert.That(v, Is.EqualTo(Matrix4x4.Identity.PostMultiply(v)));
        }

        [TestCase]
        public void PreMultiplyIsSameAsPostMultiplyInverse()
        {
            var m = Matrix4x4.CreateRotationX(0.56f);
            var v = new Vector3(1, 2, 3);

            Matrix4x4 inv;
            Matrix4x4.Invert(m, out inv);

            var v1 = m.PreMultiply(v);
            var v2 = inv.PostMultiply(v);

            Assert.That(v1, Is.EqualTo(v2));
        }

        [TestCase]
        public void PreMultiplyNotSameAsTransformIfScaled()
        {
            var m = Matrix4x4.CreateRotationX(0.56f);
            m.M34 = -1;
            m.M43 = 40;
            m.M44 = 41;
            var v = new Vector3(1, 2, 3);

            var v1 = m.PreMultiply(v);
            var v2 = Vector3.Transform(v, m);

            Assert.That(v1, Is.Not.EqualTo(v2));
        }

        [TestCase]
        public void PreMultiplySameAsTransformIfNoScale()
        {
            var m = Matrix4x4.CreateRotationX(0.56f);
            var v = new Vector3(1, 2, 3);

            var v1 = m.PreMultiply(v);
            var v2 = Vector3.Transform(v, m);

            Assert.That(v1, Is.EqualTo(v2));
        }

        [TestCase]
        public void TestStackOrder()
        {
            var s = new Stack<int>();
            s.Push(1);
            s.Push(2);
            s.Push(3);

            Assert.That(1, Is.EqualTo(s.Last()));
            Assert.That(3, Is.EqualTo(s.First()));
            Assert.That(3, Is.EqualTo(s.Peek()));
        }
    }
}