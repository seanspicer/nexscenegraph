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
    
    public interface ILineSegmentIntersector : IIntersector
    {
        public interface IIntersection : IComparable<IIntersection>
        {
            float StartToIntersectionDist { get; }
            
            Vector3 IntersectionPoint { get; }
            Vector3 WorldIntersectionPoint { get; }

            NodePath NodePath { get; }
            
            IDrawable Drawable { get; }

            Matrix4x4 Matrix { get; }
        }
        
        SortedMultiSet<IIntersection> Intersections { get; }
        
    }
    
    public class LineSegmentIntersector : Intersector, ILineSegmentIntersector
    {
        public class Intersection : IComparable<ILineSegmentIntersector.IIntersection>, ILineSegmentIntersector.IIntersection
        {
            public Vector3 _start;
            
            public Vector3 IntersectionPoint { get; private set; }
            
            public IDrawable Drawable { get; private set; }
            
            public Matrix4x4 Matrix { get; set; } = Matrix4x4.Identity;
            public Vector3 WorldIntersectionPoint => Matrix.PreMultiply(IntersectionPoint);

            public NodePath NodePath { get; private set; }
            
            public float StartToIntersectionDist { get; private set; }

            public Intersection(Vector3 start, Vector3 intersectionPoint, IDrawable d, NodePath nodePath)
            {
                _start = start;
                IntersectionPoint = intersectionPoint;
                Drawable = d;
                NodePath = nodePath;
                StartToIntersectionDist = Vector3.Distance(start, intersectionPoint);
            }

            public int CompareTo(ILineSegmentIntersector.IIntersection rhs)
            {
                var d1 = StartToIntersectionDist;
                var d2 = rhs.StartToIntersectionDist;
                
                if (d1 < d2)
                {
                    return -1;
                }
                else if (System.Math.Abs(d1 - d2) < 1e-6)
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

        public SortedMultiSet<ILineSegmentIntersector.IIntersection> Intersections { get; } = new SortedMultiSet<ILineSegmentIntersector.IIntersection>();
        
        protected Vector3 Start { get; set; }
        protected Vector3 End { get; set; }

        protected LineSegmentIntersector Parent { get; set; } = null;

        /** Construct a LineSegmentIntersector that runs between the specified start and end points in MODEL coordinates. */
        public static ILineSegmentIntersector Create(Vector3 start, Vector3 end)
        {
            return new LineSegmentIntersector(start, end);
        }
        
        /** Convenience constructor for supporting picking in WINDOW, or PROJECTION coordinates
          * In WINDOW coordinates creates a start value of (x,y,0) and end value of (x,y,1).
          * In PROJECTION coordinates (clip space cube) creates a start value of (x,y,-1) and end value of (x,y,1).
          * In VIEW and MODEL coordinates creates a start value of (x,y,0) and end value of (x,y,1).*/
        public static ILineSegmentIntersector Create(IIntersector.CoordinateFrameMode cf, double x, double y)
        {
            return new LineSegmentIntersector(cf, x, y);
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

        protected LineSegmentIntersector(
            IIntersector.CoordinateFrameMode cf,
            Vector3 start,
            Vector3 end,
            LineSegmentIntersector parent = null,
            IIntersector.IntersectionLimitModes intersectionLimit = IIntersector.IntersectionLimitModes.NoLimit) :
            base(cf, intersectionLimit)
        {
            Parent = parent;
            Start = start;
            End = end;
        }

        protected LineSegmentIntersector(IIntersector.CoordinateFrameMode cf, double x, double y) 
        : base(cf)
        {
            Parent = null;
            switch (cf)
            {
                case IIntersector.CoordinateFrameMode.Projection:
                {
                    Start = new Vector3((float)x, (float)y, -1.0f);
                    End = new Vector3((float)x, (float)y, 1.0f);
                    break;
                }
                case IIntersector.CoordinateFrameMode.Window:
                case IIntersector.CoordinateFrameMode.Model:
                case IIntersector.CoordinateFrameMode.View: 
                {
                    Start = new Vector3((float)x, (float)y, -1.0f);
                    End = new Vector3((float)x, (float)y, 1.0f);
                    break;
                }
            }
        }
        
        public override IIntersector Clone(IIntersectionVisitor iv)
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

        public void InsertIntersection(Intersection intersection)
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

            if (IntersectionLimit == IIntersector.IntersectionLimitModes.LimitNearest && 0 != Intersections.Count())
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