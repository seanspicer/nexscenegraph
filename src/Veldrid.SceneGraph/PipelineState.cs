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

namespace Veldrid.SceneGraph
{
    public class PipelineState : IPipelineState
    {
        public ShaderDescription? VertexShaderDescription { get; set; }
        public ShaderDescription? FragmentShaderDescription { get; set; }
        
        private readonly List<ITexture2D> _textureList = new List<ITexture2D>();
        public IReadOnlyList<ITexture2D> TextureList => _textureList;

        private readonly List<IBindable> _uniformList = new List<IBindable>();
        public IReadOnlyList<IBindable> UniformList => _uniformList;

        private readonly Dictionary<IDrawable, IBindable> _uniformDictionary = new Dictionary<IDrawable, IBindable>();
        public Dictionary<IDrawable, IBindable> UniformDictionary => _uniformDictionary;

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
        
        public void AddUniform(IBindable uniform)
        {
            _uniformList.Add(uniform);
        }

        public void AddUniform(IDrawable drawable, IBindable uniform)
        {
            _uniformDictionary.Add(drawable, uniform);
        }
        
    }
}