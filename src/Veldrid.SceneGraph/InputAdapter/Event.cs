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
        protected Event()
        {
        }

        public bool Handled { get; set; } = false;
        public DateTime Time { get; set; } = DateTime.MinValue;
    }
}