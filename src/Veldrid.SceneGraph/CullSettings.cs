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

namespace Veldrid.SceneGraph
{
    public class CullSettings
    {
        enum CullingMode : uint
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
        
        enum ComputeNearFarMode
        {
            DoNotComputeNearFar = 0,
            ComputeNearFarUsingBoundingVolumes,
            ComputeNearFarUsingPrimitives,
            ComputeNearUsingPrimitives
        };
        
        enum VariablesMask : uint
        {
            ComputeNearFarMode                 = (0x1 << 0),
            CullingMode                        = (0x1 << 1),
            LodScale                           = (0x1 << 2),
            SmallFeatureCullingPixelSize       = (0x1 << 3),
            ClampProjectionMatrixCallback      = (0x1 << 4),
            NearFarRatio                       = (0x1 << 5),
            ImpostorActive                     = (0x1 << 6),
            DepthSortImpostorSprites           = (0x1 << 7),
            ImpostorPixelErrorThreshold        = (0x1 << 8),
            NumFramesToKeepImpostorsSprites    = (0x1 << 9),
            CullMask                           = (0x1 << 10),
            CullMaskLeft                       = (0x1 << 11),
            CullMaskRight                      = (0x1 << 12),
            ClearColor                         = (0x1 << 13),
            ClearMask                          = (0x1 << 14),
            LightingMode                       = (0x1 << 15),
            Light                              = (0x1 << 16),
            DrawBuffer                         = (0x1 << 17),
            ReadBuffer                         = (0x1 << 18),

            NoVariables                        = 0x00000000,
            AllVariables                       = 0x7FFFFFFF
        };
        
        enum InheritanceMaskActionOnAttributeSetting
        {
            DisableAssociatedInheritanceMaskBit,
            DoNotModifyInheritanceMask
        };

        private VariablesMask                             _inheritanceMask;
        private InheritanceMaskActionOnAttributeSetting   _inheritanceMaskActionOnAttributeSetting;
        
        private CullingMode                               _cullingMode;
        private float                                     _LODScale;
        private float                                     _smallFeatureCullingPixelSize;
        
        private ComputeNearFarMode                        _computeNearFar;
        private double                                    _nearFarRatio;
        private bool                                      _impostorActive;
        private bool                                      _depthSortImpostorSprites;
        private float                                     _impostorPixelErrorThreshold;
        private int                                       _numFramesToKeepImpostorSprites;
        
        private uint                                      _cullMask;
        private uint                                      _cullMaskLeft;
        private uint                                      _cullMaskRight;
        
        public CullSettings()
        {
            SetDefaults();
        }

        protected virtual void SetDefaults()
        {
            _inheritanceMask = VariablesMask.AllVariables;
            _inheritanceMaskActionOnAttributeSetting = InheritanceMaskActionOnAttributeSetting.DisableAssociatedInheritanceMaskBit;
            _cullingMode = CullingMode.DefaultCulling;
            _LODScale = 1.0f;
            _smallFeatureCullingPixelSize = 2.0f;

            _computeNearFar = ComputeNearFarMode.ComputeNearFarUsingBoundingVolumes;
            _nearFarRatio = 0.0005;
            _impostorActive = true;
            _depthSortImpostorSprites = false;
            _impostorPixelErrorThreshold = 4.0f;
            _numFramesToKeepImpostorSprites = 10;
            _cullMask = 0xffffffff;
            _cullMaskLeft = 0xffffffff;
            _cullMaskRight = 0xffffffff;
        }
    }
}