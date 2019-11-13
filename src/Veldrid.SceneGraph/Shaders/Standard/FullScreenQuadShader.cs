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

using System;
using System.Reflection;
using Veldrid.SceneGraph.Text;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Shaders.Standard
{
    public class FullScreenQuadShader : StandardShaderBase
    {
        private static readonly Lazy<FullScreenQuadShader> lazy = new Lazy<FullScreenQuadShader>(() => new FullScreenQuadShader());

        public static FullScreenQuadShader Instance => lazy.Value;
        
        private FullScreenQuadShader() : base(@"FullScreenQuadShader", @"FullScreenQuadShader-vertex.glsl", @"FullScreenQuadShader-fragment.glsl")
        {

        }
    }
}