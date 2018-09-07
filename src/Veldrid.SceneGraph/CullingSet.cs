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
using System.Collections.Generic;
using System.Numerics;

namespace Veldrid.SceneGraph
{
    /// <summary>
    /// Class to maintain a frustum (and a list of occluders at a later date?)
    /// </summary>
    public class CullingSet
    {
        public struct StateFrustumPair
        {
            public Polytope Polytope;
            public StateSet StateSet;

        }
        
        [Flags]
        enum MaskValues
        {
            NoCulling                = 0x0,
            ViewFrustumSidesCulling  = 0x1,
            NearPlaneCulling         = 0x2,
            FarPlaneCulling          = 0x4,
            ViewFrustumCulling       = ViewFrustumSidesCulling|
                                       NearPlaneCulling|
                                       FarPlaneCulling,
            SmallFeatureCulling      = 0x8,
            ShadowOcclusionCulling   = 0x10,
            DefaultCulling           = ViewFrustumSidesCulling|
                                       SmallFeatureCulling|
                                       ShadowOcclusionCulling,
            EnableAllCulling         = ViewFrustumCulling|
                                       SmallFeatureCulling|
                                       ShadowOcclusionCulling
        };

        private MaskValues _mask = MaskValues.DefaultCulling;
        private Polytope _frustum = new Polytope();
        public List<StateFrustumPair>    StateFrustumList { get; set; } = new List<StateFrustumPair>();
        private Vector4 _pixelSizeVector = Vector4.Zero;
        private float _smallFeatureCullingPixelSize = 0;

        public CullingSet() {}

        public bool IsCulled(BoundingBox bb)
        {
            if (0 != (_mask & MaskValues.ViewFrustumCulling))
            {
                // Check outside view frustum
                if (!_frustum.Contains(bb)) return true;
            }

            if (0 != (_mask & MaskValues.SmallFeatureCulling))
            {
                throw new NotImplementedException();
            }
            
            if (0 != (_mask & MaskValues.ShadowOcclusionCulling))
            {
                throw new NotImplementedException();
            }
            
            return false;
        }
    }
}