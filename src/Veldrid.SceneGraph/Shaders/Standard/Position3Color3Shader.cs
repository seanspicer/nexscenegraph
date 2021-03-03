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
    public class Position3Color3Shader : StandardShaderBase
    {
        private static readonly Lazy<Position3Color3Shader> Lazy =
            new Lazy<Position3Color3Shader>(() => new Position3Color3Shader());

        private Position3Color3Shader() : base(@"Position3Color3", @"Position3Color3-vertex.glsl",
            @"Position3Color3-fragment.glsl")
        {
        }

        public static Position3Color3Shader Instance => Lazy.Value;
    }
}