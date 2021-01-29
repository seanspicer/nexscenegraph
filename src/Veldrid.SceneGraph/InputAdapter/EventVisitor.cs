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

namespace Veldrid.SceneGraph.InputAdapter
{
    public interface IEventVisitor : INodeVisitor
    {
        
        bool EventHandled { get; set; }
        IUiActionAdapter ActionAdapter { get; set; }
        
        void AddEvent(IEvent evt);
        void RemoveEvent(IEvent evt);

        void Reset();
    }
    
    public class EventVisitor : NodeVisitor, IEventVisitor
    {
        public IUiActionAdapter ActionAdapter { get; set; } = null;

        public bool EventHandled { get; set; } = false;

        protected EventQueue.EventList Events { get; set; } = new EventQueue.EventList();

        public static IEventVisitor Create()
        {
            return new EventVisitor();
        }

        protected EventVisitor()
        {
            
        }
        
        public virtual void Reset()
        {
            Events.Clear();
            EventHandled = false;
        }
        
        public virtual void AddEvent(IEvent evt)
        {
            Events.Add(evt);
        }

        public virtual void RemoveEvent(IEvent evt)
        {
            Events.Remove(evt);
        }
    }
}