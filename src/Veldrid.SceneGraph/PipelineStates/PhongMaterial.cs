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
using System.IO;
using System.Numerics;
using System.Reflection;
using Veldrid.SceneGraph.Shaders;

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
        
        void SetMaterial(Vector3 ambientColor, Vector3 diffuseColor, Vector3 specularColor, float shininess);
    }

    public struct LightSource
    {
        public Vector3 AmbientColor;
        public float LightPower;
        public Vector3 DiffuseColor;
        public float AttenuationConstant;
        public Vector3 SpecularColor;
        public float IsHeadlight;
        public Vector4 Position;
    }
    
    public struct Material
    {
        public Vector3 AmbientColor;
        public float Shininess;
        public Vector3 DiffuseColor;
        public float Padding0;
        public Vector3 SpecularColor;
        public float MaterialOverride;
        public Vector4 Padding1;
    }
    
    public class PhongMaterial : IPhongMaterial
    {
        private IPhongMaterialParameters _material;
        private PhongLight _light0;
        private bool _overrideColor;

        private IUniform<Material> _materialUniform;
        private IUniform<LightSource> _lightSourceUniform;
        private IPipelineState _pso;
        
        public static IPhongMaterial Create(IPhongMaterialParameters p, PhongLight light0, bool overrideColor = true)
        {
            return new PhongMaterial(p, light0, overrideColor);
        }

        protected PhongMaterial(IPhongMaterialParameters p, PhongLight light0, bool overrideColor)
        {
            _pso = null;
            _material = p;
            _light0 = light0;
            _overrideColor = overrideColor;
        }

        public void SetMaterial(Vector3 ambientColor, Vector3 diffuseColor, Vector3 specularColor, float shininess)
        {
            _material = PhongMaterialParameters.Create(ambientColor, diffuseColor, specularColor, shininess);
            
            if (null != _materialUniform)
            {
                _materialUniform.UniformData[0].AmbientColor =
                    _light0.Parameters.AmbientLightColor * _material.AmbientColor;
                _materialUniform.UniformData[0].DiffuseColor =
                    _light0.Parameters.DiffuseLightColor * _material.DiffuseColor;
                _materialUniform.UniformData[0].SpecularColor =
                    _light0.Parameters.SpecularLightColor * _material.SpecularColor;
                _materialUniform.UniformData[0].Shininess = _material.Shininess;
                
                _materialUniform.Dirty();
            }
        }
        
        public virtual IPipelineState CreatePipelineState()
        {
            if (null != _pso) return _pso;
            
            _pso = PipelineState.Create();

            var vtxShader =
                new ShaderDescription(ShaderStages.Vertex, ReadEmbeddedAssetBytes(@"Veldrid.SceneGraph.Assets.Shaders.Phong-vertex.glsl"), "main");
            
            var frgShader =
                new ShaderDescription(ShaderStages.Fragment, ReadEmbeddedAssetBytes(@"Veldrid.SceneGraph.Assets.Shaders.Phong-fragment.glsl"), "main");

            _pso.ShaderSet = ShaderSet.Create("PhongShader", vtxShader, frgShader);

            _lightSourceUniform = CreateLightSourceUniform();
            _materialUniform = CreateMaterialUniform();
            
            _pso.AddUniform(_lightSourceUniform);
            _pso.AddUniform(_materialUniform);
            
            return _pso;
        }

        protected IUniform<LightSource> CreateLightSourceUniform()
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
        
        protected IUniform<Material> CreateMaterialUniform()
        {
            var materialDescriptionUniform = Uniform<Material>.Create(
                "MaterialDescription", 
                BufferUsage.UniformBuffer | BufferUsage.Dynamic,
                ShaderStages.Vertex | ShaderStages.Fragment);

            materialDescriptionUniform.UniformData = new Material[]
            {
                new Material
                {
                    AmbientColor = _light0.Parameters.AmbientLightColor*_material.AmbientColor,
                    Shininess = _material.Shininess,
                    DiffuseColor = _light0.Parameters.DiffuseLightColor*_material.DiffuseColor,
                    Padding0 = 0f,
                    SpecularColor = _light0.Parameters.SpecularLightColor*_material.SpecularColor,
                    MaterialOverride = _overrideColor ? 1 : 0,
                    Padding1 = Vector4.Zero
                }
            };

            return materialDescriptionUniform;
        }
        
        protected byte[] ReadEmbeddedAssetBytes(string name)
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