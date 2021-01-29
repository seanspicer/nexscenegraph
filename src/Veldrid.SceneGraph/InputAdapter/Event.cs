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

namespace Veldrid.SceneGraph.InputAdapter
{
    public interface IEvent : IObject
    {
        bool Handled { get; set; }
        DateTime Time { get; set; }
    }
    
    public class Event : Object, IEvent
    {
        public bool Handled { get; set; } = false;
        public DateTime Time { get; set; } = DateTime.MinValue;
        
        protected Event()
        {
            
        }
    }
}