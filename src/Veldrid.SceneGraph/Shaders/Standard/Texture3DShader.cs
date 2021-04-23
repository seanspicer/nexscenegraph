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
    public class Texture3DShader : StandardShaderBase
    {
        private static readonly Lazy<Texture3DShader> lazy = new Lazy<Texture3DShader>(() => new Texture3DShader());

        private Texture3DShader() : base(@"BasicTexture3DShader", @"BasicTexture3DShader-vertex.glsl",
            "BasicTexture3DShader-fragment.glsl")
        {
        }

        public static Texture3DShader Instance => lazy.Value;
    }
}