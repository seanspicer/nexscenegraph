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
    
    public struct VertexPositionColor : IPrimitiveElement
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
        
        public Vector3 VertexPosition
        {
            get => new Vector3(Position, 0.0f);
            set => Position = new Vector2(value.X, value.Y);
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

            var root = new Group();
            
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
            
            var pSet = new DrawElements<VertexPositionColor>(
                geometry, 
                PrimitiveTopology.TriangleStrip,
                (uint)quadIndices.Length, 
                1, 
                0, 
                0, 
                0);
            
            geometry.PrimitiveSets.Add(pSet);
            geometry.PrimitiveSets.Add(pSet);

            geometry.VertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float4));

            geometry.PipelineState.VertexShaderDescription = new ShaderDescription(
                ShaderStages.Vertex,
                ShaderTools.LoadShaderBytes(DisplaySettings.Instance.GraphicsBackend,
                    typeof(Program).Assembly,
                    "HelloShaders", ShaderStages.Vertex), 
                "VS");
            
            geometry.PipelineState.FragmentShaderDescription = new ShaderDescription(
                ShaderStages.Fragment, 
                ShaderTools.LoadShaderBytes(DisplaySettings.Instance.GraphicsBackend,
                    typeof(Program).Assembly,
                    "HelloShaders", ShaderStages.Fragment),
                "FS");
                        
            var geode = new Geode();
            geode.Drawables.Add(geometry);
            
            root.AddChild(geode);

            viewer.SceneData = root;

            viewer.Run();

        }
    }
}