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
        private float LightPower;

        public LightData(Vector3 lightColor, float lightPower)
        {
            LightColor = lightColor;
            LightPower = lightPower;
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
            
            root.AddChild(model);

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

            var uniform = Uniform<LightData>.Create(
                "LightData",
                BufferUsage.UniformBuffer | BufferUsage.Dynamic,
                ShaderStages.Vertex);
            
            var lights = new LightData[]
            {
                new LightData(new Vector3(1.0f, 1.0f, 1.0f), 30f), 
            };

            uniform.UniformData = lights;
            
            var vtxShader =
                new ShaderDescription(ShaderStages.Vertex, ReadEmbeddedAssetBytes(@"Lighting.Assets.Shaders.Phong-vertex.glsl"), "main");
            
            var frgShader =
                new ShaderDescription(ShaderStages.Fragment, ReadEmbeddedAssetBytes(@"Lighting.Assets.Shaders.Phong-fragment.glsl"), "main");

            result.PipelineState.AddUniform(uniform);
            result.PipelineState.VertexShaderDescription = vtxShader;
            result.PipelineState.FragmentShaderDescription = frgShader;
            
            return result;
        }
    }
}