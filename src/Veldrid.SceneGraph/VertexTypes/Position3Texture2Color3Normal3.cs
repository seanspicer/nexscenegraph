using System.Numerics;
using System.Runtime.InteropServices;

namespace Veldrid.SceneGraph.VertexTypes
{
    /// <summary>
    /// Describes a Primitive Element with Position, Texture, Color, and Normal values
    /// </summary>
    public struct Position3Texture2Color3Normal3 : IPrimitiveElement
    {
        public Vector3 Position;
        public Vector2 TexCoord;
        public Vector3 Color;
        public Vector3 Normal;
        
        public Position3Texture2Color3Normal3(Vector3 position, Vector2 texCoord, Vector3 color, Vector3 normal)
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

        public static VertexLayoutDescription VertexLayoutDescription
        {
            get
            {
                return new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("Texture", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                    new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3));

            }
        }
    }
}