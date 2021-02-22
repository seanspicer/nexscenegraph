
namespace Veldrid.SceneGraph.InputAdapter
{
    public interface IUiEventHandler : IEventHandler
    {
        bool Handle(IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter);
    }
    
    public class UiEventHandler : EventHandler, IUiEventHandler
    {
        protected virtual bool Handle(IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter, IObject obj,
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