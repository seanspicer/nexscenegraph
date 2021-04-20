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
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Viewer;

namespace HelloNsg
{
    public struct VertexPositionColor : IPrimitiveElement
    {
        public const uint SizeInBytes = 24;

        public Vector2 Position;
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

    internal class Program
    {
        private static void Main(string[] args)
        {
            Bootstrapper.Configure();

            var viewer = SimpleViewer.Create("Hello Veldrid Scene Graph", TextureSampleCount.Count32);
            var trackball = TrackballManipulator.Create();
            trackball.SetHomePosition(-Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);
            viewer.SetCameraManipulator(trackball);
            viewer.SetBackgroundColor(RgbaFloat.Black);

            var root = Group.Create();

            var geometry = Geometry<VertexPositionColor>.Create();

            VertexPositionColor[] quadVertices =
            {
                new VertexPositionColor(new Vector2(-.75f, .75f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)),
                new VertexPositionColor(new Vector2(.75f, .75f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f)),
                new VertexPositionColor(new Vector2(-.75f, -.75f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new VertexPositionColor(new Vector2(.75f, -.75f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f))
            };

            geometry.VertexData = quadVertices;

            uint[] quadIndices = {0, 1, 2, 3};
            geometry.IndexData = quadIndices;

            geometry.VertexLayouts = new List<VertexLayoutDescription>
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate,
                        VertexElementFormat.Float2),
                    new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate,
                        VertexElementFormat.Float4))
            };

            var pSet = DrawElements<VertexPositionColor>.Create(
                geometry,
                PrimitiveTopology.TriangleStrip,
                (uint) geometry.IndexData.Length,
                1,
                0,
                0,
                0);

            geometry.PrimitiveSets.Add(pSet);

            geometry.PipelineState.ShaderSet = Vertex2Color4Shader.Instance.ShaderSet;

            var geode = Geode.Create();
            geode.AddDrawable(geometry);

            root.AddChild(geode);

            viewer.SetSceneData(root);

            viewer.ViewAll();

            viewer.Run();
        }
    }
}