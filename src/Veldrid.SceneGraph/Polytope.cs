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

using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Numerics;

namespace Veldrid.SceneGraph
{

    public class PlaneList : List<Plane> {}
    
    /// <summary>
    /// Class representing convex clipping volumes made up from planes.
    /// When adding planes, normals should point inward
    /// </summary>
    public class Polytope
    {
        private readonly PlaneList _planeList = new PlaneList();

        public Matrix4x4 VPMatrix { get; set; } = Matrix4x4.Identity;
        
        public Polytope()
        {
            SetToUnitFrustum();
        }

        public void SetToUnitFrustum(bool withNear=true, bool withFar = true)
        {
            _planeList.Clear();
            _planeList.Add(new Plane( 1.0f, 0.0f, 0.0f,1.0f)); // left plane.
            _planeList.Add(new Plane(-1.0f, 0.0f, 0.0f,1.0f)); // right plane.
            _planeList.Add(new Plane( 0.0f, 1.0f, 0.0f,1.0f)); // bottom plane.
            _planeList.Add(new Plane( 0.0f,-1.0f, 0.0f,1.0f)); // top plane.
            if (withNear)
            {
                _planeList.Add(new Plane(0.0f, 0.0f, 1.0f, 1.0f)); // near plane
            }

            if (withFar)
            {
                _planeList.Add(new Plane(0.0f, 0.0f, -1.0f, 1.0f)); // far plane
            }
        }

        public void SetToViewProjectionFrustum(
            Matrix4x4 viewProjectionMatrix, 
            bool withNear=true, 
            bool withFar = true)
        {
            SetToUnitFrustum(withNear, withFar);
            foreach (var plane in _planeList)
            {
                plane.Transform(viewProjectionMatrix);
            }
        }
        
        public bool Contains(BoundingBox bb)
        {
            if (_planeList.Count == 0) return true;

            foreach (var plane in _planeList)
            {
                var res = plane.Intersect(bb);
                if (res < 0) return false;  // Outside the clipping set
            }

            return true;
        }
        
        public bool Contains(BoundingBox bb, Matrix4x4 transformMatrix)
        {
            if (_planeList.Count == 0) return true;

            var mvp = Matrix4x4.Multiply(transformMatrix, VPMatrix);
            Matrix4x4.Invert(mvp, out var mvpInv);
            
            foreach (var plane in _planeList)
            {
                var isp = new Plane(plane);
                isp.Transform(mvpInv);
                var res = isp.Intersect(bb);
                if (res < 0) return false;  // Outside the clipping set
            }

            return true;
        }
    }
}