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
using System.Numerics;
using System.Xml.Schema;

namespace Veldrid.SceneGraph
{
    public interface IBoundingBox
    {
        float XMin { get; }
        float YMin { get; }
        float ZMin { get; }
        float XMax { get; }
        float YMax { get; }
        float ZMax { get; }
        
        Vector3 Min { get; }
        Vector3 Max { get; }
        
        Vector3 Center { get; }
        float Radius { get; }
        float RadiusSquared { get; }
        void Init();

        void Set(
            float xmin, float ymin, float zmin,
            float xmax, float ymax, float zmax);

        void Set(Vector3 min, Vector3 max);
        int GetHashCode();
        bool Equals(object Obj);

        /// <summary>
        /// Returns true if the bounding box extents are valid, false otherwise
        /// </summary>
        /// <returns></returns>
        bool Valid();

        /// <summary>
        /// Returns a specific corner of the bounding box.
        /// pos specifies the corner as a number between 0 and 7.
        /// Each bit selects an axis, X, Y, or Z from least- to
        /// most-significant. Unset bits select the minimum value
        /// for that axis, and set bits select the maximum.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        Vector3 Corner(uint pos);

        /// <summary>
        /// Expand the bounding box to include the given coordinate v.
        /// </summary>
        /// <param name="v"></param>
        void ExpandBy(Vector3 v);

        /// <summary>
        /// Expand the bounding box to include the coordinate (x, y, z)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        void ExpandBy(float x, float y, float z);

        /// <summary>
        /// Expand the bounding box to include the given bounding box.
        /// </summary>
        /// <param name="bb"></param>
        void ExpandBy(IBoundingBox bb);

        /// <summary>
        /// Expand the bounding box to include the given bounding sphere
        /// </summary>
        /// <param name="sh"></param>
        /// <exception cref="NotImplementedException"></exception>
        void ExpandBy(IBoundingSphere sh);

        IBoundingBox Intersect(IBoundingBox bb);
        bool Intersects(IBoundingBox bb);
        bool Contains(Vector3 v);
        bool Contains(Vector3 v, float epsilon);
    }

    public class BoundingBox : IBoundingBox
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

        public Vector3 Min => _min;
        public Vector3 Max => _max;

        public static IBoundingBox Create()
        {
            return new BoundingBox();
        }
        
        public static IBoundingBox Create(IBoundingBox bb)
        {
            return new BoundingBox(bb);
        }
        
        public static IBoundingBox Create(
            float xmin, float ymin, float zmin,
            float xmax, float ymax, float zmax)
        {
            return new BoundingBox(xmin, ymin, zmin, xmax, ymax, zmax);
        }
        
        public static IBoundingBox Create(Vector3 min, Vector3 max)
        {
            return new BoundingBox(min, max);
        }
        
        protected BoundingBox()
        {
            _min = Vector3.Multiply(Vector3.One, float.PositiveInfinity);
            _max = Vector3.Multiply(Vector3.One, float.NegativeInfinity);
        }

        protected BoundingBox(IBoundingBox bb)
        {
            _min = bb.Min;
            _max = bb.Max;
        }

        protected BoundingBox(
            float xmin, float ymin, float zmin,
            float xmax, float ymax, float zmax)
        {
            _min = new Vector3(xmin, ymin, zmin);
            _max = new Vector3(xmax, ymax, zmax);
        }

        protected BoundingBox(Vector3 min, Vector3 max)
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
                (pos & 1)>0 ? _max.X : _min.X, 
                (pos & 2)>0 ? _max.Y : _min.Y,
                (pos & 4)>0 ? _max.Z : _min.Z);
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
        public void ExpandBy(IBoundingBox bb)
        {
            if (!bb.Valid()) return;

            if(bb.XMin <_min.X) _min.X = bb.XMin;
            if(bb.XMax>_max.X) _max.X = bb.XMax;

            if(bb.YMin<_min.Y) _min.Y = bb.YMin;
            if(bb.YMax>_max.Y) _max.Y = bb.YMax;

            if(bb.ZMin<_min.Z) _min.Z = bb.ZMin;
            if(bb.ZMax>_max.Z) _max.Z = bb.ZMax;
        }

        /// <summary>
        /// Expand the bounding box to include the given bounding sphere
        /// </summary>
        /// <param name="sh"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ExpandBy(IBoundingSphere sh)
        {
            if (!sh.Valid()) return;
            
            if(sh.Center.X-sh.Radius<_min.X) _min.X = sh.Center.X-sh.Radius;
            if(sh.Center.X+sh.Radius>_max.X) _max.X = sh.Center.X+sh.Radius;

            if(sh.Center.Y-sh.Radius<_min.Y) _min.Y = sh.Center.Y-sh.Radius;
            if(sh.Center.Y+sh.Radius>_max.Y) _max.Y = sh.Center.Y+sh.Radius;

            if(sh.Center.Z-sh.Radius<_min.Z) _min.Z = sh.Center.Z-sh.Radius;
            if(sh.Center.Z+sh.Radius>_max.Z) _max.Z = sh.Center.Z+sh.Radius;
        }

        public IBoundingBox Intersect(IBoundingBox bb)
        {
            return new BoundingBox(
                Math.Max(XMin, bb.XMin), Math.Max(YMin, bb.YMin), Math.Max(ZMin, bb.ZMin),
                Math.Min(XMax, bb.XMax), Math.Min(YMax, bb.YMax), Math.Min(ZMax, bb.ZMax));
        }
        
        public bool Intersects(IBoundingBox bb)
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