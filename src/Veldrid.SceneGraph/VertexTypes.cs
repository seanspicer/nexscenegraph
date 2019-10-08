using System.Numerics;

namespace Veldrid.SceneGraph
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
    }
}