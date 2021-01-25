//
// This file is part of IMAGEFrac (R) and related technologies.
//
// Copyright (c) 2017-2020 Reveal Energy Services.  All Rights Reserved.
//
// LEGAL NOTICE:
// IMAGEFrac contains trade secrets and otherwise confidential information
// owned by Reveal Energy Services. Access to and use of this information is 
// strictly limited and controlled by the Company. This file may not be copied,
// distributed, or otherwise disclosed outside of the Company's facilities 
// except under appropriate precautions to maintain the confidentiality hereof, 
// and may not be used in any way not expressly authorized by the Company.
//

using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IDraggerTransformCallback : IDraggerCallback
    {
        enum HandleCommandMask
        {
            HandleTranslateInLine     = 1<<0,
            HandleTranslateInPlane    = 1<<1,
            HandleScaled1D            = 1<<2,
            HandleScaled2D            = 1<<3,
            HandleScaledUniform       = 1<<4,
            HandleRotate3D            = 1<<5,
            HandleAll                 = 0x8ffffff
        }
    }
    
    public class DraggerTransformCallback : DraggerCallback, IDraggerTransformCallback
    {
        public IDraggerTransformCallback Create(MatrixTransform transform,
            IDraggerTransformCallback.HandleCommandMask handleCommandMask =
                IDraggerTransformCallback.HandleCommandMask.HandleAll)
        {
            return new DraggerTransformCallback(transform, handleCommandMask);
        }

        protected DraggerTransformCallback(MatrixTransform transform,
            IDraggerTransformCallback.HandleCommandMask handleCommandMask =
                IDraggerTransformCallback.HandleCommandMask.HandleAll)
        {
            
        }
    }
}