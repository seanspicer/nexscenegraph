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
using Veldrid.SceneGraph.Shaders;

namespace Veldrid.SceneGraph
{
    public interface IPipelineState
    {
        IShaderSet ShaderSet { get; set; }
        IReadOnlyList<ITexture2D> TextureList { get; }
        IReadOnlyList<IUniform> UniformList { get; }
        IReadOnlyList<IVertexBuffer> VertexBufferList { get; }
        BlendStateDescription BlendStateDescription { get; set; }
        DepthStencilStateDescription DepthStencilState { get; set; }
        RasterizerStateDescription RasterizerStateDescription { get; set; }
        void AddTexture(ITexture2D texture);
        void AddUniform(IUniform uniform);
        void AddUniform(IDrawable drawable, IUniform uniform);
        void AddVertexBuffer(IVertexBuffer vertexBuffer);

    }
    
    public class PipelineState : IPipelineState
    {
        public IShaderSet ShaderSet { get; set; }
        
        private readonly List<ITexture2D> _textureList = new List<ITexture2D>();
        public IReadOnlyList<ITexture2D> TextureList => _textureList;

        private readonly List<IUniform> _uniformList = new List<IUniform>();
        public IReadOnlyList<IUniform> UniformList => _uniformList;
        
        private readonly List<IVertexBuffer> _vertexBufferList = new List<IVertexBuffer>();
        public IReadOnlyList<IVertexBuffer> VertexBufferList => _vertexBufferList;

        private readonly Dictionary<IDrawable, IUniform> _uniformDictionary = new Dictionary<IDrawable, IUniform>();
        public Dictionary<IDrawable, IUniform> UniformDictionary => _uniformDictionary;

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
        
        public void AddUniform(IUniform uniform)
        {
            _uniformList.Add(uniform);
        }

        public void AddUniform(IDrawable drawable, IUniform uniform)
        {
            _uniformDictionary.Add(drawable, uniform);
        }

        public void AddVertexBuffer(IVertexBuffer vertexBuffer)
        {
            _vertexBufferList.Add(vertexBuffer);
        }

        
    }
}