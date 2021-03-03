//
// Copyright 2018-2019 Sean Spicer 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Numerics;

namespace Veldrid.SceneGraph.VertexTypes
{
    /// <summary>
    ///     Describes a Primitive Element with Position, Texture, Color, and Normal values
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
            Color = color;
            Normal = normal;
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
                new VertexElementDescription("Texture", VertexElementSemantic.TextureCoordinate,
                    VertexElementFormat.Float2, 12),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate,
                    VertexElementFormat.Float3, 20),
                new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate,
                    VertexElementFormat.Float3, 32));

        public VertexLayoutDescription GetVertexLayoutDescription()
        {
            return VertexLayoutDescription;
        }

        public bool HasPosition => true;
        public bool HasNormal => true;
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
            Normal = normal;
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
            Color = color;
        }

        public void SetColor4(Vector4 color)
        {
            throw new NotImplementedException();
        }
    }
}