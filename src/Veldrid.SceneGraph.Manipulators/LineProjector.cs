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
    public interface ILineProjector : IProjector
    {
        ILineSegment LineSegment { get; }
        
        Vector3 LineStart { get; }
        Vector3 LineEnd { get; }
        
    }
    
    public class LineProjector : Projector, ILineProjector
    {
        public ILineSegment LineSegment { get; protected set; }
        public Vector3 LineStart => LineSegment.Start;
        public Vector3 LineEnd => LineSegment.End;

        public static ILineProjector Create(ILineSegment lineSegment)
        {
            return new LineProjector(lineSegment);
        }

        protected LineProjector(ILineSegment lineSegment)
        {
            LineSegment = lineSegment;
        }
    }
}