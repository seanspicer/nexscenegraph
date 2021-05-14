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

using System.Net;
using System.Numerics;
using NUnit.Framework;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Tests
{
    [TestFixture]
    public class MatrixDecompositionTests
    {
        [TestCase]
        public void PassCanaryTest()
        {
            Assert.That(2 + 2, Is.EqualTo(4));
        }

        [TestCase]
        public void PerformIdentityDecompositionSuccessfully()
        {
            Matrix4x4 mat = Matrix4x4.Identity;

            mat.DecomposeAffine(out var scale, out var rotation, out var translation, out var so);

            Assert.That(scale, Is.EqualTo(Vector3.One));
            Assert.That(translation, Is.EqualTo(Vector3.Zero));
            Assert.That(rotation, Is.EqualTo(Quaternion.Identity));
            Assert.That(so, Is.EqualTo(Quaternion.Identity));
        }
        
        
    }
}