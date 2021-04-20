using System;
using System.Numerics;

namespace Veldrid.SceneGraph.VertexTypes
{
    /// <summary>
    ///     Describes a Primitive Element with Position and Color values
    /// </summary>
    public struct Position3TexCoord2 : ISettablePrimitiveElement
    {
        public Vector3 Position;
        public Vector2 TexCoord;

        public Position3TexCoord2(Vector3 position, Vector2 texCood)
        {
            Position = position;
            TexCoord = texCood;
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
                    VertexElementFormat.Float2, 12));

        public VertexLayoutDescription GetVertexLayoutDescription()
        {
            return VertexLayoutDescription;
        }

        public bool HasPosition => true;
        public bool HasNormal => false;
        public bool HasTexCoord2 => true;
        public bool HasTexCoord3 => true;
        public bool HasColor3 => true;
        public bool HasColor4 => false;

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
            TexCoord = texCoord;
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
            throw new NotImplementedException();
        }
    }
}