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

using System.Numerics;

namespace Veldrid.SceneGraph
{
    public interface IBoundingSphere
    {
        Vector3 Center { get; set; }
        float Radius { get; set; }
        float Radius2 { get; }
        void Init();
        void Set(Vector3 center, float radius);
        bool Valid();
        int GetHashCode();
        bool Equals(object Obj);

        /// <summary>
        ///     Expand the sphere to include the coordinate v.  Repositions
        ///     The sphere center to minimize the radius increase.
        /// </summary>
        /// <param name="v"></param>
        void ExpandBy(Vector3 v);

        /// <summary>
        ///     Expand the sphere to include the coordinate v.  Does not
        ///     reposition the sphere center.
        /// </summary>
        /// <param name="v"></param>
        void ExpandRadiusBy(Vector3 v);

        /// <summary>
        ///     Expand bounding sphere to include sh. Repositions
        ///     The sphere center to minimize the radius increase.
        /// </summary>
        /// <param name="sh"></param>
        void ExpandBy(IBoundingSphere sh);

        /// <summary>
        ///     Expand the bounding sphere by sh. Does not
        ///     reposition the sphere center.
        /// </summary>
        /// <param name="sh"></param>
        void ExpandRadiusBy(IBoundingSphere sh);

        /// <summary>
        ///     Expand bounding sphere to include bb. Repositions
        ///     The sphere center to minimize the radius increase.
        /// </summary>
        /// <param name="sh"></param>
        void ExpandBy(IBoundingBox bb);

        /// <summary>
        ///     Expand the bounding sphere by bb. Does not
        ///     reposition the sphere center.
        /// </summary>
        /// <param name="sh"></param>
        void ExpandRadiusBy(IBoundingBox bb);
    }

    public class BoundingSphere : IBoundingSphere
    {
        private Vector3 _center;

        protected BoundingSphere()
        {
            _center = Vector3.Zero;
            Radius = -1.0f;
        }

        protected BoundingSphere(Vector3 center, float radius)
        {
            _center = center;
            Radius = radius;
        }

        protected BoundingSphere(IBoundingSphere sh)
        {
            _center = sh.Center;
            Radius = sh.Radius;
        }

        protected BoundingSphere(IBoundingBox bb)
        {
            _center = Vector3.Zero;
            Radius = -1.0f;
            ExpandBy(bb);
        }

        public Vector3 Center
        {
            get => _center;
            set => _center = value;
        }

        public float Radius { get; set; }

        public float Radius2 => Radius * Radius;

        public void Init()
        {
            _center.X = 0.0f;
            _center.Y = 0.0f;
            _center.Z = 0.0f;
            Radius = -1.0f;
        }

        public void Set(Vector3 center, float radius)
        {
            _center = center;
            Radius = radius;
        }

        public bool Valid()
        {
            return Radius >= 0.0f;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object Obj)
        {
            var rhs = (BoundingSphere) Obj;
            return _center == rhs._center && Radius == rhs.Radius;
        }

        /// <summary>
        ///     Expand the sphere to include the coordinate v.  Repositions
        ///     The sphere center to minimize the radius increase.
        /// </summary>
        /// <param name="v"></param>
        public void ExpandBy(Vector3 v)
        {
            if (Valid())
            {
                var dv = v - _center;
                var r = dv.Length();
                if (!(r > Radius)) return;
                var dr = (r - Radius) * 0.5f;
                _center += dv * (dr / r);
                Radius += dr;
            }
            else
            {
                _center = v;
                Radius = 0.0f;
            }
        }

        /// <summary>
        ///     Expand the sphere to include the coordinate v.  Does not
        ///     reposition the sphere center.
        /// </summary>
        /// <param name="v"></param>
        public void ExpandRadiusBy(Vector3 v)
        {
            if (Valid())
            {
                var r = (v - _center).Length();
                if (r > Radius) Radius = r;
            }
            else
            {
                _center = v;
                Radius = 0.0f;
            }
        }

        /// <summary>
        ///     Expand bounding sphere to include sh. Repositions
        ///     The sphere center to minimize the radius increase.
        /// </summary>
        /// <param name="sh"></param>
        public void ExpandBy(IBoundingSphere sh)
        {
            // ignore operation if incoming BoundingSphere is invalid.
            if (!sh.Valid()) return;

            // This sphere is not set so use the inbound sphere
            if (!Valid())
            {
                _center = sh.Center;
                Radius = sh.Radius;

                return;
            }


            // Calculate d == The distance between the sphere centers
            double d = (_center - sh.Center).Length();

            // New sphere is already inside this one
            if (d + sh.Radius <= Radius) return;

            //  New sphere completely contains this one
            if (d + Radius <= sh.Radius)
            {
                _center = sh.Center;
                Radius = sh.Radius;
                return;
            }


            // Build a new sphere that completely contains the other two:
            //
            // The center point lies halfway along the line between the furthest
            // points on the edges of the two spheres.
            //
            // Computing those two points is ugly - so we'll use similar triangles
            var newRadius = (Radius + d + sh.Radius) * 0.5;
            var ratio = (newRadius - Radius) / d;

            _center.X += (sh.Center.X - _center.X) * (float) ratio;
            _center.Y += (sh.Center.Y - _center.Y) * (float) ratio;
            _center.Z += (sh.Center.Z - _center.Z) * (float) ratio;

            Radius = (float) newRadius;
        }

        /// <summary>
        ///     Expand the bounding sphere by sh. Does not
        ///     reposition the sphere center.
        /// </summary>
        /// <param name="sh"></param>
        public void ExpandRadiusBy(IBoundingSphere sh)
        {
            if (!sh.Valid()) return;
            if (Valid())
            {
                var r = (sh.Center - _center).Length() + sh.Radius;
                if (r > Radius) Radius = r;
            }
            else
            {
                _center = sh.Center;
                Radius = sh.Radius;
            }
        }

        /// <summary>
        ///     Expand bounding sphere to include bb. Repositions
        ///     The sphere center to minimize the radius increase.
        /// </summary>
        /// <param name="sh"></param>
        public void ExpandBy(IBoundingBox bb)
        {
            if (!bb.Valid()) return;

            if (Valid())
            {
                var newbb = BoundingBox.Create(bb);

                for (uint c = 0; c < 8; ++c)
                {
                    var v = bb.Corner(c) - _center; // get the direction vector from corner
                    v = Vector3.Normalize(v); // normalise it.
                    v *= -Radius; // move the vector in the opposite direction distance radius.
                    v += _center; // move to absolute position.
                    newbb.ExpandBy(v); // add it into the new bounding box.
                }

                _center = newbb.Center;
                Radius = newbb.Radius;
            }
            else
            {
                _center = bb.Center;
                Radius = bb.Radius;
            }
        }

        /// <summary>
        ///     Expand the bounding sphere by bb. Does not
        ///     reposition the sphere center.
        /// </summary>
        /// <param name="sh"></param>
        public void ExpandRadiusBy(IBoundingBox bb)
        {
            if (!bb.Valid()) return;
            if (Valid())
            {
                for (uint c = 0; c < 8; ++c)
                {
                    var v = bb.Corner(c);
                    ExpandRadiusBy(v);
                }
            }
            else
            {
                _center = bb.Center;
                Radius = bb.Radius;
            }
        }

        public static IBoundingSphere Create()
        {
            return new BoundingSphere();
        }

        public static IBoundingSphere Create(Vector3 center, float radius)
        {
            return new BoundingSphere(center, radius);
        }

        public static IBoundingSphere Create(IBoundingSphere sh)
        {
            return new BoundingSphere(sh);
        }

        public static IBoundingSphere Create(IBoundingBox bb)
        {
            return new BoundingSphere(bb);
        }

        public static bool operator !=(BoundingSphere lhs, BoundingSphere rhs)
        {
            if (ReferenceEquals(lhs, null)) return ReferenceEquals(rhs, null);
            return !lhs.Equals(rhs);
        }

        public static bool operator ==(BoundingSphere lhs, BoundingSphere rhs)
        {
            if (ReferenceEquals(lhs, null)) return ReferenceEquals(rhs, null);
            return lhs.Equals(rhs);
        }
    }
}