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

using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Numerics;

namespace Veldrid.SceneGraph
{

    public class PlaneList : List<IPlane> {}



    /// <summary>
    /// Class representing convex clipping volumes made up from planes.
    /// When adding planes, normals should point inward
    /// </summary>
    public class Polytope : IPolytope
    {
        private readonly PlaneList _planeList = new PlaneList();

        private Matrix4x4 _viewProjectionMatrix = Matrix4x4.Identity;
        public Matrix4x4 ViewProjectionMatrix => _viewProjectionMatrix;
        
        public static IPolytope Create()
        {
            return new Polytope();
        }
        
        protected Polytope()
        {
            SetToUnitFrustum();
        }

        public void SetViewProjectionMatrix(Matrix4x4 viewProjectionMatrix)
        {
            _viewProjectionMatrix = viewProjectionMatrix;
        }
        
        public void SetToUnitFrustum(bool withNear=true, bool withFar = true)
        {
            _planeList.Clear();
            _planeList.Add(Plane.Create( 1.0f, 0.0f, 0.0f,1.0f)); // left plane.
            _planeList.Add(Plane.Create(-1.0f, 0.0f, 0.0f,1.0f)); // right plane.
            _planeList.Add(Plane.Create( 0.0f, 1.0f, 0.0f,1.0f)); // bottom plane.
            _planeList.Add(Plane.Create( 0.0f,-1.0f, 0.0f,1.0f)); // top plane.
            if (withNear)
            {
                _planeList.Add(Plane.Create(0.0f, 0.0f, 1.0f, 1.0f)); // near plane
            }

            if (withFar)
            {
                _planeList.Add(Plane.Create(0.0f, 0.0f, -1.0f, 1.0f)); // far plane
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
        
        public bool Contains(IBoundingBox bb)
        {
            if (_planeList.Count == 0) return true;

            foreach (var plane in _planeList)
            {
                var res = plane.Intersect(bb);
                if (res < 0) return false;  // Outside the clipping set
            }

            return true;
        }
        
        public bool Contains(IBoundingBox bb, Matrix4x4 transformMatrix)
        {
            if (_planeList.Count == 0) return true;

            var mvp = Matrix4x4.Multiply(transformMatrix, ViewProjectionMatrix);
            Matrix4x4.Invert(mvp, out var mvpInv);
            
            foreach (var plane in _planeList)
            {
                var isp = Plane.Create(plane);
                isp.Transform(mvpInv);
                var res = isp.Intersect(bb);
                if (res < 0) return false;  // Outside the clipping set
            }

            return true;
        }
    }
}