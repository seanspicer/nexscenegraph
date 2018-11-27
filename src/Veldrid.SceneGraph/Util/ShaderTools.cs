//
// Copyright 2018 Sean Spicer 
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
using System.Reflection;
using System.Runtime.InteropServices;

namespace Veldrid.SceneGraph.Util
{
    public static class ShaderTools
    {
        public static Stream OpenEmbeddedAssetStream(string name, Assembly assembly)
        {
            var allNames = assembly.GetManifestResourceNames();
            var stream = assembly.GetManifestResourceStream(name);
            if (stream == null)
            {
                throw new InvalidOperationException("No embedded asset with the name " + name);
            }
            return stream;
        }

//        public static Shader LoadShader(ResourceFactory factory, string set, ShaderStages stage, string entryPoint)
//        {
//            var name = $"{set}-{stage.ToString().ToLower()}.{GetExtension(factory.BackendType)}";
//            return factory.CreateShader(new ShaderDescription(stage, ReadEmbeddedAssetBytes(name), entryPoint));
//        }
        
        public static byte[] LoadShaderBytes(GraphicsBackend backend, Assembly assembly, string set, ShaderStages stage)
        {
            var allNames = assembly.GetManifestResourceNames();
            var name = $"{set}-{stage.ToString().ToLower()}.{GetExtension(backend)}";
            return ReadEmbeddedAssetBytes(name, assembly);
        }
        
        public static byte[] ReadEmbeddedAssetBytes(string name, Assembly assembly)
        {
            using (Stream stream = OpenEmbeddedAssetStream(name, assembly))
            {
                var bytes = new byte[stream.Length];
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    stream.CopyTo(ms);
                    return bytes;
                }
            }
        }

        private static string GetExtension(GraphicsBackend backendType)
        {
			bool isMacOS = RuntimeInformation.OSDescription.Contains("Darwin");

            return (backendType == GraphicsBackend.Direct3D11)
                ? "hlsl.bytes"
                : (backendType == GraphicsBackend.Vulkan)
                    ? "450.glsl.spv"
                    : (backendType == GraphicsBackend.Metal)
					    ? isMacOS ? "metal" : "ios.metallib"
                        : (backendType == GraphicsBackend.OpenGL)
                            ? "330.glsl"
                            : "300.glsles";
        }

    }
}