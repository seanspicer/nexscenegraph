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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Veldrid.SPIRV;

namespace Veldrid.SceneGraph.Util
{
    public static class ShaderTools
    {
        public static (Shader[], SpirvReflection) LoadEmbeddedShaderSet(
            Assembly assembly,
            ResourceFactory factory,
            string name)
        {
            string extension;
            switch (factory.BackendType)
            {
                case GraphicsBackend.Direct3D11:
                    extension = "hlsl";
                    break;
                case GraphicsBackend.Vulkan:
                    extension = "spv";
                    break;
                case GraphicsBackend.OpenGL:
                    extension = "glsl";
                    break;
                case GraphicsBackend.Metal:
                    extension = "metal";
                    break;
                case GraphicsBackend.OpenGLES:
                case GraphicsBackend.WebGL:
                    extension = "essl";
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported GraphicsBackend: {factory.BackendType}");
            }

            byte[] vsBytes = ReadEmbeddedBytes(assembly, $"{name}_Vertex.{extension}");
            byte[] fsBytes = ReadEmbeddedBytes(assembly, $"{name}_Fragment.{extension}");

            SpirvReflection reflection;
            using (Stream embeddedStream = assembly.GetManifestResourceStream($"{name}_ReflectionInfo.json"))
            {
                reflection = SpirvReflection.LoadFromJson(embeddedStream);
            }

            return (new[]
            {
                factory.CreateShader(new ShaderDescription(ShaderStages.Vertex, vsBytes, "main")),
                factory.CreateShader(new ShaderDescription(ShaderStages.Fragment, fsBytes, "main")),
            }, reflection);
        }
        
        private static byte[] ReadEmbeddedBytes(Assembly assembly, string name)
        {
            var names = assembly.GetManifestResourceNames();
            using (Stream s = assembly.GetManifestResourceStream(name))
            {
                byte[] bytes = new byte[s.Length];
                s.Read(bytes, 0, (int)s.Length);
                return bytes;
            }
        }
        
        public static Stream OpenEmbeddedAssetStream(string name, Assembly assembly)
        {
            var allNames = assembly.GetManifestResourceNames();
            var stream = assembly.GetManifestResourceStream(name);
            if (stream == null) throw new InvalidOperationException("No embedded asset with the name " + name);
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
            using (var stream = OpenEmbeddedAssetStream(name, assembly))
            {
                var bytes = new byte[stream.Length];
                using (var ms = new MemoryStream(bytes))
                {
                    stream.CopyTo(ms);
                    return bytes;
                }
            }
        }

        private static string GetExtension(GraphicsBackend backendType)
        {
            var isMacOS = RuntimeInformation.OSDescription.Contains("Darwin");

            return backendType == GraphicsBackend.Direct3D11
                ? "hlsl.bytes"
                : backendType == GraphicsBackend.Vulkan
                    ? "450.glsl.spv"
                    : backendType == GraphicsBackend.Metal
                        ? isMacOS ? "metal" : "ios.metallib"
                        : backendType == GraphicsBackend.OpenGL
                            ? "330.glsl"
                            : "300.glsles";
        }

        public static byte[] LoadBytecode(GraphicsBackend backend, string setName, ShaderStages stage)
        {
            var stageExt = stage == ShaderStages.Vertex ? "vert" : "frag";
            var name = setName + "." + stageExt;

            if (backend == GraphicsBackend.Vulkan || backend == GraphicsBackend.Direct3D11)
            {
                var bytecodeExtension = GetBytecodeExtension(backend);
                var bytecodePath = AssetHelper.GetPath(Path.Combine("Shaders", name + bytecodeExtension));
                if (File.Exists(bytecodePath)) return File.ReadAllBytes(bytecodePath);
            }

            var extension = GetSourceExtension(backend);
            var path = AssetHelper.GetPath(Path.Combine("Shaders.Generated", name + extension));
            return File.ReadAllBytes(path);
        }

        private static string GetBytecodeExtension(GraphicsBackend backend)
        {
            switch (backend)
            {
                case GraphicsBackend.Direct3D11: return ".hlsl.bytes";
                case GraphicsBackend.Vulkan: return ".spv";
                case GraphicsBackend.OpenGL:
                    throw new InvalidOperationException("OpenGL and OpenGLES do not support shader bytecode.");
                default: throw new InvalidOperationException("Invalid Graphics backend: " + backend);
            }
        }

        private static string GetSourceExtension(GraphicsBackend backend)
        {
            switch (backend)
            {
                case GraphicsBackend.Direct3D11: return ".hlsl";
                case GraphicsBackend.Vulkan: return ".450.glsl";
                case GraphicsBackend.OpenGL:
                    return ".330.glsl";
                case GraphicsBackend.OpenGLES:
                    return ".300.glsles";
                case GraphicsBackend.Metal:
                    return ".metallib";
                default: throw new InvalidOperationException("Invalid Graphics backend: " + backend);
            }
        }
    }
}