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
using System.Numerics;
using ShaderGen;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Viewer;

namespace HelloNsg
{
    
    public struct VertexPositionColor
    {
        public const uint SizeInBytes = 24;
        
        [PositionSemantic]
        public Vector2 Position;
        [ColorSemantic]
        public Vector4 Color;
        
        public VertexPositionColor(Vector2 position, Vector4 color)
        {
            Position = position;
            Color = color;
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var asm = typeof(Program).Assembly;
            
            var allNames = asm.GetManifestResourceNames();
            
            var viewer = new SimpleViewer("Hello Veldrid Scene Graph");
            viewer.View.CameraManipulator = new TrackballManipulator();

            var root = new Node();
            
            var geometry = new Geometry<VertexPositionColor>();
            
            VertexPositionColor[] quadVertices =
            {
                new VertexPositionColor(new Vector2(-.75f, .75f),  new Vector4(1.0f, 0.0f, 0.0f, 1.0f)),
                new VertexPositionColor(new Vector2(.75f, .75f),   new Vector4(0.0f, 1.0f, 0.0f, 1.0f)),
                new VertexPositionColor(new Vector2(-.75f, -.75f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new VertexPositionColor(new Vector2(.75f, -.75f),  new Vector4(1.0f, 1.0f, 0.0f, 1.0f))
            };

            geometry.VertexData = quadVertices;
            
            ushort[] quadIndices = { 0, 1, 2, 3 };
            geometry.IndexData = quadIndices;

            geometry.VertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float4));
            
            geometry.Topology = PrimitiveTopolgy.TriangleStrip;

            geometry.VertexShader = ShaderTools.LoadShaderBytes(GraphicsBackend.Vulkan,
                typeof(Program).Assembly,
                "HelloShaders", ShaderStages.Vertex);
            geometry.VertexShaderEntryPoint = "VS";

            geometry.FragmentShader = ShaderTools.LoadShaderBytes(GraphicsBackend.Vulkan,
                typeof(Program).Assembly,
                "HelloShaders", ShaderStages.Fragment);
            geometry.FragmentShaderEntryPoint = "FS";
            
            //var vsPath = Path.Combine(System.AppContext.BaseDirectory, "Shaders", "Vertex.spv");
            //geometry.VertexShader = File.ReadAllBytes(vsPath);
            
            //var fsPath = Path.Combine(System.AppContext.BaseDirectory, "Shaders", "Fragment.spv");
            //geometry.FragmentShader = File.ReadAllBytes(fsPath);
            
            root.Add(geometry);

            viewer.SceneData = root;

            viewer.Run();

        }
    }
}