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
using System.Numerics;
using System.Xml.Schema;

namespace Veldrid.SceneGraph
{
    public class BoundingBox
    {
        public float XMin => _min.X;
        public float YMin => _min.Y;
        public float ZMin => _min.Z;
        
        public float XMax => _max.X;
        public float YMax => _max.Y;
        public float ZMax => _max.Z;

        public Vector3 Center => (_min + _max) * 0.5f;
        public float Radius => (float)Math.Sqrt(RadiusSquared);
        public float RadiusSquared => 0.25f * ((_max - _min).LengthSquared());

        private Vector3 _min;
        private Vector3 _max;
       
        public BoundingBox()
        {
            _min = Vector3.Multiply(Vector3.One, float.PositiveInfinity);
            _max = Vector3.Multiply(Vector3.One, float.NegativeInfinity);
        }

        public BoundingBox(BoundingBox bb)
        {
            _min = bb._min;
            _max = bb._max;
        }

        public BoundingBox(
            float xmin, float ymin, float zmin,
            float xmax, float ymax, float zmax)
        {
            _min = new Vector3(xmin, ymin, zmin);
            _max = new Vector3(xmax, ymax, zmax);
        }

        public BoundingBox(Vector3 min, Vector3 max)
        {
            _min = min;
            _max = max;
        }
        
        public void Init()
        {
            _min.X = float.PositiveInfinity;
            _min.Y = float.PositiveInfinity; 
            _min.Z = float.PositiveInfinity;
            
            _max.X = float.NegativeInfinity;
            _max.Y = float.NegativeInfinity; 
            _max.Z = float.NegativeInfinity;
        }

        public void Set(
            float xmin, float ymin, float zmin,
            float xmax, float ymax, float zmax)
        {
            _min.X = xmin;
            _min.Y = ymin; 
            _min.Z = zmin;
            
            _max.X = xmax;
            _max.Y = ymax; 
            _max.Z = zmax;
        }
        
        public void Set(Vector3 min, Vector3 max)
        {
            _min = min;
            _max = max;
        }
        
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        
        public override bool Equals(object Obj)
        {
            var rhs = (BoundingBox) Obj;
            return _min == rhs._min && _max == rhs._max;
        }
        
        public static bool operator !=(BoundingBox lhs, BoundingBox rhs)
        {
            if (object.ReferenceEquals(lhs, null))
            {
                return object.ReferenceEquals(rhs, null);
            }
            return ! lhs.Equals(rhs);
        }

        public static bool operator ==(BoundingBox lhs, BoundingBox rhs)
        {
            if (object.ReferenceEquals(lhs, null))
            {
                return object.ReferenceEquals(rhs, null);
            }
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Returns true if the bounding box extents are valid, false otherwise
        /// </summary>
        /// <returns></returns>
        public bool Valid()
        {
            return _max.X >= _min.X &&
                   _max.Y >= _min.Y &&
                   _max.Z >= _min.Z;
        }

        /// <summary>
        /// Returns a specific corner of the bounding box.
        /// pos specifies the corner as a number between 0 and 7.
        /// Each bit selects an axis, X, Y, or Z from least- to
        /// most-significant. Unset bits select the minimum value
        /// for that axis, and set bits select the maximum.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vector3 Corner(uint pos)
        {
            return new Vector3(
                1 == (pos & 1) ? _max.X : _min.X, 
                1 == (pos & 2) ? _max.Y : _min.Y,
                1 == (pos & 4) ? _max.Z : _min.Z);
        }

        /// <summary>
        /// Expand the bounding box to include the given coordinate v.
        /// </summary>
        /// <param name="v"></param>
        public void ExpandBy(Vector3 v)
        {
            if(v.X<_min.X) _min.X = v.X;
            if(v.X>_max.X) _max.X = v.X;

            if(v.Y<_min.Y) _min.Y = v.Y;
            if(v.Y>_max.Y) _max.Y = v.Y;

            if(v.Z<_min.Z) _min.Z = v.Z;
            if(v.Z>_max.Z) _max.Z = v.Z;
        }

        /// <summary>
        /// Expand the bounding box to include the coordinate (x, y, z)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void ExpandBy(float x, float y, float z)
        {
            if(x<_min.X) _min.X = x;
            if(x>_max.X) _max.X = x;

            if(y<_min.Y) _min.Y = y;
            if(y>_max.Y) _max.Y = y;

            if(z<_min.Z) _min.Z = z;
            if(z>_max.Z) _max.Z = z;
        }

        /// <summary>
        /// Expand the bounding box to include the given bounding box.
        /// </summary>
        /// <param name="bb"></param>
        public void ExpandBy(BoundingBox bb)
        {
            if (!bb.Valid()) return;

            if(bb._min.X<_min.X) _min.X = bb._min.X;
            if(bb._max.X>_max.X) _max.X = bb._max.X;

            if(bb._min.Y<_min.Y) _min.Y = bb._min.Y;
            if(bb._max.Y>_max.Y) _max.Y = bb._max.Y;

            if(bb._min.Z<_min.Z) _min.Z = bb._min.Z;
            if(bb._max.Z>_max.Z) _max.Z = bb._max.Z;
        }

        /// <summary>
        /// Expand the bounding box to include the given bounding sphere
        /// </summary>
        /// <param name="sh"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ExpandBy(BoundingSphere sh)
        {
            throw new NotImplementedException();
        }

        public BoundingBox Intersect(BoundingBox bb)
        {
            return new BoundingBox(
                Math.Max(XMin, bb.XMin), Math.Max(YMin, bb.YMin), Math.Max(ZMin, bb.ZMin),
                Math.Min(XMax, bb.XMax), Math.Min(YMax, bb.YMax), Math.Min(ZMax, bb.ZMax));
        }
        
        public bool Intersects(BoundingBox bb)
        {
            return Math.Max(XMin, bb.XMin) <= Math.Min(XMax, bb.XMax) &&
                   Math.Max(YMin, bb.YMin) <= Math.Min(YMax, bb.YMax) &&
                   Math.Max(ZMin, bb.ZMin) <= Math.Min(ZMax, bb.ZMax);
        }

        public bool Contains(Vector3 v)
        {
            return Valid() &&
                   (v.X>=_min.X && v.X<=_max.X) &&
                   (v.Y>=_min.Y && v.Y<=_max.Y) &&
                   (v.Z>=_min.Z && v.Z<=_max.Z);
        }

        public bool Contains(Vector3 v, float epsilon)
        {
            return Valid() &&
                   (v.X+epsilon>=_min.X && v.X+epsilon<=_max.X) &&
                   (v.Y+epsilon>=_min.Y && v.Y+epsilon<=_max.Y) &&
                   (v.Z+epsilon>=_min.Z && v.Z+epsilon<=_max.Z);
        }
    }
}