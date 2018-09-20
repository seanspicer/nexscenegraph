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
using System.Numerics;
using TexturedCube.Shaders;
using ShaderGen;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Viewer;

namespace TexturedCube
{
    public struct VertexPositionTexture : IPrimitiveElement
    {
        public const uint SizeInBytes = 20;

        [PositionSemantic] 
        public Vector3 Position;
        [ColorSemantic]
        public Vector2 TexCoord;
        
        public VertexPositionTexture(Vector3 position, Vector2 texCoord)
        {
            Position = position;
            TexCoord = texCoord;
        }

        public Vector3 VertexPosition
        {
            get => Position;
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var asm = typeof(Program).Assembly;
            
            var allNames = asm.GetManifestResourceNames();
            
            var viewer = new SimpleViewer("Textured Cube Scene Graph");
            viewer.View.CameraManipulator = new TrackballManipulator();

            var root = new Group();
            
            var scale_xform = new MatrixTransform();
            scale_xform.Matrix = Matrix4x4.CreateScale(0.75f);
 
            var cube = CreateCube();
            scale_xform.AddChild(cube);
            
            root.AddChild(scale_xform);

            viewer.SceneData = root;

            viewer.Run();
        }

        static Drawable CreateCube()
        {
            var geometry = new Geometry<VertexPositionTexture>();

            var vertices = new VertexPositionTexture[]
            {
                // Top
                new VertexPositionTexture(new Vector3(-1.0f, +1.0f, -1.0f), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(+1.0f, +1.0f, -1.0f), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(+1.0f, +1.0f, +1.0f), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(-1.0f, +1.0f, +1.0f), new Vector2(0, 1)),
                // Bottom                                                             
                new VertexPositionTexture(new Vector3(-1.0f,-1.0f, +1.0f),  new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(+1.0f,-1.0f, +1.0f),  new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(+1.0f,-1.0f, -1.0f),  new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(-1.0f,-1.0f, -1.0f),  new Vector2(0, 1)),
                // Left                                                               
                new VertexPositionTexture(new Vector3(-1.0f, +1.0f, -1.0f), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(-1.0f, +1.0f, +1.0f), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(-1.0f, -1.0f, +1.0f), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(-1.0f, -1.0f, -1.0f), new Vector2(0, 1)),
                // Right                                                              
                new VertexPositionTexture(new Vector3(+1.0f, +1.0f, +1.0f), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(+1.0f, +1.0f, -1.0f), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(+1.0f, -1.0f, -1.0f), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(+1.0f, -1.0f, +1.0f), new Vector2(0, 1)),
                // Back                                                               
                new VertexPositionTexture(new Vector3(+1.0f, +1.0f, -1.0f), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(-1.0f, +1.0f, -1.0f), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(-1.0f, -1.0f, -1.0f), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(+1.0f, -1.0f, -1.0f), new Vector2(0, 1)),
                // Front                                                              
                new VertexPositionTexture(new Vector3(-1.0f, +1.0f, +1.0f), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(+1.0f, +1.0f, +1.0f), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(+1.0f, -1.0f, +1.0f), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(-1.0f, -1.0f, +1.0f), new Vector2(0, 1)),
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
                new VertexElementDescription("Texture", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));
            
            geometry.PrimitiveTopology = PrimitiveTopology.TriangleList;

            geometry.PipelineState.VertexShader = ShaderTools.LoadShaderBytes(GraphicsBackend.Vulkan,
                typeof(Program).Assembly,
                "TexturedCubeShader", ShaderStages.Vertex);
            geometry.PipelineState.VertexShaderEntryPoint = "VS";

            geometry.PipelineState.FragmentShader = ShaderTools.LoadShaderBytes(GraphicsBackend.Vulkan,
                typeof(Program).Assembly,
                "TexturedCubeShader", ShaderStages.Fragment);
            geometry.PipelineState.FragmentShaderEntryPoint = "FS";

            geometry.PipelineState.TextureList.Add(
                new Texture2D(Texture2D.ImageFormatType.Png,
                ShaderTools.ReadEmbeddedAssetBytes(
                    "TexturedCube.Textures.spnza_bricks_a_diff.png",
                    typeof(Program).Assembly),
                1,
                "SurfaceTexture", 
                "SurfaceSampler"));
                          
            return geometry;
        }
    }
}