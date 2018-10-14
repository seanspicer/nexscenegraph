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
            public double Ratio;
            public uint PrimitiveIndex;

            public Intersection()
            {
                Ratio = -1;
                PrimitiveIndex = 0;
            }

            public int CompareTo(Intersection rhs)
            {
                if (Ratio < rhs.Ratio)
                {
                    return -1;
                }
                else if (Ratio == rhs.Ratio)
                {
                    return 0;
                }
                else if (Ratio > rhs.Ratio)
                {
                    return 1;
                }

                throw new Exception("LineSegmentIntersector.Intersection: Comparison Error");
            }
        }

        protected SortedMultiSet<Intersection> Intersections = new SortedMultiSet<Intersection>();
        
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
            throw new System.NotImplementedException();
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
                double ratio = (startToCenter.Length() - bs.Radius) / System.Math.Sqrt(a);
                if (ratio >= Intersections.First().Ratio) return false;
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