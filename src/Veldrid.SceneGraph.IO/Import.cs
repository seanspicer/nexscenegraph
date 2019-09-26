using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Assimp;

namespace Veldrid.SceneGraph.IO
{
    public struct VertexPositionTextureColorNormal : IPrimitiveElement
    {
        public const uint SizeInBytes = 44;

        public Vector3 Position;
        public Vector2 TexCoord;
        public Vector3 Color;
        public Vector3 Normal;
        
        public VertexPositionTextureColorNormal(Vector3 position, Vector2 texCoord, Vector3 color, Vector3 normal)
        {
            Position = position;
            TexCoord = texCoord;
            Color    = color;
            Normal   = normal;
        }

        public Vector3 VertexPosition
        {
            get => Position;
            set => Position = value;
        }
    }
    
    public class Import
    {
        private const PostProcessSteps DefaultPostProcessSteps =
            PostProcessSteps.FlipWindingOrder | PostProcessSteps.Triangulate | PostProcessSteps.PreTransformVertices
            | PostProcessSteps.CalculateTangentSpace | PostProcessSteps.GenerateSmoothNormals;
        
        

        public IndexFormat IndexFormat { get; private set; } = IndexFormat.UInt32;
        
        public uint IndexCount { get; private set; }
        public uint VertexCount { get; private set; }
        
        public IGeode LoadColladaModel(Stream stream)
        {
            AssimpContext assimpContext = new AssimpContext();
            Scene pScene = assimpContext.ImportFileFromStream(stream, DefaultPostProcessSteps, "dae");
            
            parts.Clear();
            parts.Count = (uint)pScene.Meshes.Count;

            Vector3 scale = new Vector3(1.0f);
            Vector2 uvscale = new Vector2(1.0f);
            Vector3 center = new Vector3(0.0f);
            
            var vertices = new List<VertexPositionTextureColorNormal>();
            var indices = new RawList<uint>();

            VertexCount = 0;
            IndexCount = 0;

            var geometry = Geometry<VertexPositionTextureColorNormal>.Create();
            
            // Load meshes
            for (int i = 0; i < pScene.Meshes.Count; i++)
            {
                var paiMesh = pScene.Meshes[i];

                parts[i] = new ModelPart();
                parts[i].vertexBase = VertexCount;
                parts[i].indexBase = IndexCount;

                VertexCount += (uint)paiMesh.VertexCount;

                var pColor = pScene.Materials[paiMesh.MaterialIndex].ColorDiffuse;

                Vector3D Zero3D = new Vector3D(0.0f, 0.0f, 0.0f);

                for (int j = 0; j < paiMesh.VertexCount; j++)
                {
                    Vector3D pPos = paiMesh.Vertices[j];
                    Vector3D pNormal = paiMesh.Normals[j];
                    Vector3D pTexCoord = paiMesh.HasTextureCoords(0) ? paiMesh.TextureCoordinateChannels[0][j] : Zero3D;
                    Vector3D pTangent = paiMesh.HasTangentBasis ? paiMesh.Tangents[j] : Zero3D;
                    Vector3D pBiTangent = paiMesh.HasTangentBasis ? paiMesh.BiTangents[j] : Zero3D;

                    var vertex = new VertexPositionTextureColorNormal(
                        new Vector3(
                        
                            pPos.X * scale.X + center.X,
                            -pPos.Y * scale.Y + center.Y,
                            pPos.Z * scale.Z + center.Z
                        ), 
                        new Vector2(pTexCoord.X * uvscale.X,pTexCoord.Y * uvscale.Y),
                        new Vector3(pColor.R, pColor.G, pColor.B),
                        new Vector3(pNormal.X, -pNormal.Y, pNormal.Z));
                    
                    vertices.Add(vertex);
                    

                    dim.Max.X = Math.Max(pPos.X, dim.Max.X);
                    dim.Max.Y = Math.Max(pPos.Y, dim.Max.Y);
                    dim.Max.Z = Math.Max(pPos.Z, dim.Max.Z);

                    dim.Min.X = Math.Min(pPos.X, dim.Min.X);
                    dim.Min.Y = Math.Min(pPos.Y, dim.Min.Y);
                    dim.Min.Z = Math.Min(pPos.Z, dim.Min.Z);
                }

                dim.Size = dim.Max - dim.Min;

                parts[i].vertexCount = (uint)paiMesh.VertexCount;

                uint indexBase = (uint)indices.Count;
                for (uint j = 0; j < paiMesh.FaceCount; j++)
                {
                    Face Face = paiMesh.Faces[(int)j];
                    if (Face.IndexCount != 3)
                        continue;
                    indices.Add((uint)(indexBase + Face.Indices[0]));
                    indices.Add((uint)(indexBase + Face.Indices[1]));
                    indices.Add((uint)(indexBase + Face.Indices[2]));
                    parts[i].indexCount += 3;
                    IndexCount += 3;
                }
            }

            geometry.VertexData = vertices.ToArray();
            geometry.IndexData = indices.Items;

            geometry.VertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("UV", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3));
            
            var pSet = DrawElements<VertexPositionTextureColorNormal>.Create(
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
        
        public struct ModelPart
        {
            public uint vertexBase;
            public uint vertexCount;
            public uint indexBase;
            public uint indexCount;
        }

        RawList<ModelPart> parts = new RawList<ModelPart>();

        public struct Dimension
        {
            public Vector3 Min;
            public Vector3 Max;
            public Vector3 Size;
            public Dimension(Vector3 min, Vector3 max) { Min = min; Max = max; Size = new Vector3(); }
        }

        public Dimension dim = new Dimension(new Vector3(float.MaxValue), new Vector3(float.MinValue));

        public struct ModelCreateInfo
        {
            public Vector3 Center;
            public Vector3 Scale;
            public Vector2 UVScale;

            public ModelCreateInfo(Vector3 scale, Vector2 uvScale, Vector3 center)
            {
                Center = center;
                Scale = scale;
                UVScale = uvScale;
            }

            public ModelCreateInfo(float scale, float uvScale, float center)
            {
                Center = new Vector3(center);
                Scale = new Vector3(scale);
                UVScale = new Vector2(uvScale);
            }
        }
    }
}