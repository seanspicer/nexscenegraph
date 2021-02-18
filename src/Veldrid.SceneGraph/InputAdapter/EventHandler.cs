
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