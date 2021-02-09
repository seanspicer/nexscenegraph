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
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using SharpDX.Direct3D11;

namespace Veldrid.SceneGraph.Util
{
    internal class Settings
    {
        public LineSegmentIntersector _lineSegmentIntersector { get; set; } = null;
        public IIntersectionVisitor _iv { get; set; } = null;
        public IDrawable _drawable { get; set; } = null;
        public Vector3[] _vertices { get; set; } = null;
        public bool _limitOneIntersection { get; set; } = false;
    }

    internal interface IIntersectFunctor
    {
        
    }
    
    internal class LineSegmentIntersectFunctorDelegate : IPrimitiveFunctorDelegate, IIntersectFunctor
    {
        public Settings _settings { get; set; } = null;
        public uint _primitiveIndex { get; set; } = 0;
        public Vector3 _start { get; set; } = Vector3.Zero;
        public Vector3 _end { get; set; } = Vector3.Zero;
        public Stack<Tuple<Vector3, Vector3>> _startEndStack { get; set; } = new Stack<Tuple<Vector3, Vector3>>();
        public Vector3 _d { get; set; }
        public float _length { get; set; } = 0;
        public float _inverse_length { get; set; } = 0;
        public Vector3 _d_invX { get; set; }
        public Vector3 _d_invY { get; set; }
        public Vector3 _d_invZ { get; set; }
        public bool _hit { get; set; } = false;

        internal void Set(Vector3 s, Vector3 e, Settings settings)
        {
            _settings = settings;
            _start = s;
            _end = e;
            
            _d = e - s;
            _length = _d.Length();
            _inverse_length = (_length!=0.0) ? 1.0f/_length : 0.0f;
            _d *= _inverse_length;

            _d_invX = _d.X!=0.0 ? _d/_d.X : Vector3.Zero;
            _d_invY = _d.Y!=0.0 ? _d/_d.Y : Vector3.Zero;
            _d_invZ = _d.Z!=0.0 ? _d/_d.Z : Vector3.Zero;
        }

        bool enter(IBoundingBox bb)
        {
            var startend = _startEndStack.Peek();
            var s = startend.Item1;
            var e = startend.Item2;
            
            // compare s and e against the xMin to xMax range of bb.
            if (s.X<=e.X)
            {

                // trivial reject of segment wholely outside.
                if (e.X<bb.XMin) return false;
                if (s.X>bb.XMax) return false;

                if (s.X<bb.XMin)
                {
                    // clip s to xMin.
                    s = s+_d_invX*(bb.XMin-s.X);
                }

                if (e.X>bb.XMax)
                {
                    // clip e to xMax.
                    e = s+_d_invX*(bb.XMax-s.X);
                }
            }
            else
            {
                if (s.X<bb.XMin) return false;
                if (e.X>bb.XMax) return false;

                if (e.X<bb.XMin)
                {
                    // clip s to xMin.
                    e = s+_d_invX*(bb.XMin-s.X);
                }

                if (s.X>bb.XMax)
                {
                    // clip e to xMax.
                    s = s+_d_invX*(bb.XMax-s.X);
                }
            }

            // compare s and e against the yMin to yMax range of bb.
            if (s.Y<=e.Y)
            {

                // trivial reject of segment wholely outside.
                if (e.Y<bb.YMin) return false;
                if (s.Y>bb.YMax) return false;

                if (s.Y<bb.YMin)
                {
                    // clip s to yMin.
                    s = s+_d_invY*(bb.YMin-s.Y);
                }

                if (e.Y>bb.YMax)
                {
                    // clip e to yMax.
                    e = s+_d_invY*(bb.YMax-s.Y);
                }
            }
            else
            {
                if (s.Y<bb.YMin) return false;
                if (e.Y>bb.YMax) return false;

                if (e.Y<bb.YMin)
                {
                    // clip s to yMin.
                    e = s+_d_invY*(bb.YMin-s.Y);
                }

                if (s.Y>bb.YMax)
                {
                    // clip e to yMax.
                    s = s+_d_invY*(bb.YMax-s.Y);
                }
            }

            // compare s and e against the zMin to zMax range of bb.
            if (s.Z<=e.Z)
            {

                // trivial reject of segment wholely outside.
                if (e.Z<bb.ZMin) return false;
                if (s.Z>bb.ZMax) return false;

                if (s.Z<bb.ZMin)
                {
                    // clip s to zMin.
                    s = s+_d_invZ*(bb.ZMin-s.Z);
                }

                if (e.Z>bb.ZMax)
                {
                    // clip e to zMax.
                    e = s+_d_invZ*(bb.ZMax-s.Z);
                }
            }
            else
            {
                if (s.Z<bb.ZMin) return false;
                if (e.Z>bb.ZMax) return false;

                if (e.Z<bb.ZMin)
                {
                    // clip s to zMin.
                    e = s+_d_invZ*(bb.ZMin-s.Z);
                }

                if (s.Z>bb.ZMax)
                {
                    // clip e to zMax.
                    s = s+_d_invZ*(bb.ZMax-s.Z);
                }
            }

            // OSG_NOTICE<<"clampped segment "<<s<<" "<<e<<std::endl;
            _startEndStack.Push(startend);

            return true;
        }
        
        void leave()
        {
            // OSG_NOTICE<<"leave() "<<_startEndStack.size()<<std::endl;
            _startEndStack.Pop();
        }
        
        public void Handle(Vector3 v0, Vector3 v1, Vector3 v2, bool treatVertexDataAsTemporary)
        {
            Intersect(v0, v1, v2);
            ++_primitiveIndex;
        }

        private void Intersect(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            if (_settings._limitOneIntersection && _hit) return;
            
            var T = _start - v0;
            var E2 = v2 - v0;
            var E1 = v1 - v0;

            var P =  Vector3.Cross(_d, E2);

            var det = Vector3.Dot(P, E1);

            float r,r0,r1,r2;

            const float epsilon = 1e-10f;
            if (det>epsilon)
            {
                var u = Vector3.Dot(P,T);
                if (u<0.0 || u>det) return;

                var Q = Vector3.Cross(T,E1);
                var v = Vector3.Dot(Q, _d);
                if (v<0.0 || v>det) return;

                if ((u+v)> det) return;

                var inv_det = 1.0f/det;
                var t = Vector3.Dot(Q, E2)*inv_det;
                if (t<0.0 || t>_length) return;

                u *= inv_det;
                v *= inv_det;

                r0 = 1.0f-u-v;
                r1 = u;
                r2 = v;
                r = t * _inverse_length;
            }
            else if (det<-epsilon)
            {
                var u = Vector3.Dot(P,T);
                if (u>0.0 || u<det) return;

                var Q = Vector3.Cross(T,E1);
                var v = Vector3.Dot(Q,_d);
                if (v>0.0 || v<det) return;

                if ((u+v) < det) return;

                var inv_det = 1.0f/det;
                var t = Vector3.Dot(Q,E2)*inv_det;
                if (t<0.0 || t>_length) return;

                u *= inv_det;
                v *= inv_det;

                r0 = 1.0f-u-v;
                r1 = u;
                r2 = v;
                r = t * _inverse_length;
            }
            else
            {
                return;
            }

            // Remap ratio into the range of LineSegment
            var lsStart = _settings._lineSegmentIntersector.Start;
            var lsEnd = _settings._lineSegmentIntersector.End;
            var remap_ratio =  (((_start - lsStart).Length() + r*_length)/(lsEnd - lsStart).Length());

            var inVal = lsStart*(1.0f - remap_ratio) + lsEnd*remap_ratio; // == v0*r0 + v1*r1 + v2*r2;
            var normal = Vector3.Normalize(Vector3.Cross(E1,E2));

            LineSegmentIntersector.Intersection hit = new LineSegmentIntersector.Intersection();
            hit.Start = _settings._lineSegmentIntersector.Start;
            hit.Ratio = remap_ratio;
            hit.Matrix = _settings._iv.GetModelMatrix();
            hit.NodePath = _settings._iv.NodePath.Copy();
            hit.Drawable = _settings._drawable;
            hit.PrimitiveIndex = _primitiveIndex;
            hit.LocalIntersectionPoint = inVal;
            hit.LocalIntersectionNormal = normal;
            
            if (null != _settings._vertices)
            {
                var first = (_settings._vertices.First());

                if (r0!=0.0f)
                {
                    var idx = Array.IndexOf(_settings._vertices, v0);
                    hit.IndexList.Add((uint) idx);
                    hit.RatioList.Add(r0);
                }

                if (r1!=0.0f)
                {
                    var idx = Array.IndexOf(_settings._vertices, v1);
                    hit.IndexList.Add((uint) idx);
                    hit.RatioList.Add(r1);
                }

                if (r2!=0.0f)
                {
                    var idx = Array.IndexOf(_settings._vertices, v2);
                    hit.IndexList.Add((uint) idx);
                    hit.RatioList.Add(r2);
                }
            }

            _settings._lineSegmentIntersector.InsertIntersection(hit);
            _hit = true;
        }
    }
    
    public interface ILineSegmentIntersector : IIntersector
    {
        public interface IIntersection : IComparable<IIntersection>
        {
            float StartToIntersectionDist { get; }
            
            Vector3 LocalIntersectionPoint { get; }
            Vector3 WorldIntersectionPoint { get; }

            NodePath NodePath { get; }
            
            IDrawable Drawable { get; }

            Matrix4x4 Matrix { get; }
        }
        
        Vector3 Start { get; }
        Vector3 End { get; }
        
        SortedMultiSet<IIntersection> Intersections { get; }
        
    }
    
    public class LineSegmentIntersector : Intersector, ILineSegmentIntersector
    {
        public class Intersection : IComparable<ILineSegmentIntersector.IIntersection>, ILineSegmentIntersector.IIntersection
        {
            private Vector3 _start;

            public Vector3 Start
            {
                get => _start;
                set
                {
                    _start = value;
                    var worldStart = Matrix.PreMultiply(_start);
                    StartToIntersectionDist = Vector3.Distance(worldStart, WorldIntersectionPoint);
                }
            }

            private Vector3 _localIntersectionPoint = Vector3.Zero;
            public Vector3 LocalIntersectionPoint
            {
                get => _localIntersectionPoint;
                internal set
                {
                    _localIntersectionPoint = value;
                    var worldStart = Matrix.PreMultiply(_start);
                    StartToIntersectionDist = Vector3.Distance(worldStart, WorldIntersectionPoint);
                }
            }

            public Vector3 LocalIntersectionNormal { get; internal set; }
            
            public IDrawable Drawable { get; internal set; }
            
            public Matrix4x4 Matrix { get; set; } = Matrix4x4.Identity;
            public Vector3 WorldIntersectionPoint => Matrix.PreMultiply(LocalIntersectionPoint);

            public NodePath NodePath { get; internal set; }
            
            public float StartToIntersectionDist { get; internal set; }

            public float Ratio { get; set; }
            public uint PrimitiveIndex { get; set; }

            public List<uint> IndexList { get; set; } = new List<uint>();
            public List<float> RatioList { get; set; } = new List<float>();
            
            internal Intersection()
            {
                Ratio = -1;
                PrimitiveIndex = 0;

            }
            
            internal Intersection(Vector3 start, Vector3 localIntersectionPoint, IDrawable d, NodePath nodePath)
            {
                _start = start;
                LocalIntersectionPoint = localIntersectionPoint;
                Drawable = d;
                NodePath = nodePath;
                StartToIntersectionDist = Vector3.Distance(start, localIntersectionPoint);
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
        
        public Vector3 Start { get; set; }
        public Vector3 End { get; set; }

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
            if (ReachedLimit()) return;

            var s = Start;
            var e = End;
            if(drawable.CullingActive && !IntersectAndClip(ref s, ref e, drawable.GetBoundingBox()))
            {
                return;
            }
            
            Intersect(iv, drawable, s, e);
        }

        protected void Intersect(IIntersectionVisitor iv, IDrawable drawable, Vector3 s, Vector3 e)
        {
            if (ReachedLimit()) return;

            Veldrid.SceneGraph.Util.Settings settings = new Settings();
            settings._lineSegmentIntersector = this;
            settings._iv = iv;
            settings._drawable = drawable;
            settings._limitOneIntersection = (IntersectionLimit == IIntersector.IntersectionLimitModes.LimitOnePerDrawable || IntersectionLimit == IIntersector.IntersectionLimitModes.LimitOne);

            if (drawable is IGeometry geometry)
            {
                settings._vertices = geometry.GetVertexArray();
            }

            var kdTree = iv.UseKdTreeWhenAvailable ? (drawable.Shape as IKdTree) : null;

            if (PrecisionHint == IIntersector.PrecisionHintTypes.UseDoubleCalculations)
            {
                throw new NotImplementedException();
            }
            else
            {
                var intersectFunctor = new LineSegmentIntersectFunctorDelegate();
                intersectFunctor.Set(s, e, settings);
                var intersectPrimitiveFunctor = TemplatePrimitiveFunctor.Create(intersectFunctor);

                if (null != kdTree)
                {
                    kdTree.Intersect(intersectFunctor, kdTree.GetNode(0));
                }
                else
                {
                    drawable.Accept(intersectPrimitiveFunctor);
                }
            }
        }

        public override bool Enter(INode node)
        {
            if (ReachedLimit()) return false;
            
            return  !node.IsCullingActive || Intersects(node.GetBound());
        }
        
        public override void Leave()
        {
            // Nothing Required for LineSegmentIntersector
        }

        public override void Reset()
        {
            Intersections.Clear();
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

        protected bool IntersectAndClip(ref Vector3 s, ref Vector3 e, IBoundingBox bbInput)
        {
            var bb_min = bbInput.Min;
            var bb_max = bbInput.Max;

            var epsilon = 1e-5f;

            // compare s and e against the xMin to xMax range of bb.
            if (s.X<=e.X)
            {
                // trivial reject of segment wholely outside.
                if (e.X<bb_min.X) return false;
                if (s.X>bb_max.X) return false;

                if (s.X<bb_min.X)
                {
                    // clip s to xMin.
                    var r = (bb_min.X-s.X)/(e.X-s.X) - epsilon;
                    if (r>0.0) s = s + (e-s)*r;
                }

                if (e.X>bb_max.X)
                {
                    // clip e to xMax.
                    var r = (bb_max.X-s.X)/(e.X-s.X) + epsilon;
                    if (r<1.0) e = s+(e-s)*r;
                }
            }
            else
            {
                if (s.X<bb_min.X) return false;
                if (e.X>bb_max.X) return false;

                if (e.X<bb_min.X)
                {
                    // clip e to xMin.
                    var r = (bb_min.X-e.X)/(s.X-e.X) - epsilon;
                    if (r>0.0) e = e + (s-e)*r;
                }

                if (s.X>bb_max.X)
                {
                    // clip s to xMax.
                    var r = (bb_max.X-e.X)/(s.X-e.X) + epsilon;
                    if (r<1.0) s = e + (s-e)*r;
                }
            }

            // compare s and e against the yMin to yMax range of bb.
            if (s.Y<=e.Y)
            {
                // trivial reject of segment wholely outside.
                if (e.Y<bb_min.Y) return false;
                if (s.Y>bb_max.Y) return false;

                if (s.Y<bb_min.Y)
                {
                    // clip s to yMin.
                    var r = (bb_min.Y-s.Y)/(e.Y-s.Y) - epsilon;
                    if (r>0.0) s = s + (e-s)*r;
                }

                if (e.Y>bb_max.Y)
                {
                    // clip e to yMax.
                    var r = (bb_max.Y-s.Y)/(e.Y-s.Y) + epsilon;
                    if (r<1.0) e = s+(e-s)*r;
                }
            }
            else
            {
                if (s.Y<bb_min.Y) return false;
                if (e.Y>bb_max.Y) return false;

                if (e.Y<bb_min.Y)
                {
                    // clip e to yMin.
                    var r = (bb_min.Y-e.Y)/(s.Y-e.Y) - epsilon;
                    if (r>0.0) e = e + (s-e)*r;
                }

                if (s.Y>bb_max.Y)
                {
                    // clip s to yMax.
                    var r = (bb_max.Y-e.Y)/(s.Y-e.Y) + epsilon;
                    if (r<1.0) s = e + (s-e)*r;
                }
            }

            // compare s and e against the zMin to zMax range of bb.
            if (s.Z<=e.Z)
            {
                // trivial reject of segment wholely outside.
                if (e.Z<bb_min.Z) return false;
                if (s.Z>bb_max.Z) return false;

                if (s.Z<bb_min.Z)
                {
                    // clip s to zMin.
                    var r = (bb_min.Z-s.Z)/(e.Z-s.Z) - epsilon;
                    if (r>0.0) s = s + (e-s)*r;
                }

                if (e.Z>bb_max.Z)
                {
                    // clip e to zMax.
                    var r = (bb_max.Z-s.Z)/(e.Z-s.Z) + epsilon;
                    if (r<1.0) e = s+(e-s)*r;
                }
            }
            else
            {
                if (s.Z<bb_min.Z) return false;
                if (e.Z>bb_max.Z) return false;

                if (e.Z<bb_min.Z)
                {
                    // clip e to zMin.
                    var r = (bb_min.Z-e.Z)/(s.Z-e.Z) - epsilon;
                    if (r>0.0) e = e + (s-e)*r;
                }

                if (s.Z>bb_max.Z)
                {
                    // clip s to zMax.
                    var r = (bb_max.Z-e.Z)/(s.Z-e.Z) + epsilon;
                    if (r<1.0) s = e + (s-e)*r;
                }
            }

            return true;
        }


        protected override bool ContainsIntersections()
        {
            return GetIntersections().Any();
        }

        protected virtual SortedMultiSet<ILineSegmentIntersector.IIntersection> GetIntersections()
        {
            if (null != Parent)
            {
                return Parent.Intersections;
            }

            return Intersections;
        }
    }
}