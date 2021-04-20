//
// Copyright 2018-2021 Sean Spicer 
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

using System.Numerics;

namespace Veldrid.SceneGraph
{
    public interface IPrimitiveElement
    {
        Vector3 VertexPosition { get; set; }
    }

    public interface ISettablePrimitiveElement : IPrimitiveElement
    {
        bool HasPosition { get; }
        bool HasNormal { get; }
        bool HasTexCoord2 { get; }
        bool HasTexCoord3 { get; }
        bool HasColor3 { get; }
        bool HasColor4 { get; }

        void SetPosition(Vector3 position);
        void SetNormal(Vector3 normal);
        void SetTexCoord2(Vector2 texCoord);
        void SetTexCoord3(Vector3 texCoord);
        void SetColor3(Vector3 color);
        void SetColor4(Vector4 color);
        VertexLayoutDescription GetVertexLayoutDescription();
    }
}