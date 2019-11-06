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
using System.Runtime.CompilerServices;
using Examples.Common;
using SharpDX.Direct3D11;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Viewer;
using INodeCallback = Veldrid.SceneGraph.INodeCallback;

namespace UpdateVisitor
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

    internal class UpdateCallback : INodeCallback
    {
        public bool Run(IObject obj, IObject data)
        {
            Console.WriteLine("UpdateCallback");
            return true;
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper.Configure();
            
            var viewer = SimpleViewer.Create("Update Visitor Example (Press 'u' to switch updates on/off)");
            viewer.SetCameraManipulator(TrackballManipulator.Create());

            var root = Group.Create();
            var cube = CreateCube();

            var leftXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(-5, 0, 0));
            leftXForm.AddChild(cube);

            var centerXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(0, 0, 0));
            centerXForm.AddChild(cube);
            
            var rightXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(5, 0, 0));
            rightXForm.AddChild(cube);
            
            rightXForm.SetUpdateCallback(new UpdateCallback());
            
            root.AddChild(leftXForm);
            root.AddChild(centerXForm);
            root.AddChild(rightXForm);
            
            viewer.AddInputEventHandler(new UpdateInputHandler(root));
            
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
                      
            geometry.PipelineState.VertexShaderDescription = Vertex3Color4Shader.Instance.VertexShaderDescription;
            geometry.PipelineState.FragmentShaderDescription = Vertex3Color4Shader.Instance.FragmentShaderDescription;

            var geode = Geode.Create();
            geode.AddDrawable(geometry);

            return geode;
        }
    }
}