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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using AssetPrimitives;
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
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct LightData
    {
        public Vector3 LightColor;
        public float LightPower;
        public Vector3 SpecularColor;
        public float SpecularPower;
        public Vector3 MaterialColor;
        public int MaterialOverride;
        
        public Vector4 Padding;

        public LightData(Vector3 lightColor, float lightPower, Vector3 specularColor, float specularPower,
            Vector3 materialColor, int materialOverride = 0)
        {
            LightColor = lightColor;
            LightPower = lightPower;
            SpecularColor = specularColor;
            SpecularPower = specularPower;
            MaterialColor = materialColor;
            MaterialOverride = materialOverride;

            Padding = Vector4.Zero;
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper.Configure();

            var logger = Veldrid.SceneGraph.Logging.LogManager.CreateLogger<Program>();
            
            var viewer = SimpleViewer.Create("Phong Shaded Dragon Scene Graph");
            viewer.SetCameraManipulator(TrackballManipulator.Create());

            var root = Group.Create();

            var model = CreateDragonModel();

            var geometryFactory = GeometryFactory.Create();
            
            var cube = geometryFactory.CreateCube(VertexType.Position3Texture2Color3Normal3,
                TopologyType.IndexedTriangleList);

            var cubeShape = Box.CreateUnitBox();
            var cubeDrawable = 
                ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    cubeShape, 
                    new TessellationHints(), 
                    new Vector3(0.6f, 0.4f, 0.2f));
            
            var cube2 = Geode.Create();
            cube2.AddDrawable(cubeDrawable);
            
            logger.LogInformation($"Cube Geom is: {cube.Drawables.First().VertexType}");
            
            var cubeXForm = MatrixTransform.Create(Matrix4x4.CreateScale(10f, 10f, 10f));
            cubeXForm.AddChild(cube);
            
            var cubeXForm2 = MatrixTransform.Create(Matrix4x4.CreateScale(10f, 10f, 10f));
            cubeXForm2.AddChild(cube2);

            var leftTop = MatrixTransform.Create(Matrix4x4.CreateTranslation(-10f, 10f, 0f));
            var rightTop = MatrixTransform.Create(Matrix4x4.CreateTranslation(10f, 10f, 0f));

            var leftBottom = MatrixTransform.Create(Matrix4x4.CreateTranslation(-10f, -10f, 0f));
            var rightBottom = MatrixTransform.Create(Matrix4x4.CreateTranslation(10f, -10f, 0f));
            
            leftTop.AddChild(model);
            rightTop.AddChild(model);
            
            leftBottom.AddChild(cubeXForm);
            rightBottom.AddChild(cubeXForm2);

            var flatYellowMaterial = PhongMaterial.Create(
                PhongMaterialParameters.Create(
                    new Vector3(1.0f, 1.0f, 0.0f),
                    new Vector3(1.0f, 1.0f, 0.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    20),
                PhongPositionalLight.Create( new Vector4(0, 100, 0, 1), PhongLightParameters.Create(
                    new Vector3(0.2f, 0.2f, 0.2f),
                    new Vector3(0.2f, 0.2f, 0.2f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    300f,
                    1)));
            
            var shinyRedGoldMaterial = PhongMaterial.Create(
                PhongMaterialParameters.Create(
                    new Vector3(1.0f, 1.0f, 0.0f),
                    new Vector3(1.0f, 1.0f, 0.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    5),
                PhongHeadlight.Create(PhongLightParameters.Create(
                    new Vector3(0.1f, 0.1f, 0.1f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    30f,
                    1)),
                false);
            
            leftTop.PipelineState = flatYellowMaterial.CreatePipelineState();
            rightTop.PipelineState = shinyRedGoldMaterial.CreatePipelineState();
            cube.PipelineState = shinyRedGoldMaterial.CreatePipelineState();
            
//            rightTop.PipelineState = CreateHeadlightState(
//                new Vector3(1.0f, 1.0f, 0.0f), 
//                50,
//                Vector3.One,
//                5);
            
            var sceneGroup = Group.Create();
            sceneGroup.AddChild(leftTop);
            sceneGroup.AddChild(rightTop);
            sceneGroup.AddChild(leftBottom);
            sceneGroup.AddChild(rightBottom);

            sceneGroup.PipelineState = CreateSharedHeadlightState();
            
            root.AddChild(sceneGroup);

            viewer.SetSceneData(root);
            viewer.ViewAll();            
            viewer.Run();
        }
        
        public static Stream OpenEmbeddedAssetStream(string name) => Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
        
        public static byte[] ReadEmbeddedAssetBytes(string name)
        {
            var asm = Assembly.GetExecutingAssembly();
            string[] names = asm.GetManifestResourceNames();
            
            using (Stream stream = OpenEmbeddedAssetStream(name))
            {
                byte[] bytes = new byte[stream.Length];
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    stream.CopyTo(ms);
                    return bytes;
                }
            }
        }
        
        static IGeode CreateDragonModel()
        {
            IGeode result;
            
            using (Stream dragonModelStream = OpenEmbeddedAssetStream(@"Lighting.Assets.Models.chinesedragon.dae"))
            {
                var importer = new Import();
                result = importer.LoadColladaModel(dragonModelStream);
            }
            
            return result;
        }

        private static IPipelineState CreateHeadlightState(
            Vector3 lightColor, 
            float lightPower, 
            Vector3 specularColor,
            float specularPower)
        {
            var pso = PipelineState.Create();

            pso.AddUniform(CreateLight(lightColor, lightPower, specularColor, specularPower, Vector3.One));

            Headlight_Common(ref pso);
            
            return pso;
        }

        private static IPipelineState CreateSharedHeadlightState()
        {
            var pso = PipelineState.Create();
            
            var uniform = Uniform<LightData>.Create(
                "LightData",
                BufferUsage.UniformBuffer | BufferUsage.Dynamic,
                ShaderStages.Vertex, 
                ResourceLayoutElementOptions.DynamicBinding);
            
            var lights = new LightData[]
            {
                // Left Light
                new LightData(
                    new Vector3(1.0f, 1.0f, 1.0f),
                    80,
                    new Vector3(1.0f, 1.0f, 1.0f),
                    5,
                    new Vector3(0.0f, 0.8f, 0.0f),
                1)
                ,
                // Right Light
                new LightData(
                    new Vector3(1.0f, 1.0f, 1.0f),
                    50,
                    new Vector3(1.0f, 1.0f, 1.0f),
                    50,
                    new Vector3(0.0f, 0.0f, 1.0f),
                    1) 
                
            };

            uniform.UniformData = lights;
            pso.AddUniform(uniform);
            
            Headlight_Common(ref pso);
            return pso;
        }

        private static IBindable CreateLight(Vector3 lightColor,
            float lightPower,
            Vector3 specularColor,
            float specularPower,
            Vector3 materialColor,
            int materialOverride = 0)
        {
            var uniform = Uniform<LightData>.Create(
                "LightData",
                BufferUsage.UniformBuffer | BufferUsage.Dynamic,
                ShaderStages.Vertex);
            
            var lights = new LightData[]
            {
                new LightData(
                    lightColor, 
                    lightPower, 
                    specularColor,
                    specularPower,
                    materialColor, 
                    materialOverride)
            };

            uniform.UniformData = lights;

            return uniform;
        }

        private static void Headlight_Common(ref IPipelineState pso)
        {
            var vtxShader =
                new ShaderDescription(ShaderStages.Vertex, ReadEmbeddedAssetBytes(@"Lighting.Assets.Shaders.Phong-vertex.glsl"), "main");
            
            var frgShader =
                new ShaderDescription(ShaderStages.Fragment, ReadEmbeddedAssetBytes(@"Lighting.Assets.Shaders.Phong-fragment.glsl"), "main");
            
            pso.VertexShaderDescription = vtxShader;
            pso.FragmentShaderDescription = frgShader;
        }
    }
}