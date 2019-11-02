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

using System.Collections.Generic;
using System.Numerics;
using SharpDX;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.VertexTypes;

namespace Examples.Common
{
    public class ConstLineWidthExampleScene
    {
        public static IGroup Build()
        {
            var root = Group.Create();
            root.AddChild(CreateCube());
            return root;
        }

        private static IGeode CreateCube()
        {
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
            
            var cubeTriangleVertices = new List<Position3Color3>();
            var cubeTriangleIndices = new List<uint>();

            uint[] indicies = {
                0, 1, 3, 2, 0, 
                4, 5, 7, 6, 4,
                0, 4, 
                1, 5, 
                2, 6, 
                3, 7};
            
            
            var geometry = Geometry<Position3Color3>.Create();

            foreach (var t in cubeVertices)
            {
                cubeTriangleVertices.Add(new Position3Color3(
                    t, Vector3.UnitX));
            }

            geometry.VertexData = cubeTriangleVertices.ToArray();

            geometry.IndexData = indicies;

            geometry.VertexLayout = Position3Color3.VertexLayoutDescription;
            
            // Top
            {
                var pSet = DrawElements<Position3Color3>.Create(
                    geometry, 
                    PrimitiveTopology.LineStrip,
                    5, 
                    1, 
                    0, 
                    0, 
                    0);
            
                geometry.PrimitiveSets.Add(pSet);
            }
            
            // Bottom
            {
                var pSet = DrawElements<Position3Color3>.Create(
                    geometry, 
                    PrimitiveTopology.LineStrip,
                    5, 
                    1, 
                    5, 
                    0, 
                    0);
            
                geometry.PrimitiveSets.Add(pSet);
            }
            // Sides
            {
                for (uint i = 0; i < 4; ++i)
                {
                    {
                        var pSet = DrawElements<Position3Color3>.Create(
                            geometry, 
                            PrimitiveTopology.LineStrip,
                            2, 
                            1, 
                            10+(i*2), 
                            0, 
                            0);
            
                        geometry.PrimitiveSets.Add(pSet);
                    }
                }
            }
            
            geometry.PipelineState.VertexShaderDescription = Position3Color3Shader.Instance.VertexShaderDescription;
            geometry.PipelineState.FragmentShaderDescription = Position3Color3Shader.Instance.FragmentShaderDescription;

            var geode = Geode.Create();
            geode.AddDrawable(geometry);

            return geode;
        }
    }
}