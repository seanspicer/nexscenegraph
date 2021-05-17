﻿//
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
using System.IO;

namespace Veldrid.SceneGraph.AssetPrimitives
{
    public class ProcessedTexture
    {
        public ProcessedTexture(
            PixelFormat format,
            TextureType type,
            uint width,
            uint height,
            uint depth,
            uint mipLevels,
            uint arrayLayers,
            byte[] textureData)
        {
            Format = format;
            Type = type;
            Width = width;
            Height = height;
            Depth = depth;
            MipLevels = mipLevels;
            ArrayLayers = arrayLayers;
            TextureData = textureData;
        }

        public PixelFormat Format { get; set; }
        public TextureType Type { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public uint Depth { get; set; }
        public uint MipLevels { get; set; }
        public uint ArrayLayers { get; set; }
        public byte[] TextureData { get; set; }

        public unsafe Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage)
        {
            var texture = rf.CreateTexture(new TextureDescription(
                Width, Height, Depth, MipLevels, ArrayLayers, Format, usage, Type));

            var staging = rf.CreateTexture(new TextureDescription(
                Width, Height, Depth, MipLevels, ArrayLayers, Format, TextureUsage.Staging, Type));

            ulong offset = 0;
            fixed (byte* texDataPtr = &TextureData[0])
            {
                for (uint level = 0; level < MipLevels; level++)
                {
                    var mipWidth = GetDimension(Width, level);
                    var mipHeight = GetDimension(Height, level);
                    var mipDepth = GetDimension(Depth, level);
                    var subresourceSize = mipWidth * mipHeight * mipDepth * GetFormatSize(Format);

                    for (uint layer = 0; layer < ArrayLayers; layer++)
                    {
                        gd.UpdateTexture(
                            staging, (IntPtr) (texDataPtr + offset), subresourceSize,
                            0, 0, 0, mipWidth, mipHeight, mipDepth,
                            level, layer);
                        offset += subresourceSize;
                    }
                }
            }

            var cl = rf.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(staging, texture);
            cl.End();
            gd.SubmitCommands(cl);

            return texture;
        }

        private uint GetFormatSize(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R8_G8_B8_A8_UNorm: return 4;
                case PixelFormat.BC3_UNorm: return 1;
                case PixelFormat.R8_UNorm: return 1;
                default: throw new NotImplementedException();
            }
        }

        public static uint GetDimension(uint largestLevelDimension, uint mipLevel)
        {
            var ret = largestLevelDimension;
            for (uint i = 0; i < mipLevel; i++) ret /= 2;

            return System.Math.Max(1, ret);
        }
    }

    public class ProcessedTextureDataSerializer : BinaryAssetSerializer<ProcessedTexture>
    {
        public override ProcessedTexture ReadT(BinaryReader reader)
        {
            return new ProcessedTexture(
                reader.ReadEnum<PixelFormat>(),
                reader.ReadEnum<TextureType>(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadByteArray());
        }

        public override void WriteT(BinaryWriter writer, ProcessedTexture ptd)
        {
            writer.WriteEnum(ptd.Format);
            writer.WriteEnum(ptd.Type);
            writer.Write(ptd.Width);
            writer.Write(ptd.Height);
            writer.Write(ptd.Depth);
            writer.Write(ptd.MipLevels);
            writer.Write(ptd.ArrayLayers);
            writer.WriteByteArray(ptd.TextureData);
        }
    }
}