using System;
using System.Numerics;

namespace Veldrid.SceneGraph.VertexTypes
{
    public struct Position3Color4 : ISettablePrimitiveElement
    {
        public Vector3 Position;
        public Vector4 Color;

        public Position3Color4(Vector3 position, Vector4 color)
        {
            Position = position;
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
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate,
                    VertexElementFormat.Float4, 12));

        public VertexLayoutDescription GetVertexLayoutDescription()
        {
            return VertexLayoutDescription;
        }

        public bool HasPosition => true;
        public bool HasNormal => false;
        public bool HasTexCoord2 => false;
        public bool HasTexCoord3 => false;
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
            throw new NotImplementedException();
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