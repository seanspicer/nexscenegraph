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

using System.Data;
using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IMotionCommand
    {
        void Accept(IConstraint constraint);
        void Accept(IDraggerCallback callback);
    }
    
    public abstract class MotionCommand : IMotionCommand
    {
        public abstract Matrix4x4 GetMotionMatrix();

        public virtual void Accept(IConstraint constraint)
        {
            constraint.Constrain(this);
        }

        public virtual void Accept(IDraggerCallback callback)
        {
            callback.Receive(this);
        }
    }
}