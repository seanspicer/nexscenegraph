//
// Copyright (c) 2018 Sean Spicer
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System.Numerics;

namespace Veldrid.SceneGraph
{
    public class Plane
    {
        private System.Numerics.Plane _internalPlane;
        private uint _upperBBCorner = 0;
        private uint _lowerBBCorner = 0;

        public Plane(float nX, float nY, float nZ, float D)
        {
            _internalPlane = System.Numerics.Plane.Normalize(new System.Numerics.Plane(nX, nY, nZ, D));
            
            ComputeBBCorners();
        }

        public Plane(Plane plane)
        {
            _internalPlane = plane._internalPlane;
            _upperBBCorner = plane._upperBBCorner;
            _lowerBBCorner = plane._lowerBBCorner;
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