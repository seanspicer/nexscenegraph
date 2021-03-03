using System.Collections.Generic;

namespace Veldrid.SceneGraph.InputAdapter
{
    public interface IEventVisitor : INodeVisitor
    {
        bool EventHandled { get; set; }
        IUiActionAdapter ActionAdapter { get; set; }
        public IList<IEvent> Events { get; }
        void AddEvent(IEvent evt);
        void RemoveEvent(IEvent evt);
        void Reset();
    }

    public class EventVisitor : NodeVisitor, IEventVisitor
    {
        protected EventVisitor()
            : base(VisitorType.EventVisitor, TraversalModeType.TraverseActiveChildren)
        {
        }

        public IUiActionAdapter ActionAdapter { get; set; } = null;

        public bool EventHandled { get; set; }

        //private EventQueue.EventList _events = new EventQueue.EventList();
        public IList<IEvent> Events { get; set; } = new List<IEvent>();

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

        public override void Apply(INode node)
        {
            HandleCallbacksAndTraverse(node);
        }

        public override void Apply(IDrawable drawable)
        {
            var callback = drawable.GetEventCallback();
            if (null != callback)
            {
                if (callback is IUiEventHandler uiEventHandler)
                {
                    callback.Run(drawable, this);
                }
                else
                {
                    var hasExecuted = false;
                    if (callback is IDrawableEventCallback drawableEventCallback)
                    {
                        drawableEventCallback.Event(this, drawable);
                        hasExecuted = true;
                    }

                    if (callback is INodeCallback nodeCallback)
                    {
                        nodeCallback.Execute(drawable, this);
                        hasExecuted = true;
                    }

                    if (!hasExecuted) callback.Run(drawable, this);
                }
            }

            HandleCallbacks(drawable.PipelineState);
        }

        public override void Apply(IGeode node)
        {
            HandleCallbacksAndTraverse(node);
        }

        public override void Apply(IBillboard node)
        {
            HandleCallbacksAndTraverse(node);
        }

        public override void Apply(IGroup node)
        {
            HandleCallbacksAndTraverse(node);
        }

        public override void Apply(ITransform node)
        {
            HandleCallbacksAndTraverse(node);
        }

        public override void Apply(ISwitch node)
        {
            HandleCallbacksAndTraverse(node);
        }

        public static IEventVisitor Create()
        {
            return new EventVisitor();
        }

        protected void HandleCallbacks(IPipelineState pipelineState)
        {
            // TODO Handle state event callbacks.
        }

        protected void HandleCallbacksAndTraverse(INode node)
        {
            if (node.HasPipelineState) HandleCallbacks(node.PipelineState);

            var callback = node.GetEventCallback();
            if (null != callback)
                callback.Run(node, this);
            else if (node.GetNumChildrenRequiringEventTraversal() > 0) Traverse(node);
        }
    }
}