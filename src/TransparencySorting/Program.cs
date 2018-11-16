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
using System.Linq;
using System.Numerics;
using ShaderGen;
using SharpDX.Mathematics.Interop;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Viewer;

namespace TransparencySorting
{
    public struct VertexPositionColor : IPrimitiveElement
    {
        public const uint SizeInBytes = 28;

        [PositionSemantic] 
        public Vector3 Position;
        [ColorSemantic]
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
            var asm = typeof(Program).Assembly;
            
            var allNames = asm.GetManifestResourceNames();
            
            var viewer = new SimpleViewer("Transparancy Sorting Demo");
            viewer.View.CameraManipulator = new TrackballManipulator();

            var root = Group.Create();

            root.AddChild(CreateCube());

            root.PipelineState = CreateSharedState();
            
            viewer.SceneData = root;

            viewer.Run();
        }

        static Geode CreateCube()
        {
            var geode = new Geode();
            
            var vertices = new List<VertexPositionColor>
            {
                // Top
                new VertexPositionColor(new Vector3(-1.0f, +1.0f, -1.0f), new Vector4(1.0f, 0.0f, 0.0f, 0.4f)),
                new VertexPositionColor(new Vector3(+1.0f, +1.0f, -1.0f), new Vector4(1.0f, 0.0f, 0.0f, 0.4f)),
                new VertexPositionColor(new Vector3(+1.0f, +1.0f, +1.0f), new Vector4(1.0f, 0.0f, 0.0f, 0.4f)),
                new VertexPositionColor(new Vector3(-1.0f, +1.0f, +1.0f), new Vector4(1.0f, 0.0f, 0.0f, 0.4f)),
                // Bottom                                                             
                new VertexPositionColor(new Vector3(-1.0f,-1.0f, +1.0f),  new Vector4(1.0f, 1.0f, 0.0f, 0.4f)),
                new VertexPositionColor(new Vector3(+1.0f,-1.0f, +1.0f),  new Vector4(1.0f, 1.0f, 0.0f, 0.4f)),
                new VertexPositionColor(new Vector3(+1.0f,-1.0f, -1.0f),  new Vector4(1.0f, 1.0f, 0.0f, 0.4f)),
                new VertexPositionColor(new Vector3(-1.0f,-1.0f, -1.0f),  new Vector4(1.0f, 1.0f, 0.0f, 0.4f)),
                // Left                                                               
                new VertexPositionColor(new Vector3(-1.0f, +1.0f, -1.0f), new Vector4(0.0f, 1.0f, 0.0f, 0.4f)),
                new VertexPositionColor(new Vector3(-1.0f, +1.0f, +1.0f), new Vector4(0.0f, 1.0f, 0.0f, 0.4f)),
                new VertexPositionColor(new Vector3(-1.0f, -1.0f, +1.0f), new Vector4(0.0f, 1.0f, 0.0f, 0.4f)),
                new VertexPositionColor(new Vector3(-1.0f, -1.0f, -1.0f), new Vector4(0.0f, 1.0f, 0.0f, 0.4f)),
                // Right                                                              
                new VertexPositionColor(new Vector3(+1.0f, +1.0f, +1.0f), new Vector4(0.0f, 1.0f, 1.0f, 0.4f)),
                new VertexPositionColor(new Vector3(+1.0f, +1.0f, -1.0f), new Vector4(0.0f, 1.0f, 1.0f, 0.4f)),
                new VertexPositionColor(new Vector3(+1.0f, -1.0f, -1.0f), new Vector4(0.0f, 1.0f, 1.0f, 0.4f)),
                new VertexPositionColor(new Vector3(+1.0f, -1.0f, +1.0f), new Vector4(0.0f, 1.0f, 1.0f, 0.4f)),
                // Back                                                               
                new VertexPositionColor(new Vector3(+1.0f, +1.0f, -1.0f), new Vector4(0.0f, 0.0f, 1.0f, 0.4f)),
                new VertexPositionColor(new Vector3(-1.0f, +1.0f, -1.0f), new Vector4(0.0f, 0.0f, 1.0f, 0.4f)),
                new VertexPositionColor(new Vector3(-1.0f, -1.0f, -1.0f), new Vector4(0.0f, 0.0f, 1.0f, 0.4f)),
                new VertexPositionColor(new Vector3(+1.0f, -1.0f, -1.0f), new Vector4(0.0f, 0.0f, 1.0f, 0.4f)),
                // Front                                                              
                new VertexPositionColor(new Vector3(-1.0f, +1.0f, +1.0f), new Vector4(1.0f, 0.0f, 1.0f, 0.4f)),
                new VertexPositionColor(new Vector3(+1.0f, +1.0f, +1.0f), new Vector4(1.0f, 0.0f, 1.0f, 0.4f)),
                new VertexPositionColor(new Vector3(+1.0f, -1.0f, +1.0f), new Vector4(1.0f, 0.0f, 1.0f, 0.4f)),
                new VertexPositionColor(new Vector3(-1.0f, -1.0f, +1.0f), new Vector4(1.0f, 0.0f, 1.0f, 0.4f)),
            };
            
            var indices = new List<ushort>
            {
                0,1,2, 0,2,3,
                4,5,6, 4,6,7,
                8,9,10, 8,10,11,
                12,13,14, 12,14,15,
                16,17,18, 16,18,19,
                20,21,22, 20,22,23,
            };

            var faces = new int[] {0, 1, 2, 3, 4, 5};
            
            var scaleMatrix = Matrix4x4.CreateScale(0.15f);

            var sceneVertices = new List<VertexPositionColor>();
            var sceneIndices = new List<ushort>();
            
            var geometry = Geometry<VertexPositionColor>.Create();
            
            var gridSize = 3;
            var transF = 1.0f / gridSize;
            for (var i = -gridSize; i <= gridSize; ++i)
            {
                for (var j = -gridSize; j <= gridSize; ++j)
                {
                    for (var k = -gridSize; k <= gridSize; ++k)
                    {
                        var transMatrix = Matrix4x4.CreateTranslation(transF * i, transF * j, transF * k);

                        var cumMat = scaleMatrix.PostMultiply(transMatrix);

                        var curSceneIdx = (uint) sceneVertices.Count();
                        var drawElementStart = (uint) sceneIndices.Count();

                        // Transform vertices and record
                        foreach (var vtx in vertices)
                        {
                            var tmp = vtx;
                            tmp.Position = Vector3.Transform(vtx.Position, cumMat);
                            sceneVertices.Add(tmp);
                        }
                        
                        foreach (var f in faces)
                        {
                            var start = (6 * f);
                            var faceIndices = indices.GetRange(start, 6);

                            foreach (var idx in faceIndices)
                            {
                                sceneIndices.Add((ushort)(curSceneIdx+idx));
                            }
                            
                            var drawElements =
                                DrawElements<VertexPositionColor>.Create(
                                    geometry,
                                    PrimitiveTopology.TriangleList,
                                    6,
                                    1,
                                    drawElementStart + (uint) start,
                                    0,
                                    0);
                            
                            geometry.PrimitiveSets.Add(drawElements);

                        }
                    }
                }
            }
            
            geometry.VertexData = sceneVertices.ToArray();
            geometry.IndexData = sceneIndices.ToArray();
            geometry.VertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
                new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float4));
            
            geode.AddDrawable(geometry);

            return geode;
        }
        
        
        private static IPipelineState CreateSharedState()
        {
            var pso = PipelineState.Create();

            pso.VertexShaderDescription = Vertex3Color4Shader.Instance.VertexShaderDescription;
            pso.FragmentShaderDescription = Vertex3Color4Shader.Instance.FragmentShaderDescription;

            pso.BlendStateDescription = BlendStateDescription.SingleAlphaBlend;
            pso.RasterizerStateDescription = RasterizerStateDescription.CullNone;
            
            return pso;
        }
        
    }
}