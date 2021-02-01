using System;
using System.ComponentModel;
using System.Numerics;
using System.Threading;

namespace Veldrid.SceneGraph.InputAdapter
{
    public class AnimationData
    {
        public double AnimationTime { get; set; }
        public bool IsAnimating { get; set; }
        public DateTime StartTime { get; set; }
        public double Phase { get; set; }

        public AnimationData()
        {
            AnimationTime =0;
            IsAnimating = false;
            StartTime = DateTime.MinValue;
            Phase = 0;
        }

        public void Start(DateTime startTime)
        {
            IsAnimating = true;
            StartTime = startTime;
            Phase = 0;
        }
    };
    
    public abstract class StandardManipulator : CameraManipulator
    {

        private INode _node;
        private float _modelSize;
        protected UserInteractionFlags _flags;

        protected bool _thrown = false;
        public bool AllowThrow { get; protected set; } = true;

        protected IUiEventAdapter EventAdapterT0 { get; set; } = null;
        protected IUiEventAdapter EventAdapterT1 { get; set; } = null;
        
        protected DateTime LastFrameTime { get; set; } = DateTime.MinValue;
        protected double DeltaFrameTime { get; set; } = 0;
        protected AnimationData AnimationData { get; set; }
        
        [Flags]
        public enum UserInteractionFlags
        {
            UpdateModelSize = 1,
            ComputeHomeUsingBoundingBox = 2,
            ProcessMouseWheel = 4,
            SetCenterOnForwardWheelMovement = 8,
            DefaultSettings = UpdateModelSize | ComputeHomeUsingBoundingBox | ProcessMouseWheel
        };

        protected StandardManipulator(UserInteractionFlags flags = UserInteractionFlags.DefaultSettings)
        {
            _flags = flags;
        }
        
        public override INode GetNode()
        {
            return _node;
        }
        
        public override void SetNode(INode node)
        {
            _node = node;

            if (null != _node)
            {
                var boundingSphere = _node.ComputeBound();
                _modelSize = boundingSphere.Radius;
            }
            else
            {
                _modelSize = 0;
            }

            if (GetAutoComputeHomePosition())
            {
                ComputeHomePosition(null, (_flags & UserInteractionFlags.ComputeHomeUsingBoundingBox) != 0);
            }
        }

        protected void FlushMouseEventStack()
        {
            EventAdapterT0 = null;
            EventAdapterT1 = null;
        }

        protected bool IsMouseMoving()
        {
            if (null == EventAdapterT0 || null == EventAdapterT1) return false;
            const float velocity = 0.1f;
            
            var dx = EventAdapterT0.XNormalized - EventAdapterT1.XNormalized;
            var dy = EventAdapterT0.YNormalized - EventAdapterT1.YNormalized;
            var len = System.Math.Sqrt(dx * dx + dy * dy);
            var dt = (EventAdapterT0.Time - EventAdapterT1.Time).TotalSeconds;

            return (len > dt * velocity);
        }

        protected void AddMouseEvent(IUiEventAdapter eventAdapter)
        {
            EventAdapterT1 = EventAdapterT0;
            EventAdapterT0 = eventAdapter;
        }

        protected void Init(IUiActionAdapter actionAdapter)
        {
            FlushMouseEventStack();
            _thrown = false;
            actionAdapter.RequestContinuousUpdate(false);
        }
        
        public override bool Handle(IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter)
        {
            switch (eventAdapter.EventType)
            {
                case IUiEventAdapter.EventTypeValue.Frame:
                    return HandleFrame(eventAdapter, actionAdapter);
                case IUiEventAdapter.EventTypeValue.Resize:
                    return HandleResize(actionAdapter);
                default:
                    break;
            }

            if (eventAdapter.Handled) return false;

            switch (eventAdapter.EventType)
            {
                case IUiEventAdapter.EventTypeValue.Move:
                    return HandleMouseMove(eventAdapter, actionAdapter);
                case IUiEventAdapter.EventTypeValue.Drag:
                    return HandleDrag(eventAdapter, actionAdapter);
                case IUiEventAdapter.EventTypeValue.Push:
                    return HandleMouseButtonPushed(eventAdapter, actionAdapter);
                case IUiEventAdapter.EventTypeValue.Release:
                    return HandleMouseButtonReleased(eventAdapter, actionAdapter);
                case IUiEventAdapter.EventTypeValue.KeyDown:
                    return HandleKeyDown(eventAdapter, actionAdapter);
                case IUiEventAdapter.EventTypeValue.KeyUp:
                    return HandleKeyUp(eventAdapter, actionAdapter);
                case IUiEventAdapter.EventTypeValue.Scroll:
                    return (_flags & UserInteractionFlags.ProcessMouseWheel) != 0 && HandleWheelDelta(eventAdapter, actionAdapter);
                default:
                    return false;
                    
            }

            // if (InputStateTracker.IsMouseButtonPushed())
            // {
            //     HandleMouseButtonPushed(eventAdapter, actionAdapter);
            // }
            //
            // if (InputStateTracker.IsMouseButtonReleased())
            // {
            //     HandleMouseButtonReleased(eventAdapter, actionAdapter);
            // }
            //
            // if (InputStateTracker.IsMouseButtonDown() && InputStateTracker.IsMouseMove())
            // {
            //     HandleDrag(eventAdapter, actionAdapter);
            // }
            //
            // if (InputStateTracker.FrameSnapshot.WheelDelta != 0)
            // {
            //     HandleWheelDelta(eventAdapter, actionAdapter);
            // }
        }

        protected virtual bool HandleFrame(IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter)
        {
            var currentFrameTime = eventAdapter.Time;
            DeltaFrameTime = (currentFrameTime - LastFrameTime).TotalSeconds;
            LastFrameTime = currentFrameTime;

            if (_thrown && PerformMovement())
            {
                actionAdapter.RequestRedraw();
            }

            if (null != AnimationData && AnimationData.IsAnimating)
            {
                PerformAnimiationMovement(eventAdapter, actionAdapter);
            }

            return false;
        }
        
        protected virtual bool HandleResize(IUiActionAdapter actionAdapter)
        {
            Init(actionAdapter);

            actionAdapter.RequestRedraw();
            return true;
        }
        
        protected virtual bool HandleDrag(IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter)
        {
            if (PerformMovement())
            {
                actionAdapter.RequestRedraw();
                return true;
            }

            return false;
        }

        protected virtual bool HandleMouseMove(IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter)
        {
            return false;
            //_logger.Debug(m => m("Move Event!"));
        }

        protected virtual bool HandleMouseButtonPushed(IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter)
        {
            FlushMouseEventStack();
            AddMouseEvent(eventAdapter);

            if (PerformMovement())
            {
                actionAdapter.RequestRedraw();
            }
            
            actionAdapter.RequestContinuousUpdate(false);
            _thrown = false;

            return true;
            //_logger.Debug(m => m("Button Pushed!"));
        }

        protected virtual bool HandleMouseButtonReleased(IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter)
        {
            if (0 == eventAdapter.MouseButtonMask)
            {
                var timeSinceLastRecordEvent = (null != EventAdapterT0)
                    ? (eventAdapter.Time - EventAdapterT0.Time).TotalSeconds
                    : double
                        .MaxValue;

                if (timeSinceLastRecordEvent > 0.02)
                {
                    FlushMouseEventStack();
                }

                if (IsMouseMoving())
                {
                    if (PerformMovement() && AllowThrow)
                    {
                        actionAdapter.RequestRedraw();
                        actionAdapter.RequestContinuousUpdate(true);
                        _thrown = true;
                    }

                    return true;
                }
            }

            FlushMouseEventStack();
            AddMouseEvent(eventAdapter);
            if (PerformMovement())
            {
                actionAdapter.RequestRedraw();
            }

            actionAdapter.RequestContinuousUpdate(false);
            _thrown = false;

            return true;
            
            //_logger.Debug(m => m("Button Released!"));
        }

        protected virtual bool HandleWheelDelta(IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter)
        {
            return false;
            //_logger.Debug(m => m("Wheel Delta"));
        }

        protected virtual bool HandleKeyDown(IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter)
        {
            if (eventAdapter.Key == IUiEventAdapter.KeySymbol.KeySpace)
            {
                FlushMouseEventStack();
                _thrown = false;
                Home(actionAdapter);
                return true;
            }

            return false;
        }

        protected virtual bool HandleKeyUp(IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter)
        {
            return false;
        }

        protected virtual bool PerformMovement()
        {
            // Return if fewer than two events have been added
            if (null == EventAdapterT0 || null == EventAdapterT1)
            {
                return false;
            }
            
            // Determine delta time
            var eventTimeDelta = (EventAdapterT0.Time - EventAdapterT1.Time).TotalSeconds;
            if (eventTimeDelta < 0)
            {
                throw new Exception($"Event Time Delta is Wrong? EventTimeDelta={eventTimeDelta} seconds");
                eventTimeDelta = 0;
            }
            
            // Get Delta X and Delta Y
            var dx = EventAdapterT0.XNormalized - EventAdapterT1.XNormalized;
            var dy = EventAdapterT0.YNormalized - EventAdapterT1.YNormalized;
            
            // Return if there is no movement
            if (0 == dx && 0 == dy) return false;
            
            // Call Appropriate methods
            var buttonMask = EventAdapterT1.MouseButtonMask;
            var modMask = EventAdapterT1.ModKeyMask;
            if (buttonMask == IUiEventAdapter.MouseButtonMaskType.LeftMouseButton)
            {
                return PerformMovementLeftMouseButton(eventTimeDelta, dx, dy);
            }
            else if (buttonMask == IUiEventAdapter.MouseButtonMaskType.MiddleMouseButton ||
                     (buttonMask == IUiEventAdapter.MouseButtonMaskType.RightMouseButton &&
                      (modMask & IUiEventAdapter.ModKeyMaskType.ModKeyCtl) != 0) ||
                     (buttonMask == (IUiEventAdapter.MouseButtonMaskType.LeftMouseButton |
                                     IUiEventAdapter.MouseButtonMaskType.RightMouseButton)))
            {
                return PerformMovementMiddleMouseButton(eventTimeDelta, dx, dy);
            }
            else if (buttonMask == IUiEventAdapter.MouseButtonMaskType.RightMouseButton)
            {
                return PerformMovementRightMouseButton(eventTimeDelta, dx, dy);
            }

            return false;
        }
        
        
        // protected virtual bool PerformMovement(IUiEventAdapter actionAdapter)
        // {
        //     var dx = (InputStateTracker.MousePosition?.X - InputStateTracker.LastMousePosition?.X)/InputStateTracker.FrameSnapshot.WindowWidth;
        //     var dy = (InputStateTracker.MousePosition?.Y - InputStateTracker.LastMousePosition?.Y)/InputStateTracker.FrameSnapshot.WindowHeight;
        //
        //     if (dx == 0 && dy == 0) return false;
        //
        //     if (InputStateTracker.GetMouseButton(MouseButton.Left))
        //     {
        //         return PerformMovementLeftMouseButton(dx.Value, dy.Value, snapshot); 
        //     }
        //     else if (InputStateTracker.GetMouseButton(MouseButton.Right))
        //     {
        //         return PerformMovementRightMouseButton(dx.Value, dy.Value, snapshot); 
        //     }
        //
        //     return true;
        // }

        protected virtual bool PerformAnimiationMovement(IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter)
        {
            var f = ((eventAdapter.Time - AnimationData.StartTime).TotalSeconds / AnimationData.AnimationTime);
            if( f >= 1)
            {
                f = 1;
                AnimationData.IsAnimating = false;
                if( !_thrown )
                    actionAdapter.RequestContinuousUpdate( false );
            }

            ApplyAnimationStep( f, AnimationData.Phase );

            AnimationData.Phase = f;
            actionAdapter.RequestRedraw();

            return AnimationData.IsAnimating;
        }

        protected virtual void ApplyAnimationStep(double currentProgress, double previousProgress)
        {
        }

        protected virtual bool PerformMovementLeftMouseButton(double eventTimeDelta, float dx, float dy)
        {
            return false;
        }
        
        protected virtual bool PerformMovementMiddleMouseButton(double eventTimeDelta, float dx, float dy)
        {
            return false;
        }
        
        protected virtual bool PerformMovementRightMouseButton(double eventTimeDelta, float dx, float dy)
        {
            return false;
        }

        public virtual void SetTransformation(Vector3 eye, Vector3 center, Vector3 up, bool excludeRotation = false)
        {
        }

        public override void Home(IUiActionAdapter aa)
        {
            if (GetAutoComputeHomePosition())
            {
                if (aa is Viewer.IView view)
                {
                    ComputeHomePosition(view.Camera,
                        (_flags & UserInteractionFlags.ComputeHomeUsingBoundingBox) != 0);
                }
            }

            SetTransformation( _homeEye, _homeCenter, _homeUp );

            aa.RequestRedraw(); 
            aa.RequestContinuousUpdate( false );
            
        }
    }
}