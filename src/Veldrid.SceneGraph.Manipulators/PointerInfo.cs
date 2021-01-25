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

using System.Collections.Generic;
using System.Numerics;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Manipulators
{
    public class PointerInfo
    {
        protected Vector3 NearPoint { get; set; }
        protected Vector3 FarPoint { get; set; }
        protected Vector3 EyeDir { get; set; }
        protected Matrix4x4 Mvpw { get; set; }
        protected Matrix4x4 InverseMvpw { get; set; }
        
        public IReadOnlyList<LineSegmentIntersector.Intersection> HitList { get; set; }
    }
}