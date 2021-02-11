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
using Veldrid.SceneGraph.RenderGraph;
using Veldrid.SceneGraph.Shaders;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.NodeKits.DirectVolumeRendering
{
    public interface ILevoyCabralTechnique : IVolumeTechnique
    {
    }

    public class LevoyCabralTechnique : VolumeTechnique, ILevoyCabralTechnique
    {
        protected IShaderSet ShaderSet { get; private set; }
        
        protected INode Node { get; set; }
        
        public static ILevoyCabralTechnique Create(IShaderSet shaderSet = null)
        {
            return new LevoyCabralTechnique(shaderSet);
        }

        protected LevoyCabralTechnique(IShaderSet shaderSet)
        {
            ShaderSet = shaderSet;
        }
        
        public override void Init()
        {
            if (_volumeTile?.Layer == null)
            {
                return;
            }
            
            // Create the Geometry Placeholder
            CreateSlices();
        }

        public override void Update(IUpdateVisitor nv)
        {
            // Nothing to do here
        }

        public override void Cull(ICullVisitor cv)
        {
            if (null != Node)
            {
                Node.Accept(cv);
            }
        }

        protected IGeode CreateSlices()
        {
            var geode = Geode.Create();
            
            var geometry = Geometry<Position3TexCoord3Color4>.Create();
            
            geometry.VertexLayouts = new List<VertexLayoutDescription>()
            {
                Position3TexCoord3Color4.VertexLayoutDescription
            };
            
            geometry.PipelineState.ShaderSet = ShaderSet ?? Position3TexCoord3Color4Shader.Instance.ShaderSet;
            geometry.PipelineState.BlendStateDescription = BlendStateDescription.SingleAlphaBlend;
            geometry.PipelineState.RasterizerStateDescription = new RasterizerStateDescription
            {
                CullMode = FaceCullMode.Back,
                FillMode = PolygonFillMode.Solid,
                DepthClipEnabled = false,
                FrontFace = FrontFace.CounterClockwise
            };
            
            geode.AddDrawable(geometry);
            return geode;
        }
    }
}