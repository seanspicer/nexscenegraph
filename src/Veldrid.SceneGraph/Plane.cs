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
using System.Xml.Schema;

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
    
    public class Plane : IPlane
    {
        private System.Numerics.Plane _internalPlane;
        private uint _upperBBCorner = 0;
        private uint _lowerBBCorner = 0;

        public float Nx => _internalPlane.Normal.X;
        public float Ny => _internalPlane.Normal.Y;
        public float Nz => _internalPlane.Normal.Z;
        public float D => _internalPlane.D;
        
        public static IPlane Create(float nX, float nY, float nZ, float D)
        {
            return new Plane(nX, nY, nZ, D);
        }

        public static IPlane Create(IPlane other)
        {
            return new Plane(other.Nx, other.Ny, other.Nz, other.D);
        }

        protected Plane(float nX, float nY, float nZ, float D)
        {
            _internalPlane = System.Numerics.Plane.Normalize(new System.Numerics.Plane(nX, nY, nZ, D));
            
            ComputeBBCorners();
        }

        public void Transform(Matrix4x4 matrix)
        {
            _internalPlane = System.Numerics.Plane.Normalize(System.Numerics.Plane.Transform(_internalPlane, matrix));
            ComputeBBCorners();
        }

        private void ComputeBBCorners()
        {
            _upperBBCorner = (_internalPlane.Normal.X >= 0.0f ? 1 : (uint) 0) |
                             (_internalPlane.Normal.Y >= 0.0f ? 2 : (uint) 0) |
                             (_internalPlane.Normal.Z >= 0.0f ? 4 : (uint) 0);
            
            _lowerBBCorner = (~_upperBBCorner)&7;
        }

        public float Distance(Vector3 v)
        {
            return _internalPlane.Normal.X * v.X +
                   _internalPlane.Normal.Y * v.Y +
                   _internalPlane.Normal.Z * v.Z +
                   _internalPlane.D;
        }

        /// <summary>
        /// Intersection test between plane and bounding sphere.
        /// </summary>
        /// <param name="bb"></param>
        /// <returns>
        /// return 1 if the bb is completely above plane,
        /// return 0 if the bb intersects the plane,
        /// return -1 if the bb is completely below the plane.
        /// </returns>
        public int Intersect(IBoundingBox bb)
        {
            var lowerBBCorner = bb.Corner(_lowerBBCorner);
            var distLower = Distance(lowerBBCorner);
            
            // If lowest point above plane than all above.
            if (Distance(bb.Corner(_lowerBBCorner)) > 0.0f) return 1;
            
            var upperBBCorner = bb.Corner(_upperBBCorner);
            var distUpper = Distance(upperBBCorner);
            
            // If highest point is below plane then all below.
            if (Distance(bb.Corner(_upperBBCorner)) < 0.0f) return -1;
            
            // Otherwise, must be crossing a plane
            return 0;
        }
    }
}