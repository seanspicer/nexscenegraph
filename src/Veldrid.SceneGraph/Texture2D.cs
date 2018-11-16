//
// Copyright (c) 2018 Sean Spicer
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using AssetPrimitives;
using AssetProcessor;

namespace Veldrid.SceneGraph
{
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