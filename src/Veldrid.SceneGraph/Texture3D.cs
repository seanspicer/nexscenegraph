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
using System.ComponentModel;
using System.IO;
using Veldrid.SceneGraph.AssetPrimitives;
using Veldrid.SceneGraph.AssetProcessor;

namespace Veldrid.SceneGraph
{
    public interface ITexture3D : ITexture2D {}
    public class Texture3D : ITexture3D
    {
        
        private Texture3D(ProcessedTexture processedTexture, uint resourceSetNo, string textureName, string samplerName)
        {
            ResourceSetNo = resourceSetNo;
            TextureName = textureName;
            SamplerName = samplerName;
            ProcessedTexture = processedTexture;
        }
        
        private byte[] ImageBytes { get; }

        public ProcessedTexture ProcessedTexture { get; }

        public uint ResourceSetNo { get; set; } = 1;
        public string TextureName { get; set; } = string.Empty;
        public string SamplerName { get; set; } = string.Empty;
        
        public static ITexture3D Create(
            ProcessedTexture processedTexture,
            uint resourceSetNo,
            string textureName,
            string samplerName)
        {
            return new Texture3D(processedTexture, resourceSetNo, textureName, samplerName);
        }
    }
}