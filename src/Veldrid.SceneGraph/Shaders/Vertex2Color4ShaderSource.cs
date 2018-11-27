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
using System.Numerics;
using ShaderGen;

[assembly: ShaderSet("Vertex2Color4ShaderSource", "Veldrid.SceneGraph.Shaders.Vertex2Color4ShaderSource.VS", "Veldrid.SceneGraph.Shaders.Vertex2Color4ShaderSource.FS")]

namespace Veldrid.SceneGraph.Shaders
{
    #pragma warning disable 649
    
    internal class Vertex2Color4ShaderSource
    {
        public struct VertexInput
        {
            [PositionSemantic] public Vector2 Position;
            [ColorSemantic] public Vector4 Color;
        }
    
        public struct FragmentInput
        {
            [SystemPositionSemantic] public Vector4 Position;
            [ColorSemantic] public Vector4 Color;
        }

        [ResourceSet(0)]
        public Matrix4x4 Projection;
        [ResourceSet(0)]
        public Matrix4x4 View;
        [ResourceSet(1)]
        public Matrix4x4 Model;

        [VertexShader]
        public FragmentInput VS(VertexInput input)
        {
            FragmentInput output;
            output.Color = input.Color;
        
            output.Position = Vector4.Transform(
                Vector4.Transform(
                    Vector4.Transform(
                        new Vector4(input.Position.X, input.Position.Y, 0.0f, 1f),
                        Model),
                    View),
                Projection);
    
            return output;
        }

        [FragmentShader]
        public Vector4 FS(FragmentInput input)
        {
            return input.Color;
        }
    }
    #pragma warning restore 649
}