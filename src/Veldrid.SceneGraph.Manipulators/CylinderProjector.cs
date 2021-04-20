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
    public interface ICylinderProjector : IProjector
    {
        ICylinder Cylinder { get; }

        bool IsPointInFront(IPointerInfo pi, Matrix4x4 localToWorld);
    }

    public class CylinderProjector : Projector, ICylinderProjector
    {
        protected ICylinder _cylinder;
        protected Vector3 _cylinderAxis;
        protected bool _front;

        protected CylinderProjector()
        {
            _cylinder = SceneGraph.Util.Shape.Cylinder.Create();
            _cylinderAxis = Vector3.UnitZ;
            _front = true;
        }

        protected CylinderProjector(ICylinder cylinder)
        {
            SetCylinder(cylinder);
            _front = true;
        }

        public ICylinder Cylinder => _cylinder;

        public override bool Project(IPointerInfo pi, out Vector3 projectedPoint)
        {
            projectedPoint = Vector3.Zero;
            if (null == _cylinder) return false;

            var objectNearPoint = WorldToLocal.PreMultiply(pi.NearPoint);
            var objectFarPoint = WorldToLocal.PreMultiply(pi.FarPoint);

            if (GetCylinderLineIntersection(_cylinder, objectNearPoint, objectFarPoint, out var pp,
                out var dontCare))
            {
                projectedPoint = pp;
                return true;
            }

            return false;
        }

        public bool IsPointInFront(IPointerInfo pi, Matrix4x4 localToWorld)
        {
            if (ComputeClosestPointOnLine(_cylinder.Center, _cylinder.Center + _cylinderAxis,
                pi.GetLocalIntersectionPoint(), out var closestPointOnAxis))
            {
                var perpPoint = pi.GetLocalIntersectionPoint() - closestPointOnAxis;
                if (Vector3.Dot(perpPoint, GetLocalEyeDirection(pi.EyeDir, localToWorld)) < 0.0) return false;
                return true;
            }

            throw new Exception("Cannot compute closest point on line in CylinderProjector.IsPointInFront(...)");
        }

        public static ICylinderProjector Create()
        {
            return new CylinderProjector();
        }

        public static ICylinderProjector Create(ICylinder cylinder)
        {
            return new CylinderProjector(cylinder);
        }

        public void SetCylinder(ICylinder cylinder)
        {
            _cylinder = cylinder;
            _cylinderAxis =
                Vector3.Normalize(Matrix4x4.CreateFromQuaternion(cylinder.Rotation).PreMultiply(Vector3.UnitZ));
        }

        protected bool GetUnitCylinderLineIntersection(
            Vector3 lineStart,
            Vector3 lineEnd,
            out Vector3 isectFront,
            out Vector3 isectBack)
        {
            var dir = Vector3.Normalize(lineEnd - lineStart);

            double a = dir.X * dir.X + dir.Y * dir.Y;
            double b = 2.0f * (lineStart.X * dir.X + lineStart.Y * dir.Y);
            double c = lineStart.X * lineStart.X + lineStart.Y * lineStart.Y - 1;

            var d = b * b - 4 * a * c;
            if (d < 0.0)
            {
                isectFront = Vector3.Zero;
                isectBack = Vector3.Zero;
                return false;
            }

            var dSqroot = System.Math.Sqrt(d);
            double t0, t1;
            if (b > 0.0f)
            {
                t0 = -(2.0f * c) / (dSqroot + b);
                t1 = -(dSqroot + b) / (2.0 * a);
            }
            else
            {
                t0 = 2.0f * c / (dSqroot - b);
                t1 = (dSqroot - b) / (2.0 * a);
            }

            isectFront = lineStart + dir * (float) t0;
            isectBack = lineStart + dir * (float) t1;
            return true;
        }

        protected bool ComputeClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 fromPoint,
            out Vector3 closestPoint)
        {
            var v = lineEnd - lineStart;
            var w = fromPoint - lineStart;

            double c1 = Vector3.Dot(w, v);
            double c2 = Vector3.Dot(v, v);

            var almostZero = 0.000001;
            if (c2 < almostZero)
            {
                closestPoint = Vector3.Zero;
                return false;
            }

            var b = c1 / c2;
            closestPoint = lineStart + v * (float) b;

            return true;
        }

        protected bool GetCylinderLineIntersection(
            ICylinder cylinder,
            Vector3 lineStart,
            Vector3 lineEnd,
            out Vector3 isectFront,
            out Vector3 isectBack)
        {
            // Compute matrix transformation that takes the cylinder to a unit cylinder with Z-axis as it's axis and
            // (0,0,0) as it's center.
            var oneOverRadius = 1.0f / cylinder.Radius;
            var toUnitCylInZ = Matrix4x4.CreateTranslation(-cylinder.Center)
                .PostMultiply(Matrix4x4.CreateScale(oneOverRadius, oneOverRadius, oneOverRadius))
                .PostMultiply(Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(cylinder.Rotation)));

            // Transform the lineStart and lineEnd into the unit cylinder space
            var unitCylLineStart = toUnitCylInZ.PreMultiply(lineStart);
            var unitCylLineEnd = toUnitCylInZ.PreMultiply(lineEnd);

            // Intersect with unit cylinder
            if (GetUnitCylinderLineIntersection(unitCylLineStart, unitCylLineEnd, out var unitCylIsectFront,
                    out var unitCylIsectBack))
                // Transform back from unit cylinder space
                if (Matrix4x4.Invert(toUnitCylInZ, out var invToUnitCylInZ))
                {
                    isectFront = invToUnitCylInZ.PreMultiply(unitCylIsectFront);
                    isectBack = invToUnitCylInZ.PreMultiply(unitCylIsectBack);
                    return true;
                }

            isectFront = Vector3.Zero;
            isectBack = Vector3.Zero;
            return false;
        }
    }
}