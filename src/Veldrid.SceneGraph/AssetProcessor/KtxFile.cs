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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Veldrid.SceneGraph.AssetProcessor
{
    // A hand-crafted KTX file parser.
    // https://www.khronos.org/opengles/sdk/tools/KTX/file_format_spec
    public class KtxFile
    {
        public KtxFile(KtxHeader header, KtxKeyValuePair[] keyValuePairs, KtxMipmapLevel[] mipmaps)
        {
            Header = header;
            KeyValuePairs = keyValuePairs;
            Mipmaps = mipmaps;
        }

        public KtxHeader Header { get; }
        public KtxKeyValuePair[] KeyValuePairs { get; }
        public KtxMipmapLevel[] Mipmaps { get; }

        public static KtxFile Load(byte[] bytes, bool readKeyValuePairs)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return Load(ms, readKeyValuePairs);
            }
        }

        public static KtxFile Load(Stream s, bool readKeyValuePairs)
        {
            using (var br = new BinaryReader(s))
            {
                var header = ReadStruct<KtxHeader>(br);

                KtxKeyValuePair[] kvps = null;
                if (readKeyValuePairs)
                {
                    var keyValuePairBytesRead = 0;
                    var keyValuePairs = new List<KtxKeyValuePair>();
                    while (keyValuePairBytesRead < header.BytesOfKeyValueData)
                    {
                        var bytesRemaining = (int) (header.BytesOfKeyValueData - keyValuePairBytesRead);
                        var kvp = ReadNextKeyValuePair(br, out var read);
                        keyValuePairBytesRead += read;
                        keyValuePairs.Add(kvp);
                    }

                    kvps = keyValuePairs.ToArray();
                }
                else
                {
                    br.BaseStream.Seek(header.BytesOfKeyValueData, SeekOrigin.Current); // Skip over header data.
                }

                var numberOfMipmapLevels = System.Math.Max(1, header.NumberOfMipmapLevels);
                var numberOfArrayElements = System.Math.Max(1, header.NumberOfArrayElements);
                var numberOfFaces = System.Math.Max(1, header.NumberOfFaces);

                var baseWidth = System.Math.Max(1, header.PixelWidth);
                var baseHeight = System.Math.Max(1, header.PixelHeight);
                var baseDepth = System.Math.Max(1, header.PixelDepth);

                var images = new KtxMipmapLevel[numberOfMipmapLevels];
                for (var mip = 0; mip < numberOfMipmapLevels; mip++)
                {
                    var mipWidth = System.Math.Max(1, baseWidth / (uint) System.Math.Pow(2, mip));
                    var mipHeight = System.Math.Max(1, baseHeight / (uint) System.Math.Pow(2, mip));
                    var mipDepth = System.Math.Max(1, baseDepth / (uint) System.Math.Pow(2, mip));

                    var imageSize = br.ReadUInt32();
                    var arrayElementSize = imageSize / numberOfArrayElements;
                    var arrayElements = new KtxArrayElement[numberOfArrayElements];
                    for (var arr = 0; arr < numberOfArrayElements; arr++)
                    {
                        var faceSize = arrayElementSize / numberOfFaces;
                        var faces = new KtxFace[numberOfFaces];
                        for (var face = 0; face < numberOfFaces; face++)
                            faces[face] = new KtxFace(br.ReadBytes((int) faceSize));

                        arrayElements[arr] = new KtxArrayElement(faces);
                    }

                    images[mip] = new KtxMipmapLevel(
                        mipWidth,
                        mipHeight,
                        mipDepth,
                        imageSize,
                        arrayElementSize,
                        arrayElements);

                    var mipPaddingBytes = 3 - (imageSize + 3) % 4;
                    br.BaseStream.Seek(mipPaddingBytes, SeekOrigin.Current);
                }

                return new KtxFile(header, kvps, images);
            }
        }

        private static unsafe KtxKeyValuePair ReadNextKeyValuePair(BinaryReader br, out int bytesRead)
        {
            var keyAndValueByteSize = br.ReadUInt32();
            var keyAndValueBytes = stackalloc byte[(int) keyAndValueByteSize];
            ReadBytes(br, keyAndValueBytes, (int) keyAndValueByteSize);
            var paddingByteCount = (int) (3 - (keyAndValueByteSize + 3) % 4);
            br.BaseStream.Seek(paddingByteCount, SeekOrigin.Current); // Skip padding bytes

            // Find the key's null terminator
            int i;
            for (i = 0; i < keyAndValueByteSize; i++)
            {
                if (keyAndValueBytes[i] == 0) break;
                Debug.Assert(i != keyAndValueByteSize); // Fail
            }


            var keySize = i; // Don't include null terminator.
            var key = Encoding.UTF8.GetString(keyAndValueBytes, keySize);
            var valueStart = keyAndValueBytes + i + 1; // Skip null terminator
            var valueSize = (int) (keyAndValueByteSize - keySize - 1);
            var value = new byte[valueSize];
            for (var v = 0; v < valueSize; v++) value[v] = valueStart[v];

            bytesRead = (int) (keyAndValueByteSize + paddingByteCount + sizeof(uint));
            return new KtxKeyValuePair(key, value);
        }

        private static unsafe T ReadStruct<T>(BinaryReader br)
        {
            var size = Unsafe.SizeOf<T>();
            var bytes = stackalloc byte[size];
            for (var i = 0; i < size; i++) bytes[i] = br.ReadByte();

            return Unsafe.Read<T>(bytes);
        }

        private static unsafe void ReadBytes(BinaryReader br, byte* destination, int count)
        {
            for (var i = 0; i < count; i++) destination[i] = br.ReadByte();
        }

        public static Texture LoadTexture(
            GraphicsDevice gd,
            ResourceFactory factory,
            byte[] bytes,
            PixelFormat format)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return LoadTexture(gd, factory, ms, format);
            }
        }


        public static Texture LoadTexture(
            GraphicsDevice gd,
            ResourceFactory factory,
            string assetPath,
            PixelFormat format)
        {
            using (var fs = File.OpenRead(assetPath))
            {
                return LoadTexture(gd, factory, fs, format);
            }
        }

        public static unsafe Texture LoadTexture(
            GraphicsDevice gd,
            ResourceFactory factory,
            Stream assetStream,
            PixelFormat format)
        {
            var ktxTex2D = Load(assetStream, false);

            var width = ktxTex2D.Header.PixelWidth;
            var height = ktxTex2D.Header.PixelHeight;
            if (height == 0) height = width;

            var arrayLayers = System.Math.Max(1, ktxTex2D.Header.NumberOfArrayElements);
            var mipLevels = System.Math.Max(1, ktxTex2D.Header.NumberOfMipmapLevels);

            var ret = factory.CreateTexture(TextureDescription.Texture2D(
                width, height, mipLevels, arrayLayers,
                format, TextureUsage.Sampled));

            var stagingTex = factory.CreateTexture(TextureDescription.Texture2D(
                width, height, mipLevels, arrayLayers,
                format, TextureUsage.Staging));

            // Copy texture data into staging buffer
            for (uint level = 0; level < mipLevels; level++)
            {
                var mipmap = ktxTex2D.Mipmaps[level];
                for (uint layer = 0; layer < arrayLayers; layer++)
                {
                    var ktxLayer = mipmap.ArrayElements[layer];
                    Debug.Assert(ktxLayer.Faces.Length == 1);
                    var pixelData = ktxLayer.Faces[0].Data;
                    fixed (byte* pixelDataPtr = &pixelData[0])
                    {
                        gd.UpdateTexture(stagingTex, (IntPtr) pixelDataPtr, (uint) pixelData.Length,
                            0, 0, 0, mipmap.Width, mipmap.Height, 1, level, layer);
                    }
                }
            }

            var copyCL = factory.CreateCommandList();
            copyCL.Begin();
            for (uint level = 0; level < mipLevels; level++)
            {
                var mipLevel = ktxTex2D.Mipmaps[level];
                for (uint layer = 0; layer < arrayLayers; layer++)
                    copyCL.CopyTexture(
                        stagingTex, 0, 0, 0, level, layer,
                        ret, 0, 0, 0, level, layer,
                        mipLevel.Width, mipLevel.Height, mipLevel.Depth,
                        1);
            }

            copyCL.End();
            gd.SubmitCommands(copyCL);

            gd.DisposeWhenIdle(copyCL);
            gd.DisposeWhenIdle(stagingTex);

            return ret;
        }
    }

    public class KtxKeyValuePair
    {
        public KtxKeyValuePair(string key, byte[] value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public byte[] Value { get; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct KtxHeader
    {
        public fixed byte Identifier[12];
        public readonly uint Endianness;
        public readonly uint GlType;
        public readonly uint GlTypeSize;
        public readonly uint GlFormat;
        public readonly uint GlInternalFormat;
        public readonly uint GlBaseInternalFormat;
        public readonly uint PixelWidth;
        private readonly uint _pixelHeight;
        public uint PixelHeight => System.Math.Max(1, _pixelHeight);
        public readonly uint PixelDepth;
        public readonly uint NumberOfArrayElements;
        public readonly uint NumberOfFaces;
        public readonly uint NumberOfMipmapLevels;
        public readonly uint BytesOfKeyValueData;
    }

    // for each mipmap_level in numberOfMipmapLevels
    public class KtxMipmapLevel
    {
        public KtxMipmapLevel(uint width, uint height, uint depth, uint totalSize, uint arraySliceSize,
            KtxArrayElement[] slices)
        {
            Width = width;
            Height = height;
            Depth = depth;
            TotalSize = totalSize;
            ArrayElementSize = arraySliceSize;
            ArrayElements = slices;
        }

        public uint Width { get; }
        public uint Height { get; }
        public uint Depth { get; }
        public uint TotalSize { get; }
        public uint ArrayElementSize { get; }
        public KtxArrayElement[] ArrayElements { get; }
    }

    // for each array_element in numberOfArrayElements
    public class KtxArrayElement
    {
        public KtxArrayElement(KtxFace[] faces)
        {
            Faces = faces;
        }

        public KtxFace[] Faces { get; }
    }

    // for each face in numberOfFaces
    public class KtxFace
    {
        public KtxFace(byte[] data)
        {
            Data = data;
        }

        public byte[] Data { get; }
    }
}