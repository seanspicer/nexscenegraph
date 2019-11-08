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

namespace Veldrid.SceneGraph.VertexTypes
{
    public static class VertexLayoutHelpers
    {
        public static VertexLayoutDescription GetLayoutDescription(Type type)
        {
            if (type == typeof(Position3Texture2Color3Normal3))
            {
                return Position3Texture2Color3Normal3.VertexLayoutDescription;
            }
            else if (type == typeof(Position3Color3))
            {
                return Position3Color3.VertexLayoutDescription;
            }
            else if (type == typeof(Position3TexCoord2))
            {
                return Position3TexCoord2.VertexLayoutDescription;
            }
            
            throw new ArgumentException($"Invalid type {type}.  Cannot get vertex layout description");
        }
    }
}