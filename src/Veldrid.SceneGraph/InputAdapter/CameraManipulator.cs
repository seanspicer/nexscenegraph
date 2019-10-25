//
// Copyright 2018-2019 Sean Spicer 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Numerics;
//using Common.Logging;

namespace Veldrid.SceneGraph.InputAdapter
{
    public abstract class CameraManipulator : InputEventHandler, ICameraManipulator
    {
        protected abstract Matrix4x4 InverseMatrix { get; }

        protected ICamera _camera;

        //private ILog _logger;
        
        protected CameraManipulator()
        {
            //_logger = LogManager.GetLogger<CameraManipulator>();
        }

        public void SetCamera(ICamera camera)
        {
            _camera = camera;
        }

        public void ViewAll()
        {
            ViewAll(20);
        }

        public abstract void ViewAll(float slack);

        // Update a camera
        public void UpdateCamera()
        {
            _camera.ViewMatrix = InverseMatrix;
        }
        
        public override void HandleInput(IInputStateSnapshot snapshot)
        {
            base.HandleInput(snapshot);

            if (InputStateTracker.IsMouseButtonPushed())
            {
                HandleMouseButtonPushed();
            }

            if (InputStateTracker.IsMouseButtonReleased())
            {
                HandleMouseButtonReleased();
            }
            
            if (InputStateTracker.IsMouseButtonDown() && InputStateTracker.IsMouseMove())
            {
                HandleDrag();
            }
            
            else if (InputStateTracker.IsMouseMove())
            {
                HandleMouseMove();
            }

            if (InputStateTracker.FrameSnapshot.WheelDelta != 0)
            {
                HandleWheelDelta();
            }
        }

        protected void RequestRedraw()
        {
            // TODO: This doesn't really make a request to redraw...
            UpdateCamera();
        }
        
        protected virtual void HandleDrag()
        {
            if (PerformMovement())
            {
                RequestRedraw();
            }
        }

        protected virtual void HandleMouseMove()
        {
            //_logger.Debug(m => m("Move Event!"));
        }

        protected virtual void HandleMouseButtonPushed()
        {
            //_logger.Debug(m => m("Button Pushed!"));
        }

        protected virtual void HandleMouseButtonReleased()
        {
            //_logger.Debug(m => m("Button Released!"));
        }

        protected virtual void HandleWheelDelta()
        {
            //_logger.Debug(m => m("Wheel Delta"));
        }

        protected virtual bool PerformMovement()
        {
            var dx = (InputStateTracker.MousePosition?.X - InputStateTracker.LastMousePosition?.X)/InputStateTracker.FrameSnapshot.WindowWidth;
            var dy = (InputStateTracker.MousePosition?.Y - InputStateTracker.LastMousePosition?.Y)/InputStateTracker.FrameSnapshot.WindowHeight;

            if (dx == 0 && dy == 0) return false;

            if (InputStateTracker.GetMouseButton(MouseButton.Left))
            {
                return PerformMovementLeftMouseButton(dx.Value, dy.Value); 
            }
            else if (InputStateTracker.GetMouseButton(MouseButton.Right))
            {
                return PerformMovementRightMouseButton(dx.Value, dy.Value); 
            }

            return true;
        }

        protected virtual bool PerformMovementLeftMouseButton(float dx, float dy)
        {
            return false;
        }
        
        protected virtual bool PerformMovementRightMouseButton(float dx, float dy)
        {
            return false;
        }
    }
}