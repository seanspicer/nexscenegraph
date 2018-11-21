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

namespace Veldrid.SceneGraph
{
    public class PipelineState : IPipelineState
    {
        public ShaderDescription? VertexShaderDescription { get; set; }
        public ShaderDescription? FragmentShaderDescription { get; set; }
        
        private List<ITexture2D> _textureList = new List<ITexture2D>();
        public IReadOnlyList<ITexture2D> TextureList => _textureList;

        public BlendStateDescription BlendStateDescription { get; set; } = BlendStateDescription.SingleOverrideBlend;

        public DepthStencilStateDescription DepthStencilState { get; set; } =
            DepthStencilStateDescription.DepthOnlyLessEqual;

        public RasterizerStateDescription RasterizerStateDescription { get; set; } = RasterizerStateDescription.Default;

        public static IPipelineState Create()
        {
            return new PipelineState();
        }
        
        private PipelineState()
        {
            // Nothing to see here.
        }

        public void AddTexture(ITexture2D texture)
        {
            _textureList.Add(texture);
        }
    }
}