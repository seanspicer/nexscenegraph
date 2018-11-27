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
    public interface IPlane
    {
        float Nx { get; }
        float Ny { get; }
        float Nz { get; }
        float D { get; }
          
        
        void Transform(Matrix4x4 matrix);
        float Distance(Vector3 v);

        /// <summary>
        /// Intersection test between plane and bounding sphere.
        /// </summary>
        /// <param name="bb"></param>
        /// <returns>
        /// return 1 if the bb is completely above plane,
        /// return 0 if the bb intersects the plane,
        /// return -1 if the bb is completely below the plane.
        /// </returns>
        int Intersect(IBoundingBox bb);
    }
}