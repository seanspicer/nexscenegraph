//
// Copyright 2018 Sean Spicer 
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
    public class BoundingSphere : IBoundingSphere
    {
        public Vector3 Center
        {
            get => _center;
            set => _center = value;
        }

        public float Radius
        {
            get => _radius;
            set => _radius = value;
        }
        
        public float Radius2 => _radius * _radius;
        
        private Vector3 _center;
        private float _radius;

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
        
        protected BoundingSphere()
        {
            _center = Vector3.Zero;
            _radius = -1.0f;
        }

        protected BoundingSphere(Vector3 center, float radius)
        {
            _center = center;
            _radius = radius;
        }

        protected BoundingSphere(IBoundingSphere sh)
        {
            _center = sh.Center;
            _radius = sh.Radius;
        }

        protected BoundingSphere(IBoundingBox bb)
        {
            _center = Vector3.Zero;
            _radius = -1.0f;
            ExpandBy(bb);
        }

        public void Init()
        {
            _center.X = 0.0f;
            _center.Y = 0.0f;
            _center.Z = 0.0f;
            _radius = -1.0f;
        }

        public void Set(Vector3 center, float radius)
        {
            _center = center;
            _radius = radius;
        }

        public bool Valid()
        {
            return _radius >= 0.0f;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        
        public override bool Equals(object Obj)
        {
            var rhs = (BoundingSphere) Obj;
            return _center == rhs._center && _radius == rhs._radius;
        }
        
        public static bool operator !=(BoundingSphere lhs, BoundingSphere rhs)
        {
            if (object.ReferenceEquals(lhs, null))
            {
                return object.ReferenceEquals(rhs, null);
            }
            return ! lhs.Equals(rhs);
        }

        public static bool operator ==(BoundingSphere lhs, BoundingSphere rhs)
        {
            if (object.ReferenceEquals(lhs, null))
            {
                return object.ReferenceEquals(rhs, null);
            }
            return lhs.Equals(rhs);
        }
        
        /// <summary>
        /// Expand the sphere to include the coordinate v.  Repositions
        /// The sphere center to minimize the radius increase.
        /// </summary>
        /// <param name="v"></param>
        public void ExpandBy(Vector3 v)
        {
            if (Valid())
            {
                var dv = v-_center;
                var r = dv.Length();
                if (!(r > _radius)) return;
                var dr = (r-_radius)*0.5f;
                _center += dv*(dr/r);
                _radius += dr;
            }
            else
            {
                _center = v;
                _radius = 0.0f;
            }
        }

        /// <summary>
        /// Expand the sphere to include the coordinate v.  Does not
        /// reposition the sphere center.
        /// </summary>
        /// <param name="v"></param>
        public void ExpandRadiusBy(Vector3 v)
        {
            if (Valid())
            {
                var r = (v-_center).Length();
                if (r>_radius) _radius = r;
            }
            else
            {
                _center = v;
                _radius = 0.0f;
            }
        }
        
        /// <summary>
        /// Expand bounding sphere to include sh. Repositions
        /// The sphere center to minimize the radius increase.
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
                _radius = sh.Radius;

                return;
            }


            // Calculate d == The distance between the sphere centers
            double d = ( _center - sh.Center ).Length();

            // New sphere is already inside this one
            if ( d + sh.Radius <= _radius )
            {
                return;
            }

            //  New sphere completely contains this one
            if ( d + _radius <= sh.Radius )
            {
                _center = sh.Center;
                _radius = sh.Radius;
                return;
            }


            // Build a new sphere that completely contains the other two:
            //
            // The center point lies halfway along the line between the furthest
            // points on the edges of the two spheres.
            //
            // Computing those two points is ugly - so we'll use similar triangles
            var newRadius = (_radius + d + sh.Radius ) * 0.5;
            var ratio = ( newRadius - _radius ) / d ;

            _center.X += ( sh.Center.X - _center.X ) * (float)ratio;
            _center.Y += ( sh.Center.Y - _center.Y ) * (float)ratio;
            _center.Z += ( sh.Center.Z - _center.Z ) * (float)ratio;

            _radius = (float) newRadius;
        }

        /// <summary>
        /// Expand the bounding sphere by sh. Does not
        /// reposition the sphere center.
        /// </summary>
        /// <param name="sh"></param>
        public void ExpandRadiusBy(IBoundingSphere sh)
        {
            if (!sh.Valid()) return;
            if (Valid())
            {
                var r = (sh.Center-_center).Length()+sh.Radius;
                if (r>_radius) _radius = r;
            }
            else
            {
                _center = sh.Center;
                _radius = sh.Radius;
            }
        }
        
        /// <summary>
        /// Expand bounding sphere to include bb. Repositions
        /// The sphere center to minimize the radius increase.
        /// </summary>
        /// <param name="sh"></param>
        public void ExpandBy(IBoundingBox bb)
        {
            if (!bb.Valid()) return;
            
            if (Valid())
            {
                var newbb = BoundingBox.Create(bb);

                for(uint c=0;c<8;++c)
                {
                    var v = bb.Corner(c)-_center; // get the direction vector from corner
                    v = Vector3.Normalize(v);     // normalise it.
                    v *= -_radius;                // move the vector in the opposite direction distance radius.
                    v += _center;                 // move to absolute position.
                    newbb.ExpandBy(v);            // add it into the new bounding box.
                }

                _center = newbb.Center;
                _radius = newbb.Radius;

            }
            else
            {
                _center = bb.Center;
                _radius = bb.Radius;
            }
        }
        
        /// <summary>
        /// Expand the bounding sphere by bb. Does not
        /// reposition the sphere center.
        /// </summary>
        /// <param name="sh"></param>
        public void ExpandRadiusBy(IBoundingBox bb)
        {
            if (!bb.Valid()) return;
            if (Valid())
            {
                for(uint c=0;c<8;++c)
                {
                    var v = bb.Corner(c);
                    ExpandRadiusBy(v);
                }
            }
            else
            {
                _center = bb.Center;
                _radius = bb.Radius;
            }
        }
    }
}