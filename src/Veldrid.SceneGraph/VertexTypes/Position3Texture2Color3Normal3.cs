using System;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Veldrid.SceneGraph.VertexTypes
{
    public static class VertexLayoutHelpers
    {
        public static VertexLayoutDescription GetLayoutDescription(Type type)
        {
            if (type == typeof(Position3Texture2Color3Normal3))
            {
                return new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("Texture", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                    new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3));
            }

            throw new ArgumentException($"Invalid type {type}.  Cannot get vertex layout description");
        }
    }
    
    /// <summary>
    /// Describes a Primitive Element with Position, Texture, Color, and Normal values
    /// </summary>
    public struct Position3Texture2Color3Normal3 : ISettablePrimitiveElement
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

        public VertexLayoutDescription GetVertexLayoutDescription()
        {
            return VertexLayoutDescription;
        }

        public bool HasPosition => true;
        public bool HasNormal => true;
        public bool HasTexCoord => true;
        public bool HasColor3 => true;
        public bool HasColor4 => false;
        public void SetPosition(Vector3 position)
        {
            Position = position;
        }

        public void SetNormal(Vector3 normal)
        {
            Normal = normal;
        }

        public void SetTexCoord(Vector2 texCoord)
        {
            TexCoord = texCoord;
        }

        public void SetColor3(Vector3 color)
        {
            Color = color;
        }
        
        public void SetColor4(Vector4 color)
        {
            throw new System.NotImplementedException();
        }
    }
}