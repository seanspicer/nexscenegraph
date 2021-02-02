

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Veldrid.SceneGraph.InputAdapter
{
    public interface IEventQueue
    {
        IList<IEvent> Events { get; set; }
    }
    
    public class EventQueue : IEventQueue
    {
        
        public class EventList : List<IEvent>
        {
        }
        
        public IList<IEvent> Events { get; set; }
    }
}