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

using System;
using Veldrid.SceneGraph.Text;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Shaders.Standard
{
    public class Texture2DShader
    {
        private static readonly Lazy<Texture2DShader> lazy = new Lazy<Texture2DShader>(() => new Texture2DShader());

        public static Texture2DShader Instance => lazy.Value;

        public ShaderDescription VertexShaderDescription { get; }
        public ShaderDescription FragmentShaderDescription { get; }
        
        private Texture2DShader()
        {
            var vertexShaderBytes = ShaderTools.LoadShaderBytes(GraphicsBackend.Vulkan,
                typeof(TextNode).Assembly,
                "BasicTextureShader", ShaderStages.Vertex);
            
            var fragmentShaderBytes = ShaderTools.LoadShaderBytes(GraphicsBackend.Vulkan,
                typeof(TextNode).Assembly,
                "BasicTextureShader", ShaderStages.Fragment);
            
            VertexShaderDescription = new ShaderDescription(ShaderStages.Vertex, vertexShaderBytes, "VS");
            FragmentShaderDescription = new ShaderDescription(ShaderStages.Fragment, fragmentShaderBytes, "FS");
        }
    }
}