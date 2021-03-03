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

using System.Reflection;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Shaders.Standard
{
    public class StandardShaderBase
    {
        internal StandardShaderBase(string name, string vertexShader, string fragmentShader)
        {
            var asm = Assembly.GetAssembly(GetType());

            const string basePath = @"Veldrid.SceneGraph.Assets.Shaders";
            var vertexShaderPath = $"{basePath}.{vertexShader}";
            var fragmentShaderPath = $"{basePath}.{fragmentShader}";

            var vertexShaderDescription = new ShaderDescription(
                ShaderStages.Vertex,
                ShaderTools.ReadEmbeddedAssetBytes(vertexShaderPath, asm),
                "main", true);

            var fragmentShaderDescription = new ShaderDescription(
                ShaderStages.Fragment,
                ShaderTools.ReadEmbeddedAssetBytes(fragmentShaderPath, asm),
                "main", true);

            ShaderSet = Shaders.ShaderSet.Create(name, vertexShaderDescription, fragmentShaderDescription);
        }

        public IShaderSet ShaderSet { get; }
    }
}