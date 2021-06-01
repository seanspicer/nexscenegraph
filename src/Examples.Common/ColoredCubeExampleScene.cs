using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.VertexTypes;

namespace Examples.Common
{
    public class ColoredCubeExampleScene
    {
        public static IGroup Build()
        {
            var geometry = Geometry<Position3Color4>.Create();

            // TODO - make this a color index cube
            Vector3[] cubeVertices =
            {
                new Vector3(1.0f, 1.0f, -1.0f), // (0) Back top right  
                new Vector3(-1.0f, 1.0f, -1.0f), // (1) Back top left
                new Vector3(1.0f, 1.0f, 1.0f), // (2) Front top right
                new Vector3(-1.0f, 1.0f, 1.0f), // (3) Front top left
                new Vector3(1.0f, -1.0f, -1.0f), // (4) Back bottom right
                new Vector3(-1.0f, -1.0f, -1.0f), // (5) Back bottom left
                new Vector3(1.0f, -1.0f, 1.0f), // (6) Front bottom right
                new Vector3(-1.0f, -1.0f, 1.0f) // (7) Front bottom left
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

            uint[] cubeIndices = {3, 2, 7, 6, 4, 2, 0, 3, 1, 7, 5, 4, 1, 0};
            ushort[] colorIndices = {0, 0, 4, 1, 1, 2, 2, 3, 3, 4, 5, 5};

            var cubeTriangleVertices = new List<Position3Color4>();
            var cubeTriangleIndices = new List<uint>();

            for (var i = 0; i < cubeIndices.Length - 2; ++i)
            {
                if (0 == i % 2)
                {
                    cubeTriangleVertices.Add(new Position3Color4(cubeVertices[cubeIndices[i]],
                        faceColors[colorIndices[i]]));
                    cubeTriangleVertices.Add(new Position3Color4(cubeVertices[cubeIndices[i + 1]],
                        faceColors[colorIndices[i]]));
                    cubeTriangleVertices.Add(new Position3Color4(cubeVertices[cubeIndices[i + 2]],
                        faceColors[colorIndices[i]]));
                }
                else
                {
                    cubeTriangleVertices.Add(new Position3Color4(cubeVertices[cubeIndices[i + 1]],
                        faceColors[colorIndices[i]]));
                    cubeTriangleVertices.Add(new Position3Color4(cubeVertices[cubeIndices[i]],
                        faceColors[colorIndices[i]]));
                    cubeTriangleVertices.Add(new Position3Color4(cubeVertices[cubeIndices[i + 2]],
                        faceColors[colorIndices[i]]));
                }

                cubeTriangleIndices.Add((uint) (3 * i));
                cubeTriangleIndices.Add((uint) (3 * i + 1));
                cubeTriangleIndices.Add((uint) (3 * i + 2));
            }

            geometry.VertexData = cubeTriangleVertices.ToArray();

            geometry.IndexData = cubeTriangleIndices.ToArray();

            geometry.VertexLayouts = new List<VertexLayoutDescription>
            {
                Position3Color4.VertexLayoutDescription
            };

            var pSet = DrawElements<Position3Color4>.Create(
                geometry,
                PrimitiveTopology.TriangleList,
                (uint) geometry.IndexData.Length,
                1,
                0,
                0,
                0);

            geometry.PrimitiveSets.Add(pSet);

            geometry.PipelineState.ShaderSet = Vertex3Color4Shader.Instance.ShaderSet;

            var geode = Geode.Create();
            geode.AddDrawable(geometry);

            return geode;
        }
    }
}