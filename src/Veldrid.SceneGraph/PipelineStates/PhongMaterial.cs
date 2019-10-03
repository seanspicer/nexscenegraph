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
using System.Numerics;

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
        public int AttenuationConstant;
        public Vector3 SpecularColor;
        public int IsHeadlight;
        public Vector3 Position;
        public float Padding0;
    }
    
    internal struct Material
    {
        public Vector3 AmbientColor;
        public float Shininess;
        public Vector3 DiffuseColor;
        public float Padding0;
        public Vector3 SpecularColor;
        public int MaterialOverride;
        public Vector4 Padding2;
    }
    
    public class PhongMaterial : IPhongMaterial
    {
        private PhongMaterialParameters _material;
        private PhongLight _light0;
        
        public static IPhongMaterial Create(PhongMaterialParameters p, PhongLight light0)
        {
            return new PhongMaterial(p, light0);
        }

        internal PhongMaterial(PhongMaterialParameters p, PhongLight light0)
        {
            _material = p;
            _light0 = light0;
        }

        public IPipelineState CreatePipelineState()
        {
            throw new System.NotImplementedException();
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
                    AttenuationConstant = (int)_light0.Parameters.Attenuation,
                    SpecularColor = _light0.Parameters.SpecularLightColor,
                    IsHeadlight = _light0 is PhongHeadlight ? 1 : 0,
                    Position = _light0 is PhongPositionalLight light ? light.Position : Vector3.Zero
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
                    MaterialOverride = 0,
                    Padding2 = Vector4.Zero
                }
            };

            return materialDescriptionUniform;
        }
    }
}