//
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
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.VertexTypes;
using Veldrid.SceneGraph.Viewer;

namespace TexturedCube
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Bootstrapper.Configure();

            var viewer = SimpleViewer.Create("Textured Cube Scene Graph");
            viewer.SetCameraManipulator(TrackballManipulator.Create());

            var root = Group.Create();

            var scaleXform = MatrixTransform.Create(Matrix4x4.CreateScale(0.75f));

            var cube = CreateCube();
            scaleXform.AddChild(cube);

            root.AddChild(scaleXform);

            viewer.SetSceneData(root);
            viewer.ViewAll();
            viewer.Run();
        }

        private static IGeode CreateCube()
        {
            var geometry = Geometry<Position3TexCoord2>.Create();

            var vertices = new[]
            {
                // Top
                new Position3TexCoord2(new Vector3(-1.0f, +1.0f, -1.0f), new Vector2(0, 0)),
                new Position3TexCoord2(new Vector3(+1.0f, +1.0f, -1.0f), new Vector2(1, 0)),
                new Position3TexCoord2(new Vector3(+1.0f, +1.0f, +1.0f), new Vector2(1, 1)),
                new Position3TexCoord2(new Vector3(-1.0f, +1.0f, +1.0f), new Vector2(0, 1)),
                // Bottom                                                             
                new Position3TexCoord2(new Vector3(-1.0f, -1.0f, +1.0f), new Vector2(0, 0)),
                new Position3TexCoord2(new Vector3(+1.0f, -1.0f, +1.0f), new Vector2(1, 0)),
                new Position3TexCoord2(new Vector3(+1.0f, -1.0f, -1.0f), new Vector2(1, 1)),
                new Position3TexCoord2(new Vector3(-1.0f, -1.0f, -1.0f), new Vector2(0, 1)),
                // Left                                                               
                new Position3TexCoord2(new Vector3(-1.0f, +1.0f, -1.0f), new Vector2(0, 0)),
                new Position3TexCoord2(new Vector3(-1.0f, +1.0f, +1.0f), new Vector2(1, 0)),
                new Position3TexCoord2(new Vector3(-1.0f, -1.0f, +1.0f), new Vector2(1, 1)),
                new Position3TexCoord2(new Vector3(-1.0f, -1.0f, -1.0f), new Vector2(0, 1)),
                // Right                                                              
                new Position3TexCoord2(new Vector3(+1.0f, +1.0f, +1.0f), new Vector2(0, 0)),
                new Position3TexCoord2(new Vector3(+1.0f, +1.0f, -1.0f), new Vector2(1, 0)),
                new Position3TexCoord2(new Vector3(+1.0f, -1.0f, -1.0f), new Vector2(1, 1)),
                new Position3TexCoord2(new Vector3(+1.0f, -1.0f, +1.0f), new Vector2(0, 1)),
                // Back                                                               
                new Position3TexCoord2(new Vector3(+1.0f, +1.0f, -1.0f), new Vector2(0, 0)),
                new Position3TexCoord2(new Vector3(-1.0f, +1.0f, -1.0f), new Vector2(1, 0)),
                new Position3TexCoord2(new Vector3(-1.0f, -1.0f, -1.0f), new Vector2(1, 1)),
                new Position3TexCoord2(new Vector3(+1.0f, -1.0f, -1.0f), new Vector2(0, 1)),
                // Front                                                              
                new Position3TexCoord2(new Vector3(-1.0f, +1.0f, +1.0f), new Vector2(0, 0)),
                new Position3TexCoord2(new Vector3(+1.0f, +1.0f, +1.0f), new Vector2(1, 0)),
                new Position3TexCoord2(new Vector3(+1.0f, -1.0f, +1.0f), new Vector2(1, 1)),
                new Position3TexCoord2(new Vector3(-1.0f, -1.0f, +1.0f), new Vector2(0, 1))
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
                Position3TexCoord2.VertexLayoutDescription
            };

            var pSet = DrawElements<Position3TexCoord2>.Create(
                geometry,
                PrimitiveTopology.TriangleList,
                (uint) geometry.IndexData.Length,
                1,
                0,
                0,
                0);

            geometry.PrimitiveSets.Add(pSet);

            geometry.PipelineState.ShaderSet = Texture2DShader.Instance.ShaderSet;

            geometry.PipelineState.AddTexture(
                Texture2D.Create(Texture2D.ImageFormatType.Png,
                    ShaderTools.ReadEmbeddedAssetBytes(
                        "TexturedCube.Textures.spnza_bricks_a_diff.png",
                        typeof(Program).Assembly),
                    1,
                    "SurfaceTexture",
                    "SurfaceSampler"));

            var geode = Geode.Create();
            geode.AddDrawable(geometry);

            return geode;
        }
    }
}