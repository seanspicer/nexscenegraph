// #define USE_CORNER

using System;
using System.Reflection;
using Examples.Common;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.AssetPrimitives;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Logging;
using Veldrid.SceneGraph.Math.IsoSurface;
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

            var viewer = SimpleViewer.Create("Direct Volume Rendering w/ 3D Textures", TextureSampleCount.Count8);
            viewer.SetBackgroundColor(RgbaFloat.Black);
            viewer.SetCameraManipulator(TrackballManipulator.Create());

            var (root, eventHandler) = SampledVolumeRenderingExampleScene.Build(CreateShaderSet, 
                new TestVoxelVolume(256, 256, 256), GenerateVolume, 
                GenerateColormap);

            viewer.SetSceneData(root);
            viewer.AddInputEventHandler(eventHandler);
            viewer.ViewAll();
            viewer.Run();
        }
        
        public static IShaderSet CreateShaderSet()
        {
            var asm = Assembly.GetCallingAssembly();

            var vtxShader =
                new ShaderDescription(ShaderStages.Vertex,
                    ShaderTools.ReadEmbeddedAssetBytes(
                        @"Examples.Common.Assets.Shaders.ProceduralVolumeShader-vertex.glsl", asm), "main", true);

            var frgShader =
                new ShaderDescription(ShaderStages.Fragment,
                    ShaderTools.ReadEmbeddedAssetBytes(
                        @"Examples.Common.Assets.Shaders.TextureVolumeShader-fragment.glsl", asm), "main", true);

            return ShaderSet.Create("TextureVolumeShader", vtxShader, frgShader);
        }

        private static ITexture3D GenerateVolume(IVoxelVolume voxelVolume)
        {
            var xdim = voxelVolume.XValues.GetLength(0);
            var ydim = voxelVolume.XValues.GetLength(1);
            var zdim = voxelVolume.XValues.GetLength(2);
            
            var allTexData = VolumeData(voxelVolume, xdim, ydim, zdim);
            
            var texData = new ProcessedTexture(
                PixelFormat.R8_UNorm, TextureType.Texture3D,
                (uint) xdim, (uint) ydim, (uint) zdim,
                (uint) 1, 1,
                allTexData);
            
            return Texture3D.Create(texData, 1,
                "VolumeTexture", "VolumeSampler");
        }

        private static ITexture1D GenerateColormap(uint colormapSize)
        {
            var rgbaData = new byte[colormapSize*4];

            for (var i = 0; i<colormapSize; ++i)
            {
                var val = i / ((float)colormapSize-1);
                
                var index = 4 * i;
                
                rgbaData[index + 0] = 0; // R
                rgbaData[index + 1] = 255; // G
                rgbaData[index + 2] = 0; // B
                rgbaData[index + 3] = 255; // A
                
                // rgbaData[index] = 0x10000FF;  // RGBA ... A is the 4th component ... solid blue
                if (val > 0.9)
                {
                    rgbaData[index + 0] = 255; // R
                    rgbaData[index + 1] = 0; // G
                    rgbaData[index + 2] = 0; // B
                    rgbaData[index + 3] = 0; // A
                }
                else if(val < 0.1)
                {
                    // rgbaData[index] = 0xFFFF0000;  // RGBA ... A is the 4th component ... solid blue
                    rgbaData[index + 0] = 0;
                    rgbaData[index + 1] = 0;
                    rgbaData[index + 2] = 255;
                    rgbaData[index + 3] = 255;
                }
                
                // else if (i > 192)
                // {
                //     // rgbaData[index] = 0x1000FFFF;  // RGBA ... A is the 4th component ... transparent yellow
                //     rgbaData[index + 0] = 0x00;
                //     rgbaData[index + 1] = 0xFF;
                //     rgbaData[index + 2] = 0x00;
                //     rgbaData[index + 3] = 0xFF;
                // }
            }
            
            var texData = new ProcessedTexture(
                PixelFormat.R8_G8_B8_A8_UNorm, TextureType.Texture1D,
                (uint) colormapSize, 1, 1, 
                (uint) 1, 1,
                rgbaData);

            var samplerDescription = SamplerDescription.Linear;
            samplerDescription.AddressModeU = SamplerAddressMode.Clamp;
            samplerDescription.AddressModeV = SamplerAddressMode.Clamp;
            samplerDescription.AddressModeW = SamplerAddressMode.Clamp;
            
            return Texture1D.Create(texData, samplerDescription, 1,
                "ColormapTexture", "ColormapSampler");
        }
        
        private static byte[] VolumeData(IVoxelVolume voxelVolume, int xdim, int ydim, int zdim)
        {
            var volData = new byte[xdim * ydim * zdim];

            for (var x = 0; x < xdim; ++x)
            for (var y = 0; y < ydim; ++y)
            for (var z = 0; z < zdim; ++z)
            {
                var index = (y * xdim + x) + (z*xdim*ydim);
                volData[index] = (byte) (System.Math.Floor(voxelVolume.Values[x, y, z]*255.0));
                
                // if (voxelVolume.Values[x, y, z] > 0.6) // inner sphere
                // {
                //     rgbaData[index] = 68;
                // }
                // else if (voxelVolume.Values[x, y, z] > 0.1)
                // {
                //     rgbaData[index] = 200;
                // }
            }
            return volData;
        }
    }
    
    public class TestVoxelVolume : IVoxelVolume
    {
        public double[,,] Values { get; }
        public double[,,] XValues { get; }
        public double[,,] YValues { get; }
        public double[,,] ZValues { get; }

        public TestVoxelVolume(int width, int height, int depth)
        {

            var centerWidth = width / 2;
            var centerHeight = height / 2;
            var centerDepth = depth / 2;

            var outerRadius = 0.667;
            var innerRadius = 0.333;

            var outerSphereRadiusSq = (width / 2.0) * outerRadius;
            outerSphereRadiusSq *= outerSphereRadiusSq;
            var innerSphereRadiusSq = (width / 2.0) * innerRadius;
            innerSphereRadiusSq *= innerSphereRadiusSq;
            
            Values = new double[width, height, depth];
            XValues = new double[width, height, depth];
            YValues = new double[width, height, depth];
            ZValues = new double[width, height, depth]; // row major
            for (var x = 0; x < width; ++x)
            for (var y = 0; y < height; ++y)
            for (var z = 0; z < depth; ++z)
            {
                XValues[x, y, z] = x;
                YValues[x, y, z] = y;
                ZValues[x, y, z] = z;
                Values[x, y, z] = 0;
                
                var fromCenteri = x - centerWidth;
                var fromCenterj = y - centerHeight;
                var fromCenterk = z - centerDepth;
                
                if (fromCenteri * fromCenteri + fromCenterj * fromCenterj + fromCenterk * fromCenterk <
                    innerSphereRadiusSq)
                {
                    Values[x, y, z] = 1;
                }
                else if (fromCenteri * fromCenteri + fromCenterj * fromCenterj + fromCenterk * fromCenterk <
                         outerSphereRadiusSq)
                {
                    Values[x, y, z] = 0.5;
                }
            }
        }
    }
}