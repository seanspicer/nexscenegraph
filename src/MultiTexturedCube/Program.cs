﻿//
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

using System.Collections.Generic;
using System.Numerics;
using Examples.Common;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Shaders;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Viewer;

namespace MultiTexturedCube
{
    public struct VertexPositionTexture : IPrimitiveElement
    {
        public const uint SizeInBytes = 36;

        public Vector3 Position;
        public Vector2 TexCoord;
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

    internal class Program
    {
        private static void Main(string[] args)
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

        private static IGeode CreateCube()
        {
            var geometry = Geometry<VertexPositionTexture>.Create();

            var vertices = new[]
            {
                // Top
                new VertexPositionTexture(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0), new Vector4(1, 0, 0, 1)),
                new VertexPositionTexture(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0), new Vector4(1, 0, 0, 1)),
                new VertexPositionTexture(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 1), new Vector4(1, 0, 0, 1)),
                new VertexPositionTexture(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 1), new Vector4(1, 0, 0, 1)),
                // Bottom                                                             
                new VertexPositionTexture(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(0, 0), new Vector4(1, 1, 0, 1)),
                new VertexPositionTexture(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(1, 0), new Vector4(1, 1, 0, 1)),
                new VertexPositionTexture(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(1, 1), new Vector4(1, 1, 0, 1)),
                new VertexPositionTexture(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(0, 1), new Vector4(1, 1, 0, 1)),
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
                new VertexPositionTexture(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(0, 1), new Vector4(1, 0, 1, 1))
            };

            uint[] indices =
            {
                0, 1, 2, 0, 2, 3,
                4, 5, 6, 4, 6, 7,
                8, 9, 10, 8, 10, 11,
                12, 13, 14, 12, 14, 15,
                16, 17, 18, 16, 18, 19,
                20, 21, 22, 20, 22, 23
            };


            geometry.VertexData = vertices;
            geometry.IndexData = indices;

            geometry.VertexLayouts = new List<VertexLayoutDescription>
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate,
                        VertexElementFormat.Float3),
                    new VertexElementDescription("Texture", VertexElementSemantic.TextureCoordinate,
                        VertexElementFormat.Float2),
                    new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate,
                        VertexElementFormat.Float4))
            };

            var pSet = DrawElements<VertexPositionTexture>.Create(
                geometry,
                PrimitiveTopology.TriangleList,
                (uint) geometry.IndexData.Length,
                1,
                0,
                0,
                0);

            geometry.PrimitiveSets.Add(pSet);

            var vertexShaderDescription = new ShaderDescription(
                ShaderStages.Vertex,
                ShaderTools.ReadEmbeddedAssetBytes(
                    @"MultiTexturedCube.Assets.Shaders.MultiTexturedCubeShader-vertex.glsl",
                    typeof(Program).Assembly),
                "main", true);

            var fragmentShaderDescription = new ShaderDescription(
                ShaderStages.Fragment,
                ShaderTools.ReadEmbeddedAssetBytes(
                    @"MultiTexturedCube.Assets.Shaders.MultiTexturedCubeShader-fragment.glsl",
                    typeof(Program).Assembly),
                "main", true);

            geometry.PipelineState.ShaderSet = ShaderSet.Create("MultiTexturedCubeShader", vertexShaderDescription,
                fragmentShaderDescription);


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