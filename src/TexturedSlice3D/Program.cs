using System;
using System.Collections.Generic;
using System.Numerics;
using Examples.Common;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.AssetPrimitives;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.VertexTypes;
using Veldrid.SceneGraph.Viewer;

namespace TexturedSlice3D
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Bootstrapper.Configure();

            var viewer = SimpleViewer.Create("Textured Cube Scene Graph");
            viewer.SetCameraManipulator(TrackballManipulator.Create());

            var root = Group.Create();

            var scaleXform = MatrixTransform.Create(Matrix4x4.CreateScale(0.75f));

            var cube = CreateCube();
            scaleXform.AddChild(cube);

            root.AddChild(scaleXform);

            viewer.SetSceneData(root);
            viewer.ViewAll();
            viewer.Run();
        }

        private static IGeode CreateCube()
        {
            var geometry = Geometry<Position3TexCoord3Color4>.Create();

            var vertices = new[]
            {
                // Top
                new Position3TexCoord3Color4(new Vector3(-1.0f, +1.0f, -1.0f), new Vector3(0, 1, 0),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new Position3TexCoord3Color4(new Vector3(+1.0f, +1.0f, -1.0f), new Vector3(1, 1, 0),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new Position3TexCoord3Color4(new Vector3(+1.0f, +1.0f, +1.0f), new Vector3(1, 1, 1),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new Position3TexCoord3Color4(new Vector3(-1.0f, +1.0f, +1.0f), new Vector3(0, 1, 1),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                // Bottom                                                             
                new Position3TexCoord3Color4(new Vector3(-1.0f, -1.0f, +1.0f), new Vector3(0, 0, 1),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new Position3TexCoord3Color4(new Vector3(+1.0f, -1.0f, +1.0f), new Vector3(1, 0, 1),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new Position3TexCoord3Color4(new Vector3(+1.0f, -1.0f, -1.0f), new Vector3(1, 0, 0),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new Position3TexCoord3Color4(new Vector3(-1.0f, -1.0f, -1.0f), new Vector3(0, 0, 0),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                // Left                                                               
                new Position3TexCoord3Color4(new Vector3(-1.0f, +1.0f, -1.0f), new Vector3(0, 1,0),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new Position3TexCoord3Color4(new Vector3(-1.0f, +1.0f, +1.0f), new Vector3(0, 1,1),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new Position3TexCoord3Color4(new Vector3(-1.0f, -1.0f, +1.0f), new Vector3(0, 0, 1),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new Position3TexCoord3Color4(new Vector3(-1.0f, -1.0f, -1.0f), new Vector3(0, 0, 0),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                // Right                                                              
                new Position3TexCoord3Color4(new Vector3(+1.0f, +1.0f, +1.0f), new Vector3(1, 1,1),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new Position3TexCoord3Color4(new Vector3(+1.0f, +1.0f, -1.0f), new Vector3(1, 1,0),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new Position3TexCoord3Color4(new Vector3(+1.0f, -1.0f, -1.0f), new Vector3(1, 0,0),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new Position3TexCoord3Color4(new Vector3(+1.0f, -1.0f, +1.0f), new Vector3(1, 0,1),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                // Back                                                               
                new Position3TexCoord3Color4(new Vector3(+1.0f, +1.0f, -1.0f), new Vector3(1, 1, 0),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new Position3TexCoord3Color4(new Vector3(-1.0f, +1.0f, -1.0f), new Vector3(0, 1, 0),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new Position3TexCoord3Color4(new Vector3(-1.0f, -1.0f, -1.0f), new Vector3(0, 0, 0),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new Position3TexCoord3Color4(new Vector3(+1.0f, -1.0f, -1.0f), new Vector3(1, 0, 0),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                // Front                                                              
                new Position3TexCoord3Color4(new Vector3(-1.0f, +1.0f, +1.0f), new Vector3(0, 1, 1),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new Position3TexCoord3Color4(new Vector3(+1.0f, +1.0f, +1.0f), new Vector3(1, 1, 1),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new Position3TexCoord3Color4(new Vector3(+1.0f, -1.0f, +1.0f), new Vector3(1, 0, 1),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new Position3TexCoord3Color4(new Vector3(-1.0f, -1.0f, +1.0f), new Vector3(0, 0, 1),
                    new Vector4(0.0f, 0.0f, 1.0f, 1.0f))
            };

            uint[] indices =
            {
                0, 1, 2, 0, 2, 3,
                4, 5, 6, 4, 6, 7,
                8, 9, 10, 8, 10, 11,
                12, 13, 14, 12, 14, 15,
                16, 17, 18, 16, 18, 19,
                20, 21, 22, 20, 22, 23
            };

            geometry.VertexData = vertices;
            geometry.IndexData = indices;

            geometry.VertexLayouts = new List<VertexLayoutDescription>
            {
                Position3TexCoord3Color4.VertexLayoutDescription
            };

            var pSet = DrawElements<Position3TexCoord3Color4>.Create(
                geometry,
                PrimitiveTopology.TriangleList,
                (uint) geometry.IndexData.Length,
                1,
                0,
                0,
                0);

            geometry.PrimitiveSets.Add(pSet);

            geometry.PipelineState.ShaderSet = Texture3DShader.Instance.ShaderSet;

            var testData = TestData();
            
            geometry.PipelineState.AddTexture(
                Texture3D.Create(testData,
                    1,
                    "SurfaceTexture",
                    "SurfaceSampler"));

            var geode = Geode.Create();
            geode.AddDrawable(geometry);

            return geode;
        }

        private static ProcessedTexture TestData()
        {
            var width = 256;
            var height = 256;
            var depth = 256;
            var rgbaData = new UInt32[width * height * depth];
            for (int i = 0; i < width * height * depth; i++)
            {
                rgbaData[i] = 0xFF0000FF;
            }
            
            var allTexData = new byte[width * height * depth * 4]; // RGBA
            Buffer.BlockCopy(rgbaData, 0, allTexData, 0, allTexData.Length);
            
            var texData = new ProcessedTexture(
                PixelFormat.R8_G8_B8_A8_UNorm, TextureType.Texture3D,
                (uint) width, (uint) height, (uint) depth,
                (uint) 1, 1,
                allTexData);
            return texData;
        }
    }
}