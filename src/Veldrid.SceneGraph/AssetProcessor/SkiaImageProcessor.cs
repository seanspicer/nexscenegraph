//
// Copyright 2018-2021 Sean Spicer
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

using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;
using Veldrid.SceneGraph.AssetPrimitives;

namespace Veldrid.SceneGraph.AssetProcessor
{
    public class SkiaImageProcessor : BinaryAssetProcessor<ProcessedTexture>
    {
        public override ProcessedTexture ProcessT(Stream stream, string extension)
        {
            using var image = SKBitmap.Decode(stream)
                              ?? throw new InvalidDataException($"Unable to decode '{extension}' image");
            return CreateTexture(image);
        }

        /// <summary>
        ///     Builds a mipmapped R8G8B8A8 (non-premultiplied) texture from <paramref name="baseImage" />.
        ///     Each mip level is resampled from the base image.
        /// </summary>
        internal static ProcessedTexture CreateTexture(SKBitmap baseImage)
        {
            var width = baseImage.Width;
            var height = baseImage.Height;
            var mipLevelCount = ComputeMipLevels(width, height);

            var totalSize = 0L;
            for (int level = 0, w = width, h = height; level < mipLevelCount; level++)
            {
                totalSize += (long) w * h * 4;
                w = System.Math.Max(1, w / 2);
                h = System.Math.Max(1, h / 2);
            }

            var allTexData = new byte[totalSize];
            var handle = GCHandle.Alloc(allTexData, GCHandleType.Pinned);
            try
            {
                var offset = 0L;
                for (int level = 0, w = width, h = height; level < mipLevelCount; level++)
                {
                    var dstInfo = new SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Unpremul);
                    var dst = handle.AddrOfPinnedObject() + (nint) offset;

                    if (level == 0)
                    {
                        ReadPixels(baseImage, dstInfo, dst);
                    }
                    else
                    {
                        using var resized = baseImage.Resize(
                            new SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Premul), SKFilterQuality.High);
                        ReadPixels(resized, dstInfo, dst);
                    }

                    offset += (long) w * h * 4;
                    w = System.Math.Max(1, w / 2);
                    h = System.Math.Max(1, h / 2);
                }
            }
            finally
            {
                handle.Free();
            }

            return new ProcessedTexture(
                PixelFormat.R8_G8_B8_A8_UNorm, TextureType.Texture2D,
                (uint) width, (uint) height, 1,
                (uint) mipLevelCount, 1,
                allTexData);
        }

        private static void ReadPixels(SKBitmap source, SKImageInfo dstInfo, nint dst)
        {
            using var pixmap = source.PeekPixels();
            if (pixmap == null || !pixmap.ReadPixels(dstInfo, dst, dstInfo.RowBytes, 0, 0))
                throw new InvalidDataException("Unable to convert image to RGBA8888");
        }

        public static int ComputeMipLevels(int width, int height)
        {
            return 1 + (int) System.Math.Floor(System.Math.Log(System.Math.Max(width, height), 2));
        }
    }
}
