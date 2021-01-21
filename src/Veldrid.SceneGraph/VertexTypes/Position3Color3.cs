using System.Numerics;

namespace Veldrid.SceneGraph.VertexTypes
{ 
    /// <summary>
    /// Describes a Primitive Element with Position and Color values
    /// </summary>
    public struct Position3Color3 : ISettablePrimitiveElement
    {
        public Vector3 Position;
        public Vector3 Color;
        
        public Position3Color3(Vector3 position, Vector3 color)
        {
            Position = position;
            Color    = color;
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
                    VertexElementFormat.Float3));

        public VertexLayoutDescription GetVertexLayoutDescription()
        {
            return VertexLayoutDescription;
        }

        public bool HasPosition => true;
        public bool HasNormal => false;
        public bool HasTexCoord2 => false;
        public bool HasTexCoord3 => false;
        public bool HasColor3 => true;
        public bool HasColor4 => false;
        public void SetPosition(Vector3 position)
        {
            Position = position;
        }

        public void SetNormal(Vector3 normal)
        {
            throw new System.NotImplementedException();
        }

        public void SetTexCoord2(Vector2 texCoord)
        {
            throw new System.NotImplementedException();
        }
        
        public void SetTexCoord3(Vector3 texCoord)
        {
            throw new System.NotImplementedException();
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