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
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Veldrid.SceneGraph.InputAdapter
{
    public class InputStateTracker
    {
        private HashSet<Key> _currentlyPressedKeys = new HashSet<Key>();
        private HashSet<Key> _newKeysThisFrame = new HashSet<Key>();

        private HashSet<MouseButton> _currentlyPressedMouseButtons = new HashSet<MouseButton>();
        private HashSet<MouseButton> _pushedMouseButtonsThisFrame = new HashSet<MouseButton>();
        private HashSet<MouseButton> _releasedMouseButtonsThisFrame = new HashSet<MouseButton>();

        public System.Nullable<Vector2> MousePosition = null;
        public System.Nullable<Vector2> LastMousePosition = null;
        
        
        
        public IInputStateSnapshot FrameSnapshot { get; private set; }
        

        internal InputStateTracker()
        {
            
        }
        
        public bool GetKey(Key key)
        {
            return _currentlyPressedKeys.Contains(key);
        }

        public bool GetKeyDown(Key key)
        {
            return _newKeysThisFrame.Contains(key);
        }

        public bool GetMouseButton(MouseButton button)
        {
            return _currentlyPressedMouseButtons.Contains(button);
        }

        public bool GetMouseButtonDown(MouseButton button)
        {
            return _pushedMouseButtonsThisFrame.Contains(button);
        }

        public bool IsMouseButtonPushed()
        {
            return _pushedMouseButtonsThisFrame.Count > 0;
        }

        public bool IsMouseButtonReleased()
        {
            return _releasedMouseButtonsThisFrame.Count > 0;
        }
        
        public bool IsMouseButtonDown()
        {
            return _currentlyPressedMouseButtons.Count > 0;
        }

        public bool IsMouseMove()
        {
            if (null == MousePosition || null == LastMousePosition) return false;
            
            if (Math.Abs(MousePosition.Value.X - LastMousePosition.Value.X) > 1e-2) return true;
            
            else if (Math.Abs(MousePosition.Value.Y - LastMousePosition.Value.Y) > 1e-2) return true;

            return false;

        }
        
        
        public void UpdateFrameInput(IInputStateSnapshot snapshot)
        {
            FrameSnapshot = snapshot;
            _newKeysThisFrame.Clear();
            _pushedMouseButtonsThisFrame.Clear();
            _releasedMouseButtonsThisFrame.Clear();

            LastMousePosition = MousePosition;
            MousePosition = snapshot.MousePosition;

            for (int i = 0; i < snapshot.KeyEvents.Count; i++)
            {
                KeyEvent ke = snapshot.KeyEvents[i];
                if (ke.Down)
                {
                    KeyDown(ke.Key);
                }
                else
                {
                    KeyUp(ke.Key);
                }
            }
            for (int i = 0; i < snapshot.MouseEvents.Count; i++)
            {
                MouseEvent me = snapshot.MouseEvents[i];
                if (me.Down)
                {
                    MouseDown(me.MouseButton);
                }
                else
                {
                    MouseUp(me.MouseButton);
                }
            }
        }

        private void MouseUp(MouseButton mouseButton)
        {
            _currentlyPressedMouseButtons.Remove(mouseButton);
            _pushedMouseButtonsThisFrame.Remove(mouseButton);
            
            _releasedMouseButtonsThisFrame.Add(mouseButton);
        }

        private void MouseDown(MouseButton mouseButton)
        {
            if (_currentlyPressedMouseButtons.Add(mouseButton))
            {
                _pushedMouseButtonsThisFrame.Add(mouseButton);
            }
        }

        private void KeyUp(Key key)
        {
            _currentlyPressedKeys.Remove(key);
            _newKeysThisFrame.Remove(key);
        }

        private void KeyDown(Key key)
        {
            if (_currentlyPressedKeys.Add(key))
            {
                _newKeysThisFrame.Add(key);
            }
        }
    }
}