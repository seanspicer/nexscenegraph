
using System;
using Veldrid;
using Veldrid.SceneGraph.AssetPrimitives;

namespace Examples.Common
{
    public class Test3DTextures
    {
        
        public static ProcessedTexture SimpleDoubleSphere(int width, int height, int depth)
        {

            var centerWidth = width / 2;
            var centerHeight = height / 2;
            var centerDepth = depth / 2;

            var radius1 = 0.667;
            var radius2 = 0.333;

            var sphere1RadiusSq = (width / 2.0) * radius1;
            sphere1RadiusSq *= sphere1RadiusSq;
            var sphere2RadiusSq = (width / 2.0) * radius2;
            sphere2RadiusSq *= sphere2RadiusSq;
            
            var rgbaData = new UInt32[width * height * depth];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    for (int k = 0; k < depth; k++)
                    {
                        var index = i + height * (j + depth * k);
                        rgbaData[index] = 0x10000FF;

                        var fromCenteri = i - centerWidth;
                        var fromCenterj = j - centerHeight;
                        var fromCenterk = k - centerDepth;

                        if (fromCenteri * fromCenteri + fromCenterj * fromCenterj + fromCenterk * fromCenterk <
                            sphere2RadiusSq)
                        {
                            rgbaData[index] = 0xFFFF0000;
                        }
                        else if (fromCenteri * fromCenteri + fromCenterj * fromCenterj + fromCenterk * fromCenterk <
                                 sphere1RadiusSq)
                        {
                            rgbaData[index] = 0x1000FFFF;  // RGBA ... A is the 4th component
                        }
                    }
                }
                
            }
            
            var allTexData = new byte[width * height * depth * 4]; // RGBA
            Buffer.BlockCopy(rgbaData, 0, allTexData, 0, allTexData.Length);
            
            var texData = new ProcessedTexture(
                PixelFormat.R8_G8_B8_A8_UNorm, TextureType.Texture3D,
                (uint) width, (uint) height, (uint) depth,
                (uint) 1, 1,
                allTexData);
            return texData;
        }
    }
}