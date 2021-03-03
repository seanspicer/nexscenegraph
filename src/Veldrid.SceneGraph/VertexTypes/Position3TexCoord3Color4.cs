using System;
using System.Numerics;

namespace Veldrid.SceneGraph.VertexTypes
{
    public struct Position3TexCoord3Color4 : ISettablePrimitiveElement
    {
        public Vector3 Position;
        public Vector3 TexCoord;
        public Vector4 Color;

        public Position3TexCoord3Color4(Vector3 position, Vector3 texCoord, Vector4 color)
        {
            Position = position;
            TexCoord = texCoord;
            Color = color;
        }

        public Vector3 VertexPosition
        {
            get => Position;
            set => Position = value;
        }

        public static VertexLayoutDescription VertexLayoutDescription =>
            new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate,
                    VertexElementFormat.Float3),
                new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate,
                    VertexElementFormat.Float3, 12),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate,
                    VertexElementFormat.Float4, 24));

        public VertexLayoutDescription GetVertexLayoutDescription()
        {
            return VertexLayoutDescription;
        }

        public bool HasPosition => true;
        public bool HasNormal => false;
        public bool HasTexCoord2 => false;
        public bool HasTexCoord3 => true;
        public bool HasColor3 => false;
        public bool HasColor4 => true;

        public void SetPosition(Vector3 position)
        {
            Position = position;
        }

        public void SetNormal(Vector3 normal)
        {
            throw new NotImplementedException();
        }

        public void SetTexCoord2(Vector2 texCoord)
        {
            throw new NotImplementedException();
        }

        public void SetTexCoord3(Vector3 texCoord)
        {
            TexCoord = texCoord;
        }

        public void SetColor3(Vector3 color)
        {
            throw new NotImplementedException();
        }

        public void SetColor4(Vector4 color)
        {
            Color = color;
        }
    }
}