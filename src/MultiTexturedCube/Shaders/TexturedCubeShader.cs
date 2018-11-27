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

[assembly: ShaderSet("MultiTexturedCubeShader", "MultiTexturedCube.Shaders.MultiTexturedCubeShader.VS", "MultiTexturedCube.Shaders.MultiTexturedCubeShader.FS")]

namespace MultiTexturedCube.Shaders
{
    public class MultiTexturedCubeShader
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
        [ResourceSet(1)]
        public Texture2DResource TreeTexture;
        [ResourceSet(1)]
        public SamplerResource TreeSampler;

        [VertexShader]
        public FragmentInput VS(VertexInput input)
        {
            FragmentInput output;
            Vector4 worldPosition = Mul(Model, new Vector4(input.Position, 1));
            Vector4 viewPosition = Mul(View, worldPosition);
            Vector4 clipPosition = Mul(Projection, viewPosition);
            output.SystemPosition = clipPosition;
            output.TexCoords = input.TexCoords;
            output.Color = input.Color;

            return output;
        }

        [FragmentShader]
        public Vector4 FS(FragmentInput input)
        {
            var brickSample = Sample(SurfaceTexture, SurfaceSampler, input.TexCoords);
            var treeSample = Sample(TreeTexture, TreeSampler, input.TexCoords);

            brickSample.W = 0.8f;
            
            var bg = Over(brickSample, input.Color);
            
            return Over(treeSample, bg);
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
            [ColorSemantic] public Vector4 Color;
        }

        public struct FragmentInput
        {
            [SystemPositionSemantic] public Vector4 SystemPosition;
            [TextureCoordinateSemantic] public Vector2 TexCoords;
            [ColorSemantic] public Vector4 Color;
        }
    }
}
