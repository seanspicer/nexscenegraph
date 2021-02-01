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
using System.Linq;

namespace Veldrid.SceneGraph.InputAdapter
{
    public interface IEventHandler : IDrawableEventCallback
    {
        bool Handle(IEvent evt, IObject obj, INodeVisitor nodeVisitor);
    }
    
    public class EventHandler : NodeCallback, IEventHandler
    {
        public override bool Run(IObject obj, IObject data)
        {
            Execute(obj as INode, data as INodeVisitor);
            return true;
        }

        public override void Execute(INode node, INodeVisitor nodeVisitor)
        {
            if (nodeVisitor is IEventVisitor eventVisitor)
            {
                if (null != eventVisitor.ActionAdapter)
                {
                    foreach (var evt in eventVisitor.Events)
                    {
                        Handle(evt, node, nodeVisitor);
                    }
                }
            }

            if (node.GetNumChildrenRequiringEventTraversal() > 0 || null != NestedCallback)
            {
                Traverse(node, nodeVisitor);
            }
        }

        public void Event(INodeVisitor nodeVisitor, IDrawable drawable)
        {
            if (nodeVisitor is IEventVisitor eventVisitor)
            {
                if (null != eventVisitor.ActionAdapter)
                {
                    foreach (var evt in eventVisitor.Events)
                    {
                        Handle(evt, drawable, nodeVisitor);
                    }
                }
            }
        }

        public virtual bool Handle(IEvent evt, IObject obj, INodeVisitor nodeVisitor)
        {
            return false;
        }
    }
}