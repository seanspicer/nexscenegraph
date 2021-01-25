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

using System;
using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IPlaneProjector : IProjector
    {
        IPlane Plane { get; }
    }
    
    public class PlaneProjector : Projector, IPlaneProjector
    {
        public IPlane Plane { get; protected set; }
        
        
        public static IPlaneProjector Create(IPlane plane)
        {
            return new PlaneProjector(plane);
        }

        protected PlaneProjector(IPlane plane)
        {
            Plane = plane;
        }

        public override (bool, Vector3) Project(IPointerInfo pi)
        {
            throw new NotImplementedException();
        }
    }
}