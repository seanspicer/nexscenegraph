//
// Copyright 2018-2019 Sean Spicer 
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

using System;
using System.Collections.Generic;
using System.Numerics;
using CullingColoredCubes;
using Examples.Common;
using SharpDX.Mathematics.Interop;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Viewer;

namespace ColoredCube
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
    
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper.Configure();
            
            var viewer = SimpleViewer.Create("Culling Colored Cube Scene Graph");
            viewer.SetCameraManipulator(TrackballManipulator.Create());
            //viewer.AddInputEventHandler(new PickEventHandler(viewer.View));

            var root = Group.Create();
            root.NameString = "Root";
            
            var scale_xform = MatrixTransform.Create(Matrix4x4.CreateScale(0.05f));
            scale_xform.NameString = "Scale XForm";
            
            var cube = CreateCube();
            scale_xform.AddChild(cube);

            var gridSize = 10;
            var transF = 2.0f / gridSize;
            for (var i = -gridSize; i <= gridSize; ++i)
            {
                for (var j = -gridSize; j <= gridSize; ++j)
                {
                    var xform = MatrixTransform.Create(Matrix4x4.CreateTranslation(transF*i, transF*j, 0.0f));
                    xform.NameString = $"XForm[{i}, {j}]";
                    xform.AddChild(scale_xform);
                    root.AddChild(xform);
                }
            }

            root.PipelineState = CreateSharedState();
            
            viewer.SetSceneData(root);
            viewer.ViewAll();
            viewer.Run();
        }

        static IGeode CreateCube()
        {
            var geometry = Geometry<VertexPositionColor>.Create();

            // TODO - make this a color index cube
            Vector3[] cubeVertices =
            {
                new Vector3( 1.0f, 1.0f,-1.0f), // (0) Back top right  
                new Vector3(-1.0f, 1.0f,-1.0f), // (1) Back top left
                new Vector3( 1.0f, 1.0f, 1.0f), // (2) Front top right
                new Vector3(-1.0f, 1.0f, 1.0f), // (3) Front top left
                new Vector3( 1.0f,-1.0f,-1.0f), // (4) Back bottom right
                new Vector3(-1.0f,-1.0f,-1.0f), // (5) Back bottom left
                new Vector3( 1.0f,-1.0f, 1.0f), // (6) Front bottom right
                new Vector3(-1.0f,-1.0f, 1.0f)  // (7) Front bottom left
            };

            Vector4[] faceColors =
            {
                new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                new Vector4(0.1f, 0.1f, 0.1f, 1.0f) 
            };

            uint[] cubeIndices   = {3, 2, 7, 6, 4, 2, 0, 3, 1, 7, 5, 4, 1, 0};
            ushort[] colorIndices = {0, 0, 4, 1, 1, 2, 2, 3, 3, 4, 5, 5};
            
            var cubeTriangleVertices = new List<VertexPositionColor>();
            var cubeTriangleIndices = new List<uint>();

            for (var i = 0; i < cubeIndices.Length-2; ++i)
            {
                if (0 == (i % 2))
                {
                    cubeTriangleVertices.Add(new VertexPositionColor(cubeVertices[cubeIndices[i]],   faceColors[colorIndices[i]]));
                    cubeTriangleVertices.Add(new VertexPositionColor(cubeVertices[cubeIndices[i+1]], faceColors[colorIndices[i]]));
                    cubeTriangleVertices.Add(new VertexPositionColor(cubeVertices[cubeIndices[i+2]], faceColors[colorIndices[i]]));
                }
                else
                {
                    cubeTriangleVertices.Add(new VertexPositionColor(cubeVertices[cubeIndices[i+1]], faceColors[colorIndices[i]]));
                    cubeTriangleVertices.Add(new VertexPositionColor(cubeVertices[cubeIndices[i]],   faceColors[colorIndices[i]]));
                    cubeTriangleVertices.Add(new VertexPositionColor(cubeVertices[cubeIndices[i+2]], faceColors[colorIndices[i]]));
                }
                
                cubeTriangleIndices.Add((uint) (3 * i));
                cubeTriangleIndices.Add((uint) (3 * i + 1));
                cubeTriangleIndices.Add((uint) (3 * i + 2));
            }

            geometry.VertexData = cubeTriangleVertices.ToArray();

            geometry.IndexData = cubeTriangleIndices.ToArray();

            geometry.VertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

            var pSet = DrawElements<VertexPositionColor>.Create(
                geometry, 
                PrimitiveTopology.TriangleList,
                (uint)geometry.IndexData.Length, 
                
                1, 
                0, 
                0, 
                0);
            
            geometry.PrimitiveSets.Add(pSet);

            var geode = Geode.Create();
            geode.NameString = "Cube Geode";
            geometry.Name = "Colored Cube";
            geode.AddDrawable(geometry);
            return geode;
        }
        
        private static IPipelineState CreateSharedState()
        {
            var pso = PipelineState.Create();

            pso.ShaderSet = Vertex3Color4Shader.Instance.ShaderSet;

            return pso;
        }
        
    }
}