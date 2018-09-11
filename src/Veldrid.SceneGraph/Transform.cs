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

namespace Veldrid.SceneGraph
{
    /// <summary>
    /// A Transform is a node which transforms all children
    /// </summary>
    public class Transform : Group
    {
        public enum ReferenceFrameType
        {
            Relative,
            Absolute
        }
        
        public ReferenceFrameType ReferenceFrame { get; set; }

        public Transform()
        {
            ReferenceFrame = ReferenceFrameType.Relative;
        }
        
        // Required for double-dispatch
        public override void Accept(NodeVisitor visitor)
        {
            visitor.Apply(this);
        }

        public virtual bool ComputeLocalToWorldMatrix(ref Matrix4x4 matrix, NodeVisitor visitor)
        {
            if (ReferenceFrameType.Relative == ReferenceFrame)
            {
                return false;
            }

            matrix = Matrix4x4.Identity;
            return true;
        }
        
        public virtual bool ComputeWorldToLocalMatrix(ref Matrix4x4 matrix, NodeVisitor visitor)
        {
            if (ReferenceFrameType.Relative == ReferenceFrame)
            {
                return false;
            }

            matrix = Matrix4x4.Identity;
            return true;
        }

        public override BoundingSphere ComputeBound()
        {
            var bsphere = base.ComputeBound();
            if (!bsphere.Valid()) return bsphere;

            var localToWorld = new Matrix4x4();
            ComputeLocalToWorldMatrix(ref localToWorld, null);
           
            var xdash = bsphere.Center;
            xdash.X += bsphere.Radius;
            xdash = Vector3.Transform(xdash,localToWorld);

            var ydash = bsphere.Center;
            ydash.Y += bsphere.Radius;
            ydash = Vector3.Transform(ydash,localToWorld);

            var zdash = bsphere.Center;
            zdash.Z += bsphere.Radius;
            zdash = Vector3.Transform(zdash,localToWorld);

            bsphere.Center = Vector3.Transform(bsphere.Center, localToWorld);

            xdash -= bsphere.Center;
            var sqrlen_xdash = xdash.LengthSquared();

            ydash -= bsphere.Center;
            var sqrlen_ydash = ydash.LengthSquared();

            zdash -= bsphere.Center;
            var sqrlen_zdash = zdash.LengthSquared();

            bsphere.Radius = sqrlen_xdash;
            if (bsphere.Radius<sqrlen_ydash) bsphere.Radius = sqrlen_ydash;
            if (bsphere.Radius<sqrlen_zdash) bsphere.Radius = sqrlen_zdash;
            bsphere.Radius = (float)Math.Sqrt(bsphere.Radius);

            return bsphere;
            
        }
    }
}