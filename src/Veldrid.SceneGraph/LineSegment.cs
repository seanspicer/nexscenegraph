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

namespace Veldrid.SceneGraph
{
    public interface ILineSegment
    {
        public Vector3 Start { get; }
        public Vector3 End { get; }
    }
    
    public class LineSegment : ILineSegment
    {
        public Vector3 Start { get; protected set; }
        public Vector3 End { get; protected set; }
        
        public static ILineSegment Create(Vector3 start, Vector3 end)
        {
            return new LineSegment(start, end);
        }

        protected LineSegment(Vector3 start, Vector3 end)
        {
            Start = start;
            End = end;
        }
    }
}