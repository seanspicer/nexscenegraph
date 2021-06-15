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

using System;

namespace Veldrid.SceneGraph.Shaders.Standard
{
    public class Vertex3Color4Shader : StandardShaderBase
    {
        private static readonly Lazy<Vertex3Color4Shader> Lazy =
            new Lazy<Vertex3Color4Shader>(() => new Vertex3Color4Shader());

        // private Vertex3Color4Shader() : base(@"Vertex3Color4", @"Vertex3Color4-vertex.glsl",
        //     @"Vertex3Color4-fragment.glsl")
        // {
        // }

        private Vertex3Color4Shader() : base(@"Vertex3Color4")
        {
        }
        
        public static Vertex3Color4Shader Instance => Lazy.Value;
    }
}