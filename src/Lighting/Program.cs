using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using AssetPrimitives;
using Examples.Common;
using SixLabors.ImageSharp;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Viewer;
using Veldrid.SceneGraph.IO;

namespace Lighting
{
    public struct LightData
    {
        public Vector3 LightColor;
        public float LightPower;
        public Vector3 SpecularColor;
        public float SpecularPower;

        public LightData(Vector3 lightColor, float lightPower, Vector3 specularColor, float specularPower)
        {
            LightColor = lightColor;
            LightPower = lightPower;
            SpecularColor = specularColor;
            SpecularPower = specularPower;
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper.Configure();
            
            var viewer = SimpleViewer.Create("Phong Shaded Dragon Scene Graph");
            viewer.SetCameraManipulator(TrackballManipulator.Create());

            var root = Group.Create();

            var model = CreateDragonModel();

            var left = MatrixTransform.Create(Matrix4x4.CreateTranslation(-10f, 0f, 0f));
            var right = MatrixTransform.Create(Matrix4x4.CreateTranslation(10f, 0f, 0f));

            left.AddChild(model);
            right.AddChild(model);

            left.PipelineState = CreateHeadlightState(
                Vector3.One, 
                100,
                Vector3.One,
                30);
            
            right.PipelineState = CreateHeadlightState(
                new Vector3(1.0f, 1.0f, 0.0f), 
                50,
                Vector3.One,
                5);
            
            var sceneGroup = Group.Create();
            sceneGroup.AddChild(left);
            sceneGroup.AddChild(right);
            
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
                    specularPower)
            };

            uniform.UniformData = lights;
            
            pso.AddUniform(uniform);

            Headlight_Common(ref pso);
            
            return pso;
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