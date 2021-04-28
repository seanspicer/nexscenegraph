#define USE_CORNER

using System;
using System.Reflection;
using Examples.Common;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.AssetPrimitives;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Logging;
using Veldrid.SceneGraph.NodeKits.DirectVolumeRendering;
using Veldrid.SceneGraph.Shaders;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Viewer;

namespace DirectVolRen3DTexture
{
    class Program
    {
        private static void Main(string[] args)
        {
            Bootstrapper.Configure();
            LogManager.SetLogger(Bootstrapper.LoggerFactory);

            var viewer = SimpleViewer.Create("Direct Volume Rendering", TextureSampleCount.Count8);
            viewer.SetBackgroundColor(RgbaFloat.Black);
            viewer.SetCameraManipulator(TrackballManipulator.Create());

            var root = SampledVolumeRenderingExampleScene.Build(CreateShaderSet, 
                new TextureVoxelVolume(Test3DTextures.SimpleDoubleSphere()));

            viewer.SetSceneData(root);
            viewer.ViewAll();
            viewer.Run();
        }
        
        public static IShaderSet CreateShaderSet()
        {
            var asm = Assembly.GetCallingAssembly();

            var vtxShader =
                new ShaderDescription(ShaderStages.Vertex,
                    ShaderTools.ReadEmbeddedAssetBytes(
                        @"Examples.Common.Assets.Shaders.ProceduralVolumeShader-vertex.glsl", asm), "main");

            var frgShader =
                new ShaderDescription(ShaderStages.Fragment,
                    ShaderTools.ReadEmbeddedAssetBytes(
                        @"Examples.Common.Assets.Shaders.TextureVolumeShader-fragment.glsl", asm), "main");

            return ShaderSet.Create("TextureVolumeShader", vtxShader, frgShader);
        }
    }
    
#if (USE_CORNER == true)
    public class TextureVoxelVolume : CornerVoxelVolume, ITextureVoxelVolume
    {
        public ITexture3D TextureData { get; }
        
        public TextureVoxelVolume(ProcessedTexture processedTexture)
        {
            TextureData = Texture3D.Create(processedTexture,
                1,
                "SurfaceTexture",
                "SurfaceSampler");
        }

    }
#else
    public class TextureVoxelVolume : ITextureVoxelVolume
    {
        public double[,,] Values { get; }
        public double[,,] XValues { get; }
        public double[,,] YValues { get; }
        public double[,,] ZValues { get; }
        public ITexture3D TextureData { get; }

        public TextureVoxelVolume(ProcessedTexture processedTexture)
        {
            TextureData = Texture3D.Create(processedTexture,
                1,
                "SurfaceTexture",
                "SurfaceSampler");

            Values = new double[processedTexture.Width, processedTexture.Height, processedTexture.Depth];
            XValues = new double[processedTexture.Width, processedTexture.Height, processedTexture.Depth];
            YValues = new double[processedTexture.Width, processedTexture.Height, processedTexture.Depth];
            ZValues = new double[processedTexture.Width, processedTexture.Height, processedTexture.Depth];
            for (var z = 0; z < processedTexture.Depth; ++z)
            for (var y = 0; y < processedTexture.Height; ++y)
            for (var x = 0; x < processedTexture.Width; ++x)
            {
                XValues[x, y, z] = x;
                YValues[x, y, z] = y;
                ZValues[x, y, z] = z;
                Values[x, y, z] = 0;
            }
        }
    }
#endif
}