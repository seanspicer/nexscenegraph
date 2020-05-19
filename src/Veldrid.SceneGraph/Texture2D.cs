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
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using Veldrid.SceneGraph.AssetPrimitives;
using Veldrid.SceneGraph.AssetProcessor;

namespace Veldrid.SceneGraph
{
    public interface ITexture2D
    {
        ProcessedTexture ProcessedTexture { get; }
        uint ResourceSetNo { get; set; }
        string TextureName { get; set; }
        string SamplerName { get; set; }
    }
    
    public class Texture2D : ITexture2D
    {
        public enum ImageFormatType
        {
            Jpeg, 
            Png
        };

        private ImageFormatType ImageFormat { get; set; }
        private byte[] ImageBytes { get; set; } = null;
        
        public ProcessedTexture ProcessedTexture { get; private set; } = null;

        public uint ResourceSetNo { get; set; } = 1;
        public string TextureName { get; set; } = string.Empty;
        public string SamplerName { get; set; } = string.Empty;

        public static ITexture2D Create(
            ImageFormatType imageFormat, 
            byte[] imageBytes, 
            uint resourceSetNo,
            string textureName, 
            string samplerName)
        {
            return new Texture2D(imageFormat, imageBytes, resourceSetNo, textureName, samplerName);
        }

        public static ITexture2D Create(
            ProcessedTexture processedTexture, 
            uint resourceSetNo, 
            string textureName,
            string samplerName)
        {
            return new Texture2D(processedTexture, resourceSetNo, textureName, samplerName);
        }
        
        private Texture2D(ImageFormatType imageFormat, byte[] imageBytes, uint resourceSetNo, string textureName, string samplerName)
        {
            if (null == textureName || null == samplerName)
            {
                throw new ArgumentException("Must provide valid texture and sampler name");
            }

            ResourceSetNo = resourceSetNo;
            TextureName = textureName;
            SamplerName = samplerName;
            ImageFormat = imageFormat;
            ImageBytes = imageBytes;
            
            if (null != ImageBytes)
            {
                var texProcessor = new ImageSharpProcessor();
                using (var stream = new MemoryStream(ImageBytes))
                {
                    switch (ImageFormat)
                    {
                        case ImageFormatType.Jpeg:
                            ProcessedTexture = texProcessor.ProcessT(stream, "jpg");
                            break;
                        case ImageFormatType.Png:
                            ProcessedTexture = texProcessor.ProcessT(stream, "png");
                            break;
                        default:
                            throw new InvalidEnumArgumentException("Unknown type");
                    }
                }
            }
        }

        private Texture2D(ProcessedTexture processedTexture, uint resourceSetNo, string textureName, string samplerName)
        {
            ResourceSetNo = resourceSetNo;
            TextureName = textureName;
            SamplerName = samplerName;
            ProcessedTexture = processedTexture;
        }
    }
}