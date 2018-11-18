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
using System.Numerics;
using ShaderGen;
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
            
            var viewer = SimpleViewer.Create("Hello Veldrid Scene Graph");
            viewer.View.CameraManipulator = TrackballManipulator.Create();

            var root = Group.Create();
            
            var geometry = Geometry<VertexPositionColor>.Create();
            
            VertexPositionColor[] quadVertices =
            {
                new VertexPositionColor(new Vector3(-.75f, .75f, 0),  new Vector4(1.0f, 0.0f, 0.0f, 1.0f)),
                new VertexPositionColor(new Vector3(.75f, .75f, 0),   new Vector4(0.0f, 1.0f, 0.0f, 1.0f)),
                new VertexPositionColor(new Vector3(-.75f, -.75f, 0), new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new VertexPositionColor(new Vector3(.75f, -.75f, 0),  new Vector4(1.0f, 1.0f, 0.0f, 1.0f))
            };

            geometry.VertexData = quadVertices;
            
            ushort[] quadIndices = { 0, 1, 2, 3 };
            geometry.IndexData = quadIndices;
            
            var pSet = DrawElements<VertexPositionColor>.Create(
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
                new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
                new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float4));
                        
            var geode = Geode.Create();
            geode.AddDrawable(geometry);
            
            var billboard = Billboard.Create();
            billboard.AddDrawable(geometry);
            
            var leftXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(1, 0, 0));
            leftXForm.AddChild(geode);
            
            var rightXForm = MatrixTransform.Create(Matrix4x4.CreateTranslation(-1, 0, 0));
            rightXForm.AddChild(billboard);
            
            root.AddChild(leftXForm);
            root.AddChild(rightXForm);
            root.PipelineState = CreateSharedState();

            viewer.View.SceneData = root;
            viewer.ViewAll();            
            viewer.Run();

        }
        
        private static IPipelineState CreateSharedState()
        {
            var pso = PipelineState.Create();

            pso.VertexShaderDescription = Vertex3Color4Shader.Instance.VertexShaderDescription;
            pso.FragmentShaderDescription = Vertex3Color4Shader.Instance.FragmentShaderDescription;

            return pso;
        }
    }
}