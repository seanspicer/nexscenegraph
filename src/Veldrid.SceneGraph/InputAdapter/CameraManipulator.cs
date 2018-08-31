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

namespace Veldrid.SceneGraph.InputAdapter
{
    public class CameraManipulator : IInputEventHandler
    {
        protected InputStateTracker _inputStateTracker = new InputStateTracker();

        public event Action RequestRedraw;
        
        public CameraManipulator()
        {
            
        }
        
        public void HandleInput(InputStateSnapshot snapshot)
        {
            
            _inputStateTracker.UpdateFrameInput(snapshot);

            if (_inputStateTracker.IsMouseButtonPushed())
            {
                HandleMouseButtonPushed();
            }

            if (_inputStateTracker.IsMouseButtonReleased())
            {
                HandleMouseButtonReleased();
            }
            
            if (_inputStateTracker.IsMouseButtonDown() && _inputStateTracker.IsMouseMove())
            {
                HandleDrag();
            }
            
            else if (_inputStateTracker.IsMouseMove())
            {
                HandleMouseMove();
            }
        }

        protected virtual void HandleDrag()
        {
            if (PerformMovement())
            {
                RequestRedraw?.Invoke();
            }
        }

        protected virtual void HandleMouseMove()
        {
            Console.WriteLine("Move Event!");
        }

        protected virtual void HandleMouseButtonPushed()
        {
            Console.WriteLine("Button Pushed!");
        }

        protected virtual void HandleMouseButtonReleased()
        {
            Console.WriteLine("Button Released!");
        }

        protected virtual bool PerformMovement()
        {
            var dx = (_inputStateTracker.MousePosition?.X - _inputStateTracker.LastMousePosition?.X)/_inputStateTracker.FrameSnapshot.WindowWidth;
            var dy = (_inputStateTracker.MousePosition?.Y - _inputStateTracker.LastMousePosition?.Y)/_inputStateTracker.FrameSnapshot.WindowHeight;

            if (dx == 0 && dy == 0) return false;

            if (_inputStateTracker.GetMouseButton(MouseButton.Left))
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