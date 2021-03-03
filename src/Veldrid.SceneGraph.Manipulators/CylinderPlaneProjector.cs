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

using System;
using System.Numerics;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Util.Shape;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface ICylinderPlaneProjector : ICylinderProjector
    {
    }

    public class CylinderPlaneProjector : CylinderProjector, ICylinderPlaneProjector
    {
        protected bool _parallelPlane;
        protected IPlane _plane;
        protected Vector3 _planeLineEnd;
        protected Vector3 _planeLineStart;

        protected CylinderPlaneProjector()
        {
            _parallelPlane = false;
        }


        protected CylinderPlaneProjector(ICylinder cylinder) : base(cylinder)
        {
            _parallelPlane = false;
        }

        public override bool Project(IPointerInfo pi, out Vector3 projectedPoint)
        {
            projectedPoint = Vector3.Zero;
            if (null == _cylinder) return false;

            var objectNearPoint = WorldToLocal.PreMultiply(pi.NearPoint);
            var objectFarPoint = WorldToLocal.PreMultiply(pi.FarPoint);

            // Computes either a plane parallel to cylinder axis oriented to the eye or the plane
            // perpendicular to the cylinder axis if the eye-cylinder angle is close.
            _plane = ComputeIntersectionPlane(pi.EyeDir, LocalToWorld, _cylinderAxis,
                _cylinder, _front, ref _planeLineStart, ref _planeLineEnd,
                ref _parallelPlane);

            // Now find the point of intersection on our newly-calculated plane.
            if (GetPlaneLineIntersection(_plane.AsVector4(), objectNearPoint, objectFarPoint, out var pp))
            {
                projectedPoint = pp;
                return true;
            }

            throw new Exception("Cannot project point in CylinderPlaneProjector.Project(...)");
        }

        public new static ICylinderPlaneProjector Create()
        {
            return new CylinderPlaneProjector();
        }

        public new static ICylinderPlaneProjector Create(ICylinder cylinder)
        {
            return new CylinderPlaneProjector(cylinder);
        }
    }
}