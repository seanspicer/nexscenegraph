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

using System;
using System.Linq;
using System.Numerics;

namespace Veldrid.SceneGraph.Util
{
    public class LineSegmentIntersector : Intersector
    {
        public class Intersection : IComparable<Intersection>
        {
            public Vector3 _start;
            
            public Vector3 IntersectionPoint { get; private set; }
            
            public Drawable Drawable { get; private set; }
            
            public NodePath NodePath { get; set; }
            
            public float StartToIntersectionDist { get; private set; }

            public Intersection(Vector3 start, Vector3 intersectionPoint, Drawable d, NodePath nodePath)
            {
                _start = start;
                IntersectionPoint = intersectionPoint;
                Drawable = d;
                NodePath = nodePath;
                StartToIntersectionDist = Vector3.Distance(start, intersectionPoint);
            }

            public int CompareTo(Intersection rhs)
            {
                var d1 = StartToIntersectionDist;
                var d2 = rhs.StartToIntersectionDist;
                
                if (d1 < d2)
                {
                    return -1;
                }
                else if (d1 == d2)
                {
                    return 0;
                }
                else if (d1 > d2)
                {
                    return 1;
                }

                throw new Exception("LineSegmentIntersector.Intersection: Comparison Error");
            }
        }

        public SortedMultiSet<Intersection> Intersections = new SortedMultiSet<Intersection>();
        
        protected Vector3 Start { get; set; }
        protected Vector3 End { get; set; }

        protected LineSegmentIntersector Parent { get; set; } = null;
        
        /// <summary>
        /// Constructor for LineSegment Intersector given start and end points in
        /// Model Coordinates
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public LineSegmentIntersector(Vector3 start, Vector3 end)
        {
            Start = start;
            End = end;
        }

        public override void Intersect(IntersectionVisitor iv, Drawable drawable)
        {
            var bb = drawable.GetBoundingBox();
            if (intersects(new BoundingSphere(drawable.GetBoundingBox())))
            {
                var intersection = new Intersection(Start, bb.Center, drawable, iv.NodePath.Copy());
                Intersections.Add(intersection);
            }
        }

        public override bool Enter(Node node)
        {
            return intersects(node.GetBound());
        }

        private bool intersects(BoundingSphere bs)
        {
            if (!bs.Valid()) return true;

            var startToCenter = Start - bs.Center;
            
            double c = startToCenter.LengthSquared()-System.Math.Pow(bs.Radius,2);
            
            if (c<0.0) return true;

            var startToEnd = End-Start;
            double a = startToEnd.LengthSquared();
            
            double b = Vector3.Dot(startToCenter,startToEnd)*2.0;
            
            double d = b*b-4.0*a*c;

            if (d<0.0) return false;

            d = System.Math.Sqrt(d);

            double div = 1.0/(2.0*a);

            double r1 = (-b-d)*div;
            double r2 = (-b+d)*div;

            if (r1<=0.0 && r2<=0.0) return false;

            if (r1>=1.0 && r2>=1.0) return false;

            if (IntersectionLimit == IntersectionLimitModes.LimitNearest && 0 != Intersections.Count())
            {
                if (startToCenter.Length() > Intersections.First().StartToIntersectionDist) return false;
                
                //double ratio = (startToCenter.Length() - bs.Radius) / System.Math.Sqrt(a);
                
                //if (ratio >= Intersections.First().Ratio) return false;
            }

            // passed all the rejection tests so line must intersect bounding sphere, return true.
            return true;
        }

        public override void Leave()
        {
            // Nothing Required for LineSegmentIntersector
        }

        public override void Reset()
        {
            Intersections.Clear();
        }
    }
}