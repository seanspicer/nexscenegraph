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
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface ILineProjector : IProjector
    {
        ILineSegment LineSegment { get; }

        Vector3 LineStart { get; set; }
        Vector3 LineEnd { get; set; }
    }

    public class LineProjector : Projector, ILineProjector
    {
        protected LineProjector(ILineSegment lineSegment)
        {
            LineSegment = lineSegment;
        }

        public ILineSegment LineSegment { get; protected set; }

        public Vector3 LineStart
        {
            get => LineSegment.Start;
            set => LineSegment.Start = value;
        }

        public Vector3 LineEnd
        {
            get => LineSegment.End;
            set => LineSegment.End = value;
        }

        public override bool Project(IPointerInfo pi, out Vector3 projectedPoint)
        {
            if (null == LineSegment)
            {
                projectedPoint = Vector3.Zero;
                return false;
            }

            // Transform the line to world/object coordinate space
            var objectLine = SceneGraph.LineSegment.Create();
            objectLine.PostMultiply(LineSegment, LocalToWorld);

            // Get the near and far points
            var pointerLine = SceneGraph.LineSegment.Create(pi.NearPoint, pi.FarPoint);

            // Find the closest point
            if (ComputeClosestPoints(objectLine, pointerLine, out var closestPointLine,
                out var closestPointProjWorkingLine))
            {
                var localClosestPtLine = WorldToLocal.PreMultiply(closestPointLine);

                projectedPoint = localClosestPtLine;

                return true;
            }

            projectedPoint = Vector3.Zero;
            return false;
        }

        public static ILineProjector Create(ILineSegment lineSegment)
        {
            return new LineProjector(lineSegment);
        }

        // Computes the closest points (p1 and p2 on line l1 and l2 respectively) between the two lines
        // An explanation of the algorithm can be found at
        // http://www.geometryalgorithms.com/Archive/algorithm_0106/algorithm_0106.htm
        public static bool ComputeClosestPoints(ILineSegment l1, ILineSegment l2, out Vector3 p1, out Vector3 p2)
        {
            p1 = Vector3.Zero;
            p2 = Vector3.Zero;

            var u = Vector3.Normalize(l1.End - l1.Start);
            var v = Vector3.Normalize(l2.End - l2.Start);

            var w0 = l1.Start - l2.Start;

            var a = Vector3.Dot(u, u);
            var b = Vector3.Dot(u, v);
            var c = Vector3.Dot(v, v);
            var d = Vector3.Dot(u, w0);
            var e = Vector3.Dot(v, w0);

            var denominator = a * c - b * b;

            // Test if lines are parallel
            if (denominator == 0.0) return false;

            var sc = (b * e - c * d) / denominator;
            var tc = (a * e - b * d) / denominator;

            p1 = l1.Start + u * sc;
            p2 = l2.Start + v * tc;

            return true;
        }
    }
}