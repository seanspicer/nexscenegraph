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
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Util.Shape
{
    internal class BuildBoxGeometry<T> : GeometryBuilderBase<T> where T : struct, ISettablePrimitiveElement
    {
        private uint _instanceCount = 1;

        internal void Build(IGeometry<T> geometry, ITessellationHints hints, Vector3[] colors, uint instanceCount,
            IBox box)
        {
            Matrix = Matrix4x4.CreateFromQuaternion(box.Rotation) * Matrix4x4.CreateTranslation(box.Center);

            _instanceCount = instanceCount;

            var dx = box.HalfLengths.X;
            var dy = box.HalfLengths.Y;
            var dz = box.HalfLengths.Z;

            var normVec = Vector3.Normalize(box.HalfLengths);

            var nx = normVec.X;
            var ny = normVec.Y;
            var nz = normVec.Z;

            var vertices = new Vector3[8]
            {
                new Vector3(-dx, +dy, -dz), // T1, L1, Bk2 
                new Vector3(+dx, +dy, -dz), // T2, R2, Bk1
                new Vector3(+dx, +dy, +dz), // T3, R1, F2
                new Vector3(-dx, +dy, +dz), // T4, L2, F1
                new Vector3(-dx, -dy, -dz), // B4, L4, Bk3
                new Vector3(+dx, -dy, -dz), // B3, R3, Bk4
                new Vector3(+dx, -dy, +dz), // B2, R4, F3
                new Vector3(-dx, -dy, +dz) // B1, L3, F4
            };

            Vector3[] normals;
            if (hints.NormalsType == NormalsType.PerVertex)
                normals = new Vector3[8]
                {
                    new Vector3(-nx, +ny, -nz), // T1, L1, Bk2
                    new Vector3(+nx, +ny, -nz), // T2, R2, Bk1
                    new Vector3(+nx, +ny, +nz), // T3, R1, F2
                    new Vector3(-nx, +ny, +nz), // T4, L2, F1
                    new Vector3(-nx, -ny, -nz), // B4, L4, Bk3
                    new Vector3(+nx, -ny, -nz), // B3, R3, Bk4
                    new Vector3(+nx, -ny, +nz), // B2, R4, F3
                    new Vector3(-nx, -ny, +nz) // B1, L3, F4
                };
            else
                normals = new Vector3[6]
                {
                    new Vector3(0, 1, 0), // T
                    new Vector3(0, -1, 0), // B
                    new Vector3(1, 0, 0), // R
                    new Vector3(-1, 0, 0), // L
                    new Vector3(0, 0, 1), // F
                    new Vector3(0, 0, -1) // Bk
                };

            var texcoords = new Vector2[4]
            {
                new Vector2(+0, +0),
                new Vector2(+0, +1),
                new Vector2(+1, +1),
                new Vector2(+1, +0)
            };

            var faces = new Face[6]
            {
                new Face(), // T
                new Face(), // B
                new Face(), // R
                new Face(), // L
                new Face(), // F
                new Face() // B
            };

            // Set Positions
            {
                faces[0].VertexIndices.AddRange(new uint[] {0, 1, 2, 3}); // T
                faces[1].VertexIndices.AddRange(new uint[] {7, 6, 5, 4}); // B
                faces[2].VertexIndices.AddRange(new uint[] {2, 1, 5, 6}); // R
                faces[3].VertexIndices.AddRange(new uint[] {0, 3, 7, 4}); // L
                faces[4].VertexIndices.AddRange(new uint[] {3, 2, 6, 7}); // F
                faces[5].VertexIndices.AddRange(new uint[] {1, 0, 4, 5}); // B
            }

            // Set Tex Coords
            {
                for (uint i = 0; i < 6; ++i) faces[i].TexCoordIndices.AddRange(new uint[] {0, 1, 2, 3});
            }

            // Set Normals
            if (hints.NormalsType == NormalsType.PerVertex)
            {
                faces[0].NormalIndices.AddRange(new uint[] {0, 1, 2, 3}); // T
                faces[1].NormalIndices.AddRange(new uint[] {7, 6, 5, 4}); // B
                faces[2].NormalIndices.AddRange(new uint[] {2, 1, 5, 6}); // R
                faces[3].NormalIndices.AddRange(new uint[] {0, 3, 7, 4}); // L
                faces[4].NormalIndices.AddRange(new uint[] {3, 2, 6, 7}); // F
                faces[5].NormalIndices.AddRange(new uint[] {1, 0, 4, 5}); // B
            }
            else // Per face Normals
            {
                for (uint i = 0; i < 6; ++i) faces[i].NormalIndices.AddRange(new[] {i, i, i, i});
            }


            // Set Colors
            {
                switch (hints.ColorsType)
                {
                    case ColorsType.ColorOverall:
                    default:
                    {
                        if (colors.Length < 1) throw new Exception("Not enough colors specified");

                        for (uint i = 0; i < 6; ++i) faces[i].ColorIndices.AddRange(new uint[] {0, 0, 0, 0});

                        break;
                    }
                    case ColorsType.ColorPerFace:
                    {
                        if (colors.Length < 6) throw new Exception("Not enough colors specified for per-face coloring");

                        for (uint i = 0; i < 6; ++i) faces[i].ColorIndices.AddRange(new[] {i, i, i, i});

                        break;
                    }
                    case ColorsType.ColorPerVertex:
                    {
                        if (colors.Length < 8)
                            throw new Exception("Not enough colors specified for per-vertex coloring");

                        faces[0].ColorIndices.AddRange(new uint[] {0, 1, 2, 3}); // T
                        faces[1].ColorIndices.AddRange(new uint[] {7, 6, 5, 4}); // B
                        faces[2].ColorIndices.AddRange(new uint[] {2, 1, 5, 6}); // R
                        faces[3].ColorIndices.AddRange(new uint[] {0, 3, 7, 4}); // L
                        faces[4].ColorIndices.AddRange(new uint[] {3, 2, 6, 7}); // F
                        faces[5].ColorIndices.AddRange(new uint[] {1, 0, 4, 5}); // B
                        break;
                    }
                }
            }

            var vertexDataList = new List<T>();

            var inverseTranspose = Matrix4x4.Transpose(Inverse);

            foreach (var face in faces)
                for (var i = 0; i < face.VertexIndices.Count; ++i)
                {
                    var vtx = new T();
                    vtx.SetPosition(Vector3.Transform(vertices[face.VertexIndices[i]], Matrix));
                    vtx.SetNormal(
                        Vector3.Normalize(Vector3.Transform(normals[face.NormalIndices[i]], inverseTranspose)));
                    vtx.SetColor3(colors[face.ColorIndices[i]]);
                    vtx.SetTexCoord2(texcoords[face.TexCoordIndices[i]]);
                    vertexDataList.Add(vtx);
                }

            geometry.VertexData = vertexDataList.ToArray();

            uint[] indices =
            {
                0, 1, 2, 0, 2, 3,
                4, 5, 6, 4, 6, 7,
                8, 9, 10, 8, 10, 11,
                12, 13, 14, 12, 14, 15,
                16, 17, 18, 16, 18, 19,
                20, 21, 22, 20, 22, 23
            };

            geometry.IndexData = indices;

            geometry.VertexLayouts = new List<VertexLayoutDescription>
                {VertexLayoutHelpers.GetLayoutDescription(typeof(T))};

            var pSet = DrawElements<T>.Create(
                geometry,
                PrimitiveTopology.TriangleList,
                (uint) geometry.IndexData.Length,
                _instanceCount,
                0,
                0,
                0);

            geometry.PrimitiveSets.Add(pSet);
        }
    }
}