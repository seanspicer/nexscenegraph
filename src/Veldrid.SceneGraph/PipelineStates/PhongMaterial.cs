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

using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;

namespace Veldrid.SceneGraph.PipelineStates
{
    public interface IPhongMaterialParameters
    {
        Vector3 AmbientColor { get; }
        Vector3 DiffuseColor { get; }
        Vector3 SpecularColor { get; }
        float Shininess { get; }
    }
    
    public class PhongMaterialParameters : IPhongMaterialParameters
    {
        public Vector3 AmbientColor { get; }
        public Vector3 DiffuseColor { get; }
        public Vector3 SpecularColor { get; }
        public float Shininess { get; }

        public static IPhongMaterialParameters Default()
        {
            return Create(
                Vector3.One,
                Vector3.One,
                Vector3.One,
                5);
        }

        public static IPhongMaterialParameters Create(
            Vector3 ambientColor, 
            Vector3 diffuseColor, 
            Vector3 specularColor,
            float shininess)
        {
            return new PhongMaterialParameters(
                ambientColor, 
                diffuseColor, 
                specularColor,
                shininess);
        }

        private PhongMaterialParameters(
            Vector3 ambientColor, 
            Vector3 diffuseColor, 
            Vector3 specularColor,
            float shininess)
        {
            AmbientColor = ambientColor;
            DiffuseColor = diffuseColor;
            SpecularColor = specularColor;
            Shininess = shininess;
        }
    }

    public interface IPhongMaterial
    {
        IPipelineState CreatePipelineState();
    }

    public struct LightSource
    {
        public Vector3 AmbientColor;
        public float LightPower;
        public Vector3 DiffuseColor;
        public float AttenuationConstant;
        public Vector3 SpecularColor;
        public int IsHeadlight;
        public Vector4 Position;
    }
    
    internal struct Material
    {
        public Vector3 AmbientColor;
        public float Shininess;
        public Vector3 DiffuseColor;
        public float Padding0;
        public Vector3 SpecularColor;
        public int MaterialOverride;
        public Vector4 Padding1;
    }
    
    public class PhongMaterial : IPhongMaterial
    {
        private IPhongMaterialParameters _material;
        private PhongLight _light0;
        private bool _overrideColor;

        public static IPhongMaterial Create(IPhongMaterialParameters p, PhongLight light0, bool overrideColor = true)
        {
            return new PhongMaterial(p, light0, overrideColor);
        }

        internal PhongMaterial(IPhongMaterialParameters p, PhongLight light0, bool overrideColor)
        {
            _material = p;
            _light0 = light0;
            _overrideColor = overrideColor;
        }

        public IPipelineState CreatePipelineState()
        {
            var pso = PipelineState.Create();

            var vtxShader =
                new ShaderDescription(ShaderStages.Vertex, ReadEmbeddedAssetBytes(@"Veldrid.SceneGraph.Assets.Shaders.Phong-vertex.glsl"), "main");
            
            var frgShader =
                new ShaderDescription(ShaderStages.Fragment, ReadEmbeddedAssetBytes(@"Veldrid.SceneGraph.Assets.Shaders.Phong-fragment.glsl"), "main");
            
            pso.VertexShaderDescription = vtxShader;
            pso.FragmentShaderDescription = frgShader;
            
            pso.AddUniform(CreateLightSourceUniform());
            pso.AddUniform(CreateMaterialUniform());
            
            return pso;
        }

        private IBindable CreateLightSourceUniform()
        {
            var lightSourceUniform = Uniform<LightSource>.Create(
                "LightSource", 
                BufferUsage.UniformBuffer | BufferUsage.Dynamic,
                ShaderStages.Vertex | ShaderStages.Fragment);

            lightSourceUniform.UniformData = new LightSource[]
            {
                new LightSource
                {
                    AmbientColor = _light0.Parameters.AmbientLightColor,
                    LightPower = _light0.Parameters.LightPower,
                    DiffuseColor = _light0.Parameters.DiffuseLightColor,
                    AttenuationConstant = _light0.Parameters.AttenuationConstant,
                    SpecularColor = _light0.Parameters.SpecularLightColor,
                    IsHeadlight = _light0 is PhongHeadlight ? 1 : 0,
                    Position = _light0 is PhongPositionalLight light ? light.Position : Vector4.Zero
                }
            };

            return lightSourceUniform;
        }
        
        private IBindable CreateMaterialUniform()
        {
            var materialDescriptionUniform = Uniform<Material>.Create(
                "MaterialDescription", 
                BufferUsage.UniformBuffer | BufferUsage.Dynamic,
                ShaderStages.Vertex | ShaderStages.Fragment);

            materialDescriptionUniform.UniformData = new Material[]
            {
                new Material
                {
                    AmbientColor = _material.AmbientColor,
                    Shininess = _material.Shininess,
                    DiffuseColor = _material.DiffuseColor,
                    Padding0 = 0f,
                    SpecularColor = _material.SpecularColor,
                    MaterialOverride = _overrideColor ? 1 : 0,
                    Padding1 = Vector4.Zero
                }
            };

            return materialDescriptionUniform;
        }
        
        public static byte[] ReadEmbeddedAssetBytes(string name)
        {
            var asm = Assembly.GetCallingAssembly();
            
            using (Stream stream = asm.GetManifestResourceStream(name))
            {
                var bytes = new byte[stream.Length];
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    stream.CopyTo(ms);
                    return bytes;
                }
            }
        }
    }
}