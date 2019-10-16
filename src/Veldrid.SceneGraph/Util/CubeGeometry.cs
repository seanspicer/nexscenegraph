using System;
using System.Numerics;
using Veldrid.SceneGraph.VertexTypes;

//
// Copyright 2018 Sean Spicer 
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

namespace Veldrid.SceneGraph.Util
{
    internal class CubeGeometry
    {
        internal static IGeode CreatePosition3Texture2Color3Normal3_IndexedTriangleList()
        {
            var geometry = Geometry<Position3Texture2Color3Normal3>.Create();

            var nl = 1f / (float)System.Math.Sqrt(3f);
            
            var vertices = new[]
            {
                // Top
                new Position3Texture2Color3Normal3(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0), new Vector3(1, 0, 0), new Vector3(-nl,  nl, -nl)),
                new Position3Texture2Color3Normal3(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0), new Vector3(1, 0, 0), new Vector3( nl,  nl, -nl)),
                new Position3Texture2Color3Normal3(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 1), new Vector3(1, 0, 0), new Vector3( nl,  nl,  nl)),
                new Position3Texture2Color3Normal3(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 1), new Vector3(1, 0, 0), new Vector3(-nl,  nl,  nl)),
                // Bottom                                                             
                new Position3Texture2Color3Normal3(new Vector3(-0.5f,-0.5f, +0.5f),  new Vector2(0, 0), new Vector3(1, 1, 0), new Vector3(-nl, -nl,  nl)),
                new Position3Texture2Color3Normal3(new Vector3(+0.5f,-0.5f, +0.5f),  new Vector2(1, 0), new Vector3(1, 1, 0), new Vector3( nl, -nl,  nl)),
                new Position3Texture2Color3Normal3(new Vector3(+0.5f,-0.5f, -0.5f),  new Vector2(1, 1), new Vector3(1, 1, 0), new Vector3( nl, -nl, -nl)),
                new Position3Texture2Color3Normal3(new Vector3(-0.5f,-0.5f, -0.5f),  new Vector2(0, 1), new Vector3(1, 1, 0), new Vector3(-nl, -nl, -nl)),
                // Left                                                               
                new Position3Texture2Color3Normal3(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0), new Vector3(0, 1, 0), new Vector3(-nl,  nl, -nl)),
                new Position3Texture2Color3Normal3(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(1, 0), new Vector3(0, 1, 0), new Vector3(-nl,  nl,  nl)),
                new Position3Texture2Color3Normal3(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(1, 1), new Vector3(0, 1, 0), new Vector3(-nl, -nl,  nl)),
                new Position3Texture2Color3Normal3(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(0, 1), new Vector3(0, 1, 0), new Vector3(-nl, -nl, -nl)),
                // Right                                                              
                new Position3Texture2Color3Normal3(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(0, 0), new Vector3(0, 1, 1), new Vector3( nl,  nl,  nl)),
                new Position3Texture2Color3Normal3(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0), new Vector3(0, 1, 1), new Vector3( nl,  nl, -nl)),
                new Position3Texture2Color3Normal3(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(1, 1), new Vector3(0, 1, 1), new Vector3( nl, -nl, -nl)),
                new Position3Texture2Color3Normal3(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(0, 1), new Vector3(0, 1, 1), new Vector3( nl, -nl,  nl)),
                // Back                                                               
                new Position3Texture2Color3Normal3(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(0, 0), new Vector3(0, 0, 1), new Vector3( nl,  nl, -nl)),
                new Position3Texture2Color3Normal3(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(1, 0), new Vector3(0, 0, 1), new Vector3(-nl,  nl, -nl)),
                new Position3Texture2Color3Normal3(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(1, 1), new Vector3(0, 0, 1), new Vector3(-nl, -nl, -nl)),
                new Position3Texture2Color3Normal3(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(0, 1), new Vector3(0, 0, 1), new Vector3( nl, -nl, -nl)),
                // Front                                                              
                new Position3Texture2Color3Normal3(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 0), new Vector3(1, 0, 1), new Vector3(-nl,  nl,  nl)),
                new Position3Texture2Color3Normal3(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 0), new Vector3(1, 0, 1), new Vector3( nl,  nl,  nl)),
                new Position3Texture2Color3Normal3(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(1, 1), new Vector3(1, 0, 1), new Vector3( nl, -nl,  nl)),
                new Position3Texture2Color3Normal3(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(0, 1), new Vector3(1, 0, 1), new Vector3(-nl, -nl,  nl)),
            };
            
            uint[] indices =
            {
                0,1,2, 0,2,3,
                4,5,6, 4,6,7,
                8,9,10, 8,10,11,
                12,13,14, 12,14,15,
                16,17,18, 16,18,19,
                20,21,22, 20,22,23,
            };
            
            geometry.VertexData = vertices;
            geometry.IndexData = indices;

            geometry.VertexLayout = Position3Texture2Color3Normal3.VertexLayoutDescription;

            var pSet = DrawElements<Position3Texture2Color3Normal3>.Create(
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
    }
}