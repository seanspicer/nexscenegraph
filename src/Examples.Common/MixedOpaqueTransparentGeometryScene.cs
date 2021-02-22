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
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.VertexTypes;

namespace Examples.Common
{

    
    public class MixedOpaqueTransparentGeometryScene
    {
        public static IGroup Build()
        {
            var root = Group.Create();
            
            // Construct Transparent Geometry
            var transparentGroup = Group.Create();            
            var scale_xform = MatrixTransform.Create(Matrix4x4.CreateScale(0.35f));
            
            var cube = CreateCube();
            scale_xform.AddChild(cube);
            
            var gridSize = 1;
            var transF = 1.0f / gridSize;
            for (var i = -gridSize; i <= gridSize; ++i)
            {
                for (var j = -gridSize; j <= gridSize; ++j)
                {
                    for (var k = -gridSize; k <= gridSize; ++k)
                    {
                        var xform = MatrixTransform.Create(
                            Matrix4x4.CreateTranslation(transF * i, transF * j, transF * k)
                        );

                        xform.AddChild(scale_xform);
                        transparentGroup.AddChild(xform);
                    }
                }
            }
            transparentGroup.PipelineState = TransparentState();
            root.AddChild(transparentGroup);
            
            // Construct Opaque Geometry

            var greyCube = CreateUniformColorCube();
            
            var opaqueGroup = Group.Create();
            var opaqueScaleX = MatrixTransform.Create(Matrix4x4.CreateScale(2, 0.10f, 0.10f));
            opaqueScaleX.PipelineState = OpaqueState();
            opaqueScaleX.AddChild(greyCube);
            
            var opaqueScaleY = MatrixTransform.Create(Matrix4x4.CreateScale(0.10f, 2f, 0.10f));
            opaqueScaleY.PipelineState = OpaqueState();
            opaqueScaleY.AddChild(greyCube);
            
            var opaqueScaleZ = MatrixTransform.Create(Matrix4x4.CreateScale(0.10f, 0.10f, 2f));
            opaqueScaleZ.PipelineState = OpaqueState();
            opaqueScaleZ.AddChild(greyCube);

            opaqueGroup.AddChild(opaqueScaleX);
            opaqueGroup.AddChild(opaqueScaleY);
            opaqueGroup.AddChild(opaqueScaleZ);
            root.AddChild(opaqueGroup);

            return root;
        }
        
        static IGeode CreateCube()
        {
            var geode = Geode.Create();
            
            var vertices = new List<Position3Color4>
            {
                // Top
                new Position3Color4(new Vector3(-1.0f, +1.0f, -1.0f), new Vector4(1.0f, 0.0f, 0.0f, 0.5f)),
                new Position3Color4(new Vector3(+1.0f, +1.0f, -1.0f), new Vector4(1.0f, 0.0f, 0.0f, 0.5f)),
                new Position3Color4(new Vector3(+1.0f, +1.0f, +1.0f), new Vector4(1.0f, 0.0f, 0.0f, 0.5f)),
                new Position3Color4(new Vector3(-1.0f, +1.0f, +1.0f), new Vector4(1.0f, 0.0f, 0.0f, 0.5f)),
                // Bottom                                                             
                new Position3Color4(new Vector3(-1.0f,-1.0f, +1.0f),  new Vector4(1.0f, 1.0f, 0.0f, 0.5f)),
                new Position3Color4(new Vector3(+1.0f,-1.0f, +1.0f),  new Vector4(1.0f, 1.0f, 0.0f, 0.5f)),
                new Position3Color4(new Vector3(+1.0f,-1.0f, -1.0f),  new Vector4(1.0f, 1.0f, 0.0f, 0.5f)),
                new Position3Color4(new Vector3(-1.0f,-1.0f, -1.0f),  new Vector4(1.0f, 1.0f, 0.0f, 0.5f)),
                // Left                                                               
                new Position3Color4(new Vector3(-1.0f, +1.0f, -1.0f), new Vector4(0.0f, 1.0f, 0.0f, 0.5f)),
                new Position3Color4(new Vector3(-1.0f, +1.0f, +1.0f), new Vector4(0.0f, 1.0f, 0.0f, 0.5f)),
                new Position3Color4(new Vector3(-1.0f, -1.0f, +1.0f), new Vector4(0.0f, 1.0f, 0.0f, 0.5f)),
                new Position3Color4(new Vector3(-1.0f, -1.0f, -1.0f), new Vector4(0.0f, 1.0f, 0.0f, 0.5f)),
                // Right                                                              
                new Position3Color4(new Vector3(+1.0f, +1.0f, +1.0f), new Vector4(0.0f, 1.0f, 1.0f, 0.5f)),
                new Position3Color4(new Vector3(+1.0f, +1.0f, -1.0f), new Vector4(0.0f, 1.0f, 1.0f, 0.5f)),
                new Position3Color4(new Vector3(+1.0f, -1.0f, -1.0f), new Vector4(0.0f, 1.0f, 1.0f, 0.5f)),
                new Position3Color4(new Vector3(+1.0f, -1.0f, +1.0f), new Vector4(0.0f, 1.0f, 1.0f, 0.5f)),
                // Back                                                               
                new Position3Color4(new Vector3(+1.0f, +1.0f, -1.0f), new Vector4(0.0f, 0.0f, 1.0f, 0.5f)),
                new Position3Color4(new Vector3(-1.0f, +1.0f, -1.0f), new Vector4(0.0f, 0.0f, 1.0f, 0.5f)),
                new Position3Color4(new Vector3(-1.0f, -1.0f, -1.0f), new Vector4(0.0f, 0.0f, 1.0f, 0.5f)),
                new Position3Color4(new Vector3(+1.0f, -1.0f, -1.0f), new Vector4(0.0f, 0.0f, 1.0f, 0.5f)),
                // Front                                                              
                new Position3Color4(new Vector3(-1.0f, +1.0f, +1.0f), new Vector4(1.0f, 0.0f, 1.0f, 0.5f)),
                new Position3Color4(new Vector3(+1.0f, +1.0f, +1.0f), new Vector4(1.0f, 0.0f, 1.0f, 0.5f)),
                new Position3Color4(new Vector3(+1.0f, -1.0f, +1.0f), new Vector4(1.0f, 0.0f, 1.0f, 0.5f)),
                new Position3Color4(new Vector3(-1.0f, -1.0f, +1.0f), new Vector4(1.0f, 0.0f, 1.0f, 0.5f)),
            };
            
            var indices = new List<uint>
            {
                0,1,2, 0,2,3,
                4,5,6, 4,6,7,
                8,9,10, 8,10,11,
                12,13,14, 12,14,15,
                16,17,18, 16,18,19,
                20,21,22, 20,22,23,
            };

            var faces = new int[] {0, 1, 2, 3, 4, 5};
            
            var vld = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));
            
            foreach(var f in faces)
            {
                var start = 6 * f;

                var geometry = Geometry<Position3Color4>.Create();
                
                geometry.VertexData = vertices.ToArray();
                geometry.IndexData = indices.GetRange(start, 6).ToArray();

                // TODO -> this causes multiple render states
                geometry.VertexLayouts = new List<VertexLayoutDescription>() {vld};

                var pSet = DrawElements<Position3Color4>.Create(
                    geometry, 
                    PrimitiveTopology.TriangleList,
                    (uint)geometry.IndexData.Length, 
                    1, 
                    0, 
                    0, 
                    0);
            
                geometry.PrimitiveSets.Add(pSet);
                
                geode.AddDrawable(geometry);

            }

            return geode;
        }
        
        static IGeode CreateUniformColorCube()
        {
            var geometry = Geometry<Position3Color4>.Create();

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
                new Vector4(0.8f, 0.8f, 0.8f, 1.0f),
                new Vector4(0.8f, 0.8f, 0.8f, 1.0f),
                new Vector4(0.8f, 0.8f, 0.8f, 1.0f),
                new Vector4(0.8f, 0.8f, 0.8f, 1.0f),
                new Vector4(0.8f, 0.8f, 0.8f, 1.0f),
                new Vector4(0.8f, 0.8f, 0.8f, 1.0f),
                new Vector4(0.8f, 0.8f, 0.8f, 1.0f),
            };

            uint[] cubeIndices   = {3, 2, 7, 6, 4, 2, 0, 3, 1, 7, 5, 4, 1, 0};
            ushort[] colorIndices = {0, 0, 4, 1, 1, 2, 2, 3, 3, 4, 5, 5};
            
            var cubeTriangleVertices = new List<Position3Color4>();
            var cubeTriangleIndices = new List<uint>();

            for (var i = 0; i < cubeIndices.Length-2; ++i)
            {
                if (0 == (i % 2))
                {
                    cubeTriangleVertices.Add(new Position3Color4(cubeVertices[cubeIndices[i]],   faceColors[colorIndices[i]]));
                    cubeTriangleVertices.Add(new Position3Color4(cubeVertices[cubeIndices[i+1]], faceColors[colorIndices[i]]));
                    cubeTriangleVertices.Add(new Position3Color4(cubeVertices[cubeIndices[i+2]], faceColors[colorIndices[i]]));
                }
                else
                {
                    cubeTriangleVertices.Add(new Position3Color4(cubeVertices[cubeIndices[i+1]], faceColors[colorIndices[i]]));
                    cubeTriangleVertices.Add(new Position3Color4(cubeVertices[cubeIndices[i]],   faceColors[colorIndices[i]]));
                    cubeTriangleVertices.Add(new Position3Color4(cubeVertices[cubeIndices[i+2]], faceColors[colorIndices[i]]));
                }
                
                cubeTriangleIndices.Add((uint) (3 * i));
                cubeTriangleIndices.Add((uint) (3 * i + 1));
                cubeTriangleIndices.Add((uint) (3 * i + 2));
            }

            geometry.VertexData = cubeTriangleVertices.ToArray();

            geometry.IndexData = cubeTriangleIndices.ToArray();

            geometry.VertexLayouts = new List<VertexLayoutDescription>()
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate,
                        VertexElementFormat.Float3),
                    new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate,
                        VertexElementFormat.Float4))

            };
            
            var pSet = DrawElements<Position3Color4>.Create(
                geometry, 
                PrimitiveTopology.TriangleList,
                (uint)geometry.IndexData.Length, 
                1, 
                0, 
                0, 
                0);
            
            geometry.PrimitiveSets.Add(pSet);

            var geode = Geode.Create();
            geode.AddDrawable(geometry);
            
            return geode;
        }
        
        
        private static IPipelineState TransparentState()
        {
            var pso = PipelineState.Create();

            pso.ShaderSet = Vertex3Color4Shader.Instance.ShaderSet;

            pso.BlendStateDescription = BlendStateDescription.SingleAlphaBlend;
            pso.RasterizerStateDescription = RasterizerStateDescription.CullNone;
            
            return pso;
        }

        private static IPipelineState OpaqueState()
        {
            var pso = PipelineState.Create();

            pso.ShaderSet = Vertex3Color4Shader.Instance.ShaderSet;
            
            return pso;
        }
    }
}