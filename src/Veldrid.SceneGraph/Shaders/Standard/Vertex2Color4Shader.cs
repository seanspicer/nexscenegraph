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
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Shaders.Standard
{
    public class Vertex2Color4Shader
    {
        private static readonly Lazy<Vertex2Color4Shader> Lazy = new Lazy<Vertex2Color4Shader>(() => new Vertex2Color4Shader());

        public static Vertex2Color4Shader Instance => Lazy.Value;

        public ShaderDescription VertexShaderDescription { get; }
        public ShaderDescription FragmentShaderDescription { get; }
        
        private Vertex2Color4Shader()
        {
            var vertexShaderBytes = ShaderTools.LoadShaderBytes(DisplaySettings.Instance.GraphicsBackend,
                typeof(Vertex2Color4Shader).Assembly,
                "Vertex2Color4ShaderSource", ShaderStages.Vertex);
            
            var fragmentShaderBytes = ShaderTools.LoadShaderBytes(DisplaySettings.Instance.GraphicsBackend,
                typeof(Vertex2Color4Shader).Assembly,
                "Vertex2Color4ShaderSource", ShaderStages.Fragment);
            
            VertexShaderDescription = new ShaderDescription(ShaderStages.Vertex, vertexShaderBytes, "VS");
            FragmentShaderDescription = new ShaderDescription(ShaderStages.Fragment, fragmentShaderBytes, "FS");
        }
    }
}