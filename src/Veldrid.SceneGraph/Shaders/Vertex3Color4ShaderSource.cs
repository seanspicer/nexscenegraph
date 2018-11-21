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
using System.Numerics;
using ShaderGen;

[assembly: ShaderSet("Vertex3Color4ShaderSource", "Veldrid.SceneGraph.Shaders.Vertex3Color4ShaderSource.VS", "Veldrid.SceneGraph.Shaders.Vertex3Color4ShaderSource.FS")]

namespace Veldrid.SceneGraph.Shaders
{
    #pragma warning disable 649
    
    internal class Vertex3Color4ShaderSource
    {
        public struct VertexInput
        {
            [PositionSemantic] public Vector3 Position;
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
                        new Vector4(input.Position, 1f),
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