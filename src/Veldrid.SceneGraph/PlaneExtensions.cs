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

using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Veldrid.SceneGraph
{
    public static class PlaneExtensions
    {
        private class PlaneCornerInfo
        {
            public int upperBBCorner = 0;
            public int lowerBBCorner = 0;
        }

        private static readonly Dictionary<Plane, PlaneCornerInfo> FieldTable = new Dictionary<Plane, PlaneCornerInfo>();
        
        public static int Intersect(this System.Numerics.Plane plane, BoundingBox bb)
        {
            if (false == FieldTable.TryGetValue(plane, out var pci))
            {
                pci = new PlaneCornerInfo();
                pci.upperBBCorner = (plane.Normal.X>=0.0?1:0) |
                                    (plane.Normal.Y>=0.0?2:0) |
                                    (plane.Normal.Z>=0.0?4:0);

                pci.lowerBBCorner = (~pci.upperBBCorner)&7;
                
                FieldTable.Add(plane, pci);
            }

            // if lowest point above plane than all above.
            if (plane.Distance(bb.Corner((uint) pci.lowerBBCorner)) > 0.0f) return 1;
            
            // if highest point is below plane then all below.
            if (plane.Distance(bb.Corner((uint) pci.upperBBCorner)) < 0.0f) return -1;
            
            // Otherwise, must be crossing a plane
            return 0;
        }

        public static float Distance(this System.Numerics.Plane plane, Vector3 v)
        {
            return plane.Normal.X * v.X +
                   plane.Normal.Y * v.Y +
                   plane.Normal.Z * v.Z +
                   plane.D;
        }
    }
}