

using System.Collections.Generic;

namespace Veldrid.SceneGraph.InputAdapter
{
    public interface IEventVisitor : INodeVisitor
    {
        
        bool EventHandled { get; set; }
        IUiActionAdapter ActionAdapter { get; set; }
        void AddEvent(IEvent evt);
        void RemoveEvent(IEvent evt);
        void Reset();
        public IList<IEvent> Events { get; }
    }
    
    public class EventVisitor : NodeVisitor, IEventVisitor
    {
        public IUiActionAdapter ActionAdapter { get; set; } = null;

        public bool EventHandled { get; set; } = false;

        private EventQueue.EventList _events = new EventQueue.EventList();
        public IList<IEvent> Events => _events;

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