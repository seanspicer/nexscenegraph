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
using System.Linq;
using System.Numerics;

namespace Veldrid.SceneGraph.Util
{
    public class LineSegmentIntersector : Intersector, ILineSegmentIntersector
    {
        public class Intersection : IComparable<Intersection>
        {
            public Vector3 _start;
            
            public Vector3 IntersectionPoint { get; private set; }
            
            public IDrawable Drawable { get; private set; }
            
            public NodePath NodePath { get; set; }
            
            public float StartToIntersectionDist { get; private set; }

            public Intersection(Vector3 start, Vector3 intersectionPoint, IDrawable d, NodePath nodePath)
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

        public SortedMultiSet<Intersection> Intersections { get; }= new SortedMultiSet<Intersection>();
        
        protected Vector3 Start { get; set; }
        protected Vector3 End { get; set; }

        protected LineSegmentIntersector Parent { get; set; } = null;

        public static ILineSegmentIntersector Create(Vector3 start, Vector3 end)
        {
            return new LineSegmentIntersector(start, end);
        }
        
        /// <summary>
        /// Constructor for LineSegment Intersector given start and end points in
        /// Model Coordinates
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        protected LineSegmentIntersector(Vector3 start, Vector3 end)
        {
            Start = start;
            End = end;
        }
        
        public override Intersector Clone(IIntersectionVisitor iv)
        {
            Matrix4x4 matrix;
            Matrix4x4.Invert(iv.GetModelMatrix(), out matrix);
            
            var lsi = new LineSegmentIntersector(
                matrix.PreMultiply(Start),
                matrix.PreMultiply(End));

            lsi.Parent = this;
            lsi.IntersectionLimit = this.IntersectionLimit;

            return lsi;
        }

        private void InsertIntersection(Intersection intersection)
        {
            if (null == Parent)
            {
                Intersections.Add(intersection);
            }
            else
            {
                Parent.InsertIntersection(intersection);
            }
        }

        public override void Intersect(IIntersectionVisitor iv, IDrawable drawable)
        {
            var bb = drawable.GetBoundingBox();
            if (drawable.IsCullingActive && Intersects(BoundingSphere.Create(drawable.GetBoundingBox())))
            {
                var intersection = new Intersection(Start, bb.Center, drawable, iv.NodePath.Copy());
                InsertIntersection(intersection);
            }
        }

        public override bool Enter(INode node)
        {
            return  !node.IsCullingActive || Intersects(node.GetBound());
        }

        private bool Intersects(IBoundingSphere bs)
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