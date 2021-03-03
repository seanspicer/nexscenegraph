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
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using Veldrid.SceneGraph.AssetPrimitives;

namespace Veldrid.SceneGraph.AssetProcessor
{
    public class ImageSharpProcessor : BinaryAssetProcessor<ProcessedTexture>
    {
        // Taken from Veldrid.ImageSharp

        private static readonly IResampler s_resampler = new Lanczos3Resampler();

        public unsafe ProcessedTexture ProcessT(Image<Rgba32> image)
        {
            var mipmaps = GenerateMipmaps(image, out var totalSize);

            var allTexData = new byte[totalSize];
            long offset = 0;
            fixed (byte* allTexDataPtr = allTexData)
            {
                foreach (var mipmap in mipmaps)
                {
                    long mipSize = mipmap.Width * mipmap.Height * sizeof(Rgba32);
                    fixed (Rgba32* pixelPtr =
                        &MemoryMarshal.GetReference(mipmap.GetPixelSpan())
                    ) //&mipmap.DangerousGetPinnableReferenceToPixelBuffer()))
                    {
                        Buffer.MemoryCopy(pixelPtr, allTexDataPtr + offset, mipSize, mipSize);
                    }

                    offset += mipSize;
                }
            }

            var texData = new ProcessedTexture(
                PixelFormat.R8_G8_B8_A8_UNorm, TextureType.Texture2D,
                (uint) image.Width, (uint) image.Height, 1,
                (uint) mipmaps.Length, 1,
                allTexData);
            return texData;
        }

        public override unsafe ProcessedTexture ProcessT(Stream stream, string extension)
        {
            var image = (Image<Rgba32>) Image.Load(stream);
            var mipmaps = GenerateMipmaps(image, out var totalSize);

            var allTexData = new byte[totalSize];
            long offset = 0;
            fixed (byte* allTexDataPtr = allTexData)
            {
                foreach (var mipmap in mipmaps)
                {
                    long mipSize = mipmap.Width * mipmap.Height * sizeof(Rgba32);
                    fixed (Rgba32* pixelPtr =
                        &MemoryMarshal.GetReference(mipmap.GetPixelSpan())
                    ) //&mipmap.DangerousGetPinnableReferenceToPixelBuffer())
                    {
                        Buffer.MemoryCopy(pixelPtr, allTexDataPtr + offset, mipSize, mipSize);
                    }

                    offset += mipSize;
                }
            }

            var texData = new ProcessedTexture(
                PixelFormat.R8_G8_B8_A8_UNorm, TextureType.Texture2D,
                (uint) image.Width, (uint) image.Height, 1,
                (uint) mipmaps.Length, 1,
                allTexData);
            return texData;
        }

        private static Image<T>[] GenerateMipmaps<T>(Image<T> baseImage, out int totalSize) where T : struct, IPixel<T>
        {
            var mipLevelCount = ComputeMipLevels(baseImage.Width, baseImage.Height);
            var mipLevels = new Image<T>[mipLevelCount];
            mipLevels[0] = baseImage;
            totalSize = baseImage.Width * baseImage.Height * Unsafe.SizeOf<T>();
            var i = 1;

            var currentWidth = baseImage.Width;
            var currentHeight = baseImage.Height;
            while (currentWidth != 1 || currentHeight != 1)
            {
                var newWidth = System.Math.Max(1, currentWidth / 2);
                var newHeight = System.Math.Max(1, currentHeight / 2);
                var newImage = baseImage.Clone(context => context.Resize(newWidth, newHeight, s_resampler));
                Debug.Assert(i < mipLevelCount);
                mipLevels[i] = newImage;

                totalSize += newWidth * newHeight * Unsafe.SizeOf<T>();
                i++;
                currentWidth = newWidth;
                currentHeight = newHeight;
            }

            Debug.Assert(i == mipLevelCount);

            return mipLevels;
        }

        public static int ComputeMipLevels(int width, int height)
        {
            return 1 + (int) System.Math.Floor(System.Math.Log(System.Math.Max(width, height), 2));
        }
    }
}