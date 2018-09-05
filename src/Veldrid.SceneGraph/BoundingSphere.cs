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

using System.Numerics;

namespace Veldrid.SceneGraph
{
    public class BoundingSphere
    {
        public Vector3 Center => _center;
        public float Radius => _radius;
        public float Radius2 => _radius * _radius;
        
        private Vector3 _center;
        private float _radius;

        public BoundingSphere()
        {
            _center = Vector3.Zero;
            _radius = -1.0f;
        }

        public BoundingSphere(Vector3 center, float radius)
        {
            _center = center;
            _radius = radius;
        }

        public BoundingSphere(BoundingSphere sh)
        {
            _center = sh._center;
            _radius = sh._radius;
        }

        public BoundingSphere(BoundingBox bb)
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
        public void ExpandBy(BoundingSphere sh)
        {
            // ignore operation if incoming BoundingSphere is invalid.
            if (!sh.Valid()) return;

            // This sphere is not set so use the inbound sphere
            if (!Valid())
            {
                _center = sh._center;
                _radius = sh._radius;

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
                _center = sh._center;
                _radius = sh._radius;
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
        public void ExpandRadiusBy(BoundingSphere sh)
        {
            if (!sh.Valid()) return;
            if (Valid())
            {
                var r = (sh._center-_center).Length()+sh._radius;
                if (r>_radius) _radius = r;
            }
            else
            {
                _center = sh._center;
                _radius = sh._radius;
            }
        }
        
        /// <summary>
        /// Expand bounding sphere to include bb. Repositions
        /// The sphere center to minimize the radius increase.
        /// </summary>
        /// <param name="sh"></param>
        public void ExpandBy(BoundingBox bb)
        {
            if (!bb.Valid()) return;
            
            if (Valid())
            {
                var newbb = new BoundingBox(bb);

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
        public void ExpandRadiusBy(BoundingBox bb)
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