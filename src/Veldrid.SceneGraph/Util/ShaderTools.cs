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