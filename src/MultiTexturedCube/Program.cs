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
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Examples.Common;
using MultiTexturedCube.Shaders;
using ShaderGen;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Viewer;

namespace MultiTexturedCube
{
    public struct VertexPositionTexture : IPrimitiveElement
    {
        public const uint SizeInBytes = 36;

        [PositionSemantic] 
        public Vector3 Position;
        [TextureCoordinateSemantic]
        public Vector2 TexCoord;
        [ColorSemantic]
        public Vector4 Color;
        
        public VertexPositionTexture(Vector3 position, Vector2 texCoord, Vector4 color)
        {
            Position = position;
            TexCoord = texCoord;
            Color = color;
        }

        public Vector3 VertexPosition
        {
            get => Position;
            set => Position = value;
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper.Configure();
            
            var viewer = SimpleViewer.Create("Textured Cube Scene Graph");
            viewer.SetCameraManipulator(TrackballManipulator.Create());

            var root = Group.Create();
            
            var cube = CreateCube();
            
            root.AddChild(cube);

            viewer.SetSceneData(root);
            viewer.ViewAll();            
            viewer.Run();
        }

        static IGeode CreateCube()
        {
            var geometry = Geometry<VertexPositionTexture>.Create();

            var vertices = new VertexPositionTexture[]
            {
                // Top
                new VertexPositionTexture(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0), new Vector4(1, 0, 0, 1)),
                new VertexPositionTexture(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0), new Vector4(1, 0, 0, 1)),
                new VertexPositionTexture(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 1), new Vector4(1, 0, 0, 1)),
                new VertexPositionTexture(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 1), new Vector4(1, 0, 0, 1)),
                // Bottom                                                             
                new VertexPositionTexture(new Vector3(-0.5f,-0.5f, +0.5f),  new Vector2(0, 0), new Vector4(1, 1, 0, 1)),
                new VertexPositionTexture(new Vector3(+0.5f,-0.5f, +0.5f),  new Vector2(1, 0), new Vector4(1, 1, 0, 1)),
                new VertexPositionTexture(new Vector3(+0.5f,-0.5f, -0.5f),  new Vector2(1, 1), new Vector4(1, 1, 0, 1)),
                new VertexPositionTexture(new Vector3(-0.5f,-0.5f, -0.5f),  new Vector2(0, 1), new Vector4(1, 1, 0, 1)),
                // Left                                                               
                new VertexPositionTexture(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0), new Vector4(0, 1, 0, 1)),
                new VertexPositionTexture(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(1, 0), new Vector4(0, 1, 0, 1)),
                new VertexPositionTexture(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(1, 1), new Vector4(0, 1, 0, 1)),
                new VertexPositionTexture(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(0, 1), new Vector4(0, 1, 0, 1)),
                // Right                                                              
                new VertexPositionTexture(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(0, 0), new Vector4(0, 1, 1, 1)),
                new VertexPositionTexture(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0), new Vector4(0, 1, 1, 1)),
                new VertexPositionTexture(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(1, 1), new Vector4(0, 1, 1, 1)),
                new VertexPositionTexture(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(0, 1), new Vector4(0, 1, 1, 1)),
                // Back                                                               
                new VertexPositionTexture(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(0, 0), new Vector4(0, 0, 1, 1)),
                new VertexPositionTexture(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(1, 0), new Vector4(0, 0, 1, 1)),
                new VertexPositionTexture(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(1, 1), new Vector4(0, 0, 1, 1)),
                new VertexPositionTexture(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(0, 1), new Vector4(0, 0, 1, 1)),
                // Front                                                              
                new VertexPositionTexture(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 0), new Vector4(1, 0, 1, 1)),
                new VertexPositionTexture(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 0), new Vector4(1, 0, 1, 1)),
                new VertexPositionTexture(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(1, 1), new Vector4(1, 0, 1, 1)),
                new VertexPositionTexture(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(0, 1), new Vector4(1, 0, 1, 1)),
            };
            
            ushort[] indices =
            {
                0,1,2, 0,2,3,
                4,5,6, 4,6,7,
                8,9,10, 8,10,11,
                12,13,14, 12,14,15,
                16,17,18, 16,18,19,
                20,21,22, 20,22,23,
            };
            

            geometry.VertexData = vertices;
            geometry.IndexData = indices;

            geometry.VertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
                new VertexElementDescription("Texture", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float4));

            var pSet = DrawElements<VertexPositionTexture>.Create(
                geometry, 
                PrimitiveTopology.TriangleList, 
                (uint)geometry.IndexData.Length, 
                1, 
                0, 
                0, 
                0);
            
            geometry.PrimitiveSets.Add(pSet);

            geometry.PipelineState.VertexShaderDescription = new ShaderDescription(
                ShaderStages.Vertex,
                ShaderTools.LoadShaderBytes(DisplaySettings.Instance.GraphicsBackend,
                    typeof(Program).Assembly,
                    "MultiTexturedCubeShader", ShaderStages.Vertex), 
                "VS");
            
            geometry.PipelineState.FragmentShaderDescription = new ShaderDescription(
                ShaderStages.Fragment, 
                ShaderTools.LoadShaderBytes(DisplaySettings.Instance.GraphicsBackend,
                    typeof(Program).Assembly,
                    "MultiTexturedCubeShader", ShaderStages.Fragment),
                "FS");
            
            geometry.PipelineState.AddTexture(
                Texture2D.Create(Texture2D.ImageFormatType.Png,
                    ShaderTools.ReadEmbeddedAssetBytes(
                        "MultiTexturedCube.Textures.spnza_bricks_a_diff.png",
                        typeof(Program).Assembly),
                    1,
                    "SurfaceTexture", 
                    "SurfaceSampler")
            );
            
            geometry.PipelineState.AddTexture(
                Texture2D.Create(Texture2D.ImageFormatType.Png,
                    ShaderTools.ReadEmbeddedAssetBytes(
                        "MultiTexturedCube.Textures.tree.png",
                        typeof(Program).Assembly),
                    1,
                    "TreeTexture", 
                    "TreeSampler")
                );

            var geode = Geode.Create();
            geode.AddDrawable(geometry);
            return geode;
        }
    }
}