//
// Copyright 2018 Sean Spicer 
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
    public interface IPolytope
    {
        Matrix4x4 ViewProjectionMatrix { get; }
        void SetViewProjectionMatrix(Matrix4x4 viewProjectionMatrix);

        void SetToViewProjectionFrustum(
            Matrix4x4 viewProjectionMatrix, 
            bool withNear=true, 
            bool withFar = true);

        bool Contains(IBoundingBox bb);
        bool Contains(IBoundingBox bb, Matrix4x4 transformMatrix);
    }
}