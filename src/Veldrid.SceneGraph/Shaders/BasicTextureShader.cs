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

using ShaderGen;
using System.Numerics;
using static ShaderGen.ShaderBuiltins;

[assembly: ShaderSet("BasicTextureShader", "Veldrid.SceneGraph.Shaders.BasicTextureShader.VS", "Veldrid.SceneGraph.Shaders.BasicTextureShader.FS")]

namespace Veldrid.SceneGraph.Shaders
{
    
    #pragma warning disable 649
    
    internal class BasicTextureShader
    {
        [ResourceSet(0)]
        public Matrix4x4 Projection;

        [ResourceSet(0)]
        public Matrix4x4 View;

        [ResourceSet(1)]
        public Matrix4x4 Model;
        [ResourceSet(1)]
        public Texture2DResource SurfaceTexture;
        [ResourceSet(1)]
        public SamplerResource SurfaceSampler;

        [VertexShader]
        public FragmentInput VS(VertexInput input)
        {
            FragmentInput output;
            Vector4 worldPosition = Mul(Model, new Vector4(input.Position, 1));
            Vector4 viewPosition = Mul(View, worldPosition);
            Vector4 clipPosition = Mul(Projection, viewPosition);
            output.SystemPosition = clipPosition;
            output.TexCoords = input.TexCoords;

            return output;
        }

        [FragmentShader]
        public Vector4 FS(FragmentInput input)
        {
            return Sample(SurfaceTexture, SurfaceSampler, input.TexCoords);
        }
        
        private static Vector4 Over(Vector4 a, Vector4 b)
        {
            var result = new Vector4();

            result = a*a.W + b*b.W * (1 - a.W) / (a.W + b.W * 1 - a.W);
            
            return result;
        }

        public struct VertexInput
        {
            [PositionSemantic] public Vector3 Position;
            [TextureCoordinateSemantic] public Vector2 TexCoords;
        }

        public struct FragmentInput
        {
            [SystemPositionSemantic] public Vector4 SystemPosition;
            [TextureCoordinateSemantic] public Vector2 TexCoords;
        }
    }
    #pragma warning restore 649
}
