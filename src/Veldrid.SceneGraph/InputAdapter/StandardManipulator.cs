using System;
using System.Numerics;

namespace Veldrid.SceneGraph.InputAdapter
{
    public abstract class StandardManipulator : CameraManipulator
    {
        private INode _node;
        private float _modelSize;
        protected UserInteractionFlags _flags;
     
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
        
        public override void HandleInput(IInputStateSnapshot snapshot, IUiActionAdapter uiActionAdapter)
        {
            base.HandleInput(snapshot, uiActionAdapter);

            if (InputStateTracker.IsMouseButtonPushed())
            {
                HandleMouseButtonPushed(snapshot, uiActionAdapter);
            }

            if (InputStateTracker.IsMouseButtonReleased())
            {
                HandleMouseButtonReleased(snapshot, uiActionAdapter);
            }
            
            if (InputStateTracker.IsMouseButtonDown() && InputStateTracker.IsMouseMove())
            {
                HandleDrag(snapshot, uiActionAdapter);
            }
            
            else if (InputStateTracker.IsMouseMove())
            {
                HandleMouseMove(snapshot, uiActionAdapter);
            }

            if (InputStateTracker.FrameSnapshot.WheelDelta != 0)
            {
                HandleWheelDelta(snapshot, uiActionAdapter);
            }
        }
        
        protected virtual void HandleDrag(IInputStateSnapshot snapshot, IUiActionAdapter uiActionAdapter)
        {
            if (PerformMovement(snapshot))
            {
                uiActionAdapter.RequestRedraw();
            }
        }

        protected virtual void HandleMouseMove(IInputStateSnapshot snapshot, IUiActionAdapter uiActionAdapter)
        {
            //_logger.Debug(m => m("Move Event!"));
        }

        protected virtual void HandleMouseButtonPushed(IInputStateSnapshot snapshot, IUiActionAdapter uiActionAdapter)
        {
            //_logger.Debug(m => m("Button Pushed!"));
        }

        protected virtual void HandleMouseButtonReleased(IInputStateSnapshot snapshot, IUiActionAdapter uiActionAdapter)
        {
            //_logger.Debug(m => m("Button Released!"));
        }

        protected virtual void HandleWheelDelta(IInputStateSnapshot snapshot, IUiActionAdapter uiActionAdapter)
        {
            //_logger.Debug(m => m("Wheel Delta"));
        }

        protected virtual bool PerformMovement(IInputStateSnapshot snapshot)
        {
            var dx = (InputStateTracker.MousePosition?.X - InputStateTracker.LastMousePosition?.X)/InputStateTracker.FrameSnapshot.WindowWidth;
            var dy = (InputStateTracker.MousePosition?.Y - InputStateTracker.LastMousePosition?.Y)/InputStateTracker.FrameSnapshot.WindowHeight;

            if (dx == 0 && dy == 0) return false;

            if (InputStateTracker.GetMouseButton(MouseButton.Left))
            {
                return PerformMovementLeftMouseButton(dx.Value, dy.Value, snapshot); 
            }
            else if (InputStateTracker.GetMouseButton(MouseButton.Right))
            {
                return PerformMovementRightMouseButton(dx.Value, dy.Value, snapshot); 
            }

            return true;
        }

        protected virtual bool PerformMovementLeftMouseButton(float dx, float dy, IInputStateSnapshot snapshot)
        {
            return false;
        }
        
        protected virtual bool PerformMovementRightMouseButton(float dx, float dy, IInputStateSnapshot snapshot)
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