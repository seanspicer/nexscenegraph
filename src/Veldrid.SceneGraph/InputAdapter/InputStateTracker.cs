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