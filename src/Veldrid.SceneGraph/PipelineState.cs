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

using System.Collections.Generic;
using Veldrid.SceneGraph.Shaders;

namespace Veldrid.SceneGraph
{
    public interface IPipelineState
    {
        IShaderSet ShaderSet { get; set; }
        string EmbeddedShaderName { get; set; }
        IReadOnlyList<ITexture> TextureList { get; }
        IReadOnlyList<IUniform> UniformList { get; }
        IReadOnlyList<IVertexBuffer> VertexBufferList { get; }
        BlendStateDescription BlendStateDescription { get; set; }
        DepthStencilStateDescription DepthStencilState { get; set; }
        RasterizerStateDescription RasterizerStateDescription { get; set; }
        void AddTexture(ITexture texture);
        void AddUniform(IUniform uniform);
        void AddUniform(IDrawable drawable, IUniform uniform);
        void AddVertexBuffer(IVertexBuffer vertexBuffer);
    }

    public class PipelineState : IPipelineState
    {
        private readonly List<ITexture> _textureList = new List<ITexture>();

        private readonly List<IUniform> _uniformList = new List<IUniform>();

        private readonly List<IVertexBuffer> _vertexBufferList = new List<IVertexBuffer>();

        private PipelineState()
        {
            // Nothing to see here.
        }

        public Dictionary<IDrawable, IUniform> UniformDictionary { get; } = new Dictionary<IDrawable, IUniform>();
        public IShaderSet ShaderSet { get; set; }
        public string EmbeddedShaderName { get; set; }
        public IReadOnlyList<ITexture> TextureList => _textureList;
        public IReadOnlyList<IUniform> UniformList => _uniformList;
        public IReadOnlyList<IVertexBuffer> VertexBufferList => _vertexBufferList;

        public BlendStateDescription BlendStateDescription { get; set; } = BlendStateDescription.SingleOverrideBlend;

        public DepthStencilStateDescription DepthStencilState { get; set; } =
            DepthStencilStateDescription.DepthOnlyLessEqual;

        public RasterizerStateDescription RasterizerStateDescription { get; set; } = RasterizerStateDescription.Default;

        public void AddTexture(ITexture texture)
        {
            _textureList.Add(texture);
        }

        public void AddUniform(IUniform uniform)
        {
            _uniformList.Add(uniform);
        }

        public void AddUniform(IDrawable drawable, IUniform uniform)
        {
            UniformDictionary.Add(drawable, uniform);
        }

        public void AddVertexBuffer(IVertexBuffer vertexBuffer)
        {
            _vertexBufferList.Add(vertexBuffer);
        }

        public static IPipelineState Create()
        {
            return new PipelineState();
        }
    }
}