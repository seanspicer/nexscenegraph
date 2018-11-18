//
// Copyright (c) 2018 Sean Spicer
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System;
using System.Numerics;

namespace Veldrid.SceneGraph.InputAdapter
{
    public abstract class CameraManipulator : InputEventHandler, ICameraManipulator
    {
        protected abstract Matrix4x4 InverseMatrix { get; }

        protected ICamera _camera;
        
        protected CameraManipulator()
        {
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
            //Console.WriteLine("Move Event!");
        }

        protected virtual void HandleMouseButtonPushed()
        {
            //Console.WriteLine("Button Pushed!");
        }

        protected virtual void HandleMouseButtonReleased()
        {
            //Console.WriteLine("Button Released!");
        }

        protected virtual void HandleWheelDelta()
        {
            //Console.WriteLine("Wheel Delta");
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

            return true;
        }

        protected virtual bool PerformMovementLeftMouseButton(float dx, float dy)
        {
            return false;
        }
    }
}