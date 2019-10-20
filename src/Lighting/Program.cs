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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Examples.Common;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using SixLabors.ImageSharp;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Viewer;
using Veldrid.SceneGraph.IO;
using Veldrid.SceneGraph.PipelineStates;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;

namespace Lighting
{
    
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper.Configure();

            var logger = Veldrid.SceneGraph.Logging.LogManager.CreateLogger<Program>();
            
            var viewer = SimpleViewer.Create("Phong Shaded Dragon Scene Graph");
            viewer.SetCameraManipulator(TrackballManipulator.Create());

            var root = LightingExampleScene.Build();

            viewer.SetSceneData(root);
            viewer.ViewAll();            
            viewer.Run();
        }
        
//        public static Stream OpenEmbeddedAssetStream(string name) => Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
        
//        public static byte[] ReadEmbeddedAssetBytes(string name)
//        {
//            var asm = Assembly.GetExecutingAssembly();
//            string[] names = asm.GetManifestResourceNames();
//            
//            using (Stream stream = OpenEmbeddedAssetStream(name))
//            {
//                byte[] bytes = new byte[stream.Length];
//                using (MemoryStream ms = new MemoryStream(bytes))
//                {
//                    stream.CopyTo(ms);
//                    return bytes;
//                }
//            }
//        }
//        
//        static IGeode CreateDragonModel()
//        {
//            IGeode result;
//            
//            using (Stream dragonModelStream = OpenEmbeddedAssetStream(@"Lighting.Assets.Models.chinesedragon.dae"))
//            {
//                var importer = new Import();
//                result = importer.LoadColladaModel(dragonModelStream);
//            }
//            
//            return result;
//        }

//        private static IPipelineState CreateHeadlightState(
//            Vector3 lightColor, 
//            float lightPower, 
//            Vector3 specularColor,
//            float specularPower)
//        {
//            var pso = PipelineState.Create();
//
//            pso.AddUniform(CreateLight(lightColor, lightPower, specularColor, specularPower, Vector3.One));
//
//            Headlight_Common(ref pso);
//            
//            return pso;
//        }
//
//        private static IPipelineState CreateSharedHeadlightState()
//        {
//            var pso = PipelineState.Create();
//            
//            var uniform = Uniform<LightData>.Create(
//                "LightData",
//                BufferUsage.UniformBuffer | BufferUsage.Dynamic,
//                ShaderStages.Vertex, 
//                ResourceLayoutElementOptions.DynamicBinding);
//            
//            var lights = new LightData[]
//            {
//                // Left Light
//                new LightData(
//                    new Vector3(1.0f, 1.0f, 1.0f),
//                    80,
//                    new Vector3(1.0f, 1.0f, 1.0f),
//                    5,
//                    new Vector3(0.0f, 0.8f, 0.0f),
//                1)
//                ,
//                // Right Light
//                new LightData(
//                    new Vector3(1.0f, 1.0f, 1.0f),
//                    50,
//                    new Vector3(1.0f, 1.0f, 1.0f),
//                    50,
//                    new Vector3(0.0f, 0.0f, 1.0f),
//                    1) 
//                
//            };
//
//            uniform.UniformData = lights;
//            pso.AddUniform(uniform);
//            
//            Headlight_Common(ref pso);
//            return pso;
//        }

//        private static IBindable CreateLight(Vector3 lightColor,
//            float lightPower,
//            Vector3 specularColor,
//            float specularPower,
//            Vector3 materialColor,
//            int materialOverride = 0)
//        {
//            var uniform = Uniform<LightData>.Create(
//                "LightData",
//                BufferUsage.UniformBuffer | BufferUsage.Dynamic,
//                ShaderStages.Vertex);
//            
//            var lights = new LightData[]
//            {
//                new LightData(
//                    lightColor, 
//                    lightPower, 
//                    specularColor,
//                    specularPower,
//                    materialColor, 
//                    materialOverride)
//            };
//
//            uniform.UniformData = lights;
//
//            return uniform;
//        }
//
//        private static void Headlight_Common(ref IPipelineState pso)
//        {
//            var vtxShader =
//                new ShaderDescription(ShaderStages.Vertex, ReadEmbeddedAssetBytes(@"Lighting.Assets.Shaders.Phong-vertex.glsl"), "main");
//            
//            var frgShader =
//                new ShaderDescription(ShaderStages.Fragment, ReadEmbeddedAssetBytes(@"Lighting.Assets.Shaders.Phong-fragment.glsl"), "main");
//            
//            pso.VertexShaderDescription = vtxShader;
//            pso.FragmentShaderDescription = frgShader;
//        }
//        
//        private static List<Vector3> MakeColorGradient(float frequency1, float frequency2, float frequency3,
//            float phase1, float phase2, float phase3, uint len)
//        {
//            var center = 128;
//            var width = 127;
//
//            var result = new List<Vector3>();
//            
//            for (var i = 0; i < len; ++i)
//            {
//                var red = (float)System.Math.Sin(frequency1*i + phase1) * width + center;
//                var grn = (float)System.Math.Sin(frequency2*i + phase2) * width + center;
//                var blu = (float)System.Math.Sin(frequency3*i + phase3) * width + center;
//                
//                result.Add(new Vector3(red/255f,grn/255f, blu/255f));
//            }
//
//            return result;
//        }
    }
}