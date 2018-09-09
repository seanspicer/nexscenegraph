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
            else
            {
                matrix = Matrix4x4.Identity;
                return true;
            }
        }
        
        public virtual bool ComputeWorldToLocalMatrix(ref Matrix4x4 matrix, NodeVisitor visitor)
        {
            if (ReferenceFrameType.Relative == ReferenceFrame)
            {
                return false;
            }
            else
            {
                matrix = Matrix4x4.Identity;
                return true;
            }
        }

        public virtual BoundingSphere computeBound()
        {
            throw new NotImplementedException();
        }
    }
}