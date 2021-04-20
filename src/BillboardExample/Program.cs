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

namespace BillboardExample
{
    public struct VertexPositionColor : IPrimitiveElement
    {
        public const uint SizeInBytes = 28;

        public Vector3 Position;
        public Vector4 Color;

        public VertexPositionColor(Vector3 position, Vector4 color)
        {
            Position = position;
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

            var viewer = SimpleViewer.Create("Hello Veldrid Scene Graph");
            viewer.SetCameraManipulator(TrackballManipulator.Create());

            var root = Group.Create();

            var geometry = Geometry<VertexPositionColor>.Create();

            VertexPositionColor[] quadVertices =
            {
                new VertexPositionColor(new Vector3(-.75f, .75f, 0), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)),
                new VertexPositionColor(new Vector3(.75f, .75f, 0), new Vector4(0.0f, 1.0f, 0.0f, 1.0f)),
                new VertexPositionColor(new Vector3(-.75f, -.75f, 0), new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new VertexPositionColor(new Vector3(.75f, -.75f, 0), new Vector4(1.0f, 1.0f, 0.0f, 1.0f))
            };

            geometry.VertexData = quadVertices;

            uint[] quadIndices = {0, 1, 2, 3};
            geometry.IndexData = quadIndices;

            var pSet = DrawElements<VertexPositionColor>.Create(
                geometry,
                PrimitiveTopology.TriangleStrip,
                (uint) quadIndices.Length,
                1,
                0,
                0,
                0);

            geometry.PrimitiveSets.Add(pSet);
            geometry.PrimitiveSets.Add(pSet);

            geometry.VertexLayouts = new List<VertexLayoutDescription>
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate,
                        VertexElementFormat.Float3),
                    new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate,
                        VertexElementFormat.Float4))
            };

            var geode = Geode.Create();
            geode.AddDrawable(geometry);

            var billboard = Billboard.Create();
            //billboard.SizeMode = Billboard.SizeModes.ScreenCoords;
            billboard.AddDrawable(geometry);

            var leftXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(1, 0, 0));
            leftXForm.AddChild(geode);

            var rightXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(-1, 0, 0));
            rightXForm.AddChild(billboard);

            root.AddChild(leftXForm);
            root.AddChild(rightXForm);
            root.PipelineState = CreateSharedState();

            viewer.SetSceneData(root);
            viewer.ViewAll();
            viewer.Run();
        }

        private static IPipelineState CreateSharedState()
        {
            var pso = PipelineState.Create();

            pso.ShaderSet = Vertex3Color4Shader.Instance.ShaderSet;

            return pso;
        }
    }
}