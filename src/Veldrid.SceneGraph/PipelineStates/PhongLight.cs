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

using System;
using System.Numerics;

namespace Veldrid.SceneGraph.PipelineStates
{
    public interface IPhongLightParameters
    {
        Vector3 AmbientLightColor { get; }
        Vector3 DiffuseLightColor { get; }
        Vector3 SpecularLightColor { get; }
        float LightPower { get; }

        // Set to -1.0 = error
        // Set to  0.0 = constant
        // Set to  1.0 = linear
        // Set to  2.0 = quadratic
        float AttenuationConstant { get; }
    }

    public class PhongLightParameters : IPhongLightParameters
    {
        private PhongLightParameters(
            Vector3 ambientLightColor,
            Vector3 diffuseLightColor,
            Vector3 specularLightColor,
            float lightPower,
            float attenuation)
        {
            AmbientLightColor = ambientLightColor;
            DiffuseLightColor = diffuseLightColor;
            SpecularLightColor = specularLightColor;
            LightPower = lightPower;

            if (attenuation < 0) throw new Exception("Can't have a negative attenuation constant");

            AttenuationConstant = attenuation;
        }

        public Vector3 AmbientLightColor { get; }
        public Vector3 DiffuseLightColor { get; }
        public Vector3 SpecularLightColor { get; }
        public float LightPower { get; }

        public float AttenuationConstant { get; }

        public static IPhongLightParameters Default()
        {
            return Create(
                Vector3.One,
                Vector3.One,
                Vector3.One,
                1,
                0);
        }

        public static IPhongLightParameters Create(
            Vector3 ambientLightColor,
            Vector3 diffuseLightColor,
            Vector3 specularLightColor,
            float lightPower,
            float attenuation)
        {
            return new PhongLightParameters(
                ambientLightColor,
                diffuseLightColor,
                specularLightColor,
                lightPower,
                attenuation);
        }
    }

    public class PhongHeadlight : PhongLight
    {
        internal PhongHeadlight(IPhongLightParameters p) : base(p)
        {
        }

        public static PhongHeadlight Create(IPhongLightParameters p)
        {
            return new PhongHeadlight(p);
        }
    }

    public class PhongPositionalLight : PhongLight
    {
        internal PhongPositionalLight(Vector4 position, IPhongLightParameters p) : base(p)
        {
            Position = position;
        }

        public Vector4 Position { get; }

        public static PhongPositionalLight Create(Vector4 position, IPhongLightParameters p)
        {
            return new PhongPositionalLight(position, p);
        }
    }

    public abstract class PhongLight
    {
        internal PhongLight(IPhongLightParameters p)
        {
            Parameters = p;
        }

        public IPhongLightParameters Parameters { get; }
    }
}