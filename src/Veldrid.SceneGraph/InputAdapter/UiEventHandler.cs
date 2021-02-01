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
    public interface IUiEventHandler : IEventHandler
    {
        bool Handle(IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter);

        bool Handle(IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter, IObject obj,
            INodeVisitor nodeVisitor);
    }
    
    public class UiEventHandler : EventHandler, IUiEventHandler
    {
        public virtual bool Handle(IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter, IObject obj,
            INodeVisitor nodeVisitor)
        {
            return Handle(eventAdapter, actionAdapter);
        }

        public virtual bool Handle(IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter)
        {
            return false;
        }
        
        public override bool Handle(IEvent evt, IObject obj, INodeVisitor nodeVisitor)
        {
            if (nodeVisitor is IEventVisitor eventVisitor && evt is IUiEventAdapter eventAdapter)
            {
                if (null != eventVisitor.ActionAdapter)
                {
                    var handled = Handle(eventAdapter, eventVisitor.ActionAdapter, obj, nodeVisitor);
                    if (handled) eventAdapter.Handled = true;
                    return handled;
                }
            }

            return false;
        }
    }
}