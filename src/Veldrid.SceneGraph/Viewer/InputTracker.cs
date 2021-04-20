//
// Copyright 2018-2021 Sean Spicer 
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

using System.Collections.Generic;
using System.Numerics;

namespace Veldrid.SceneGraph.Viewer
{
    internal static class InputTracker
    {
        private static readonly HashSet<Key> _currentlyPressedKeys = new HashSet<Key>();
        private static readonly HashSet<Key> _newKeysThisFrame = new HashSet<Key>();

        private static readonly HashSet<MouseButton> _currentlyPressedMouseButtons = new HashSet<MouseButton>();
        private static readonly HashSet<MouseButton> _newMouseButtonsThisFrame = new HashSet<MouseButton>();

        public static Vector2 MousePosition;
        public static InputSnapshot FrameSnapshot { get; private set; }

        public static bool GetKey(Key key)
        {
            return _currentlyPressedKeys.Contains(key);
        }

        public static bool GetKeyDown(Key key)
        {
            return _newKeysThisFrame.Contains(key);
        }

        public static bool GetMouseButton(MouseButton button)
        {
            return _currentlyPressedMouseButtons.Contains(button);
        }

        public static bool GetMouseButtonDown(MouseButton button)
        {
            return _newMouseButtonsThisFrame.Contains(button);
        }

        public static void UpdateFrameInput(InputSnapshot snapshot)
        {
            FrameSnapshot = snapshot;
            _newKeysThisFrame.Clear();
            _newMouseButtonsThisFrame.Clear();

            MousePosition = snapshot.MousePosition;
            for (var i = 0; i < snapshot.KeyEvents.Count; i++)
            {
                var ke = snapshot.KeyEvents[i];
                if (ke.Down)
                    KeyDown(ke.Key);
                else
                    KeyUp(ke.Key);
            }

            for (var i = 0; i < snapshot.MouseEvents.Count; i++)
            {
                var me = snapshot.MouseEvents[i];
                if (me.Down)
                    MouseDown(me.MouseButton);
                else
                    MouseUp(me.MouseButton);
            }
        }

        private static void MouseUp(MouseButton mouseButton)
        {
            _currentlyPressedMouseButtons.Remove(mouseButton);
            _newMouseButtonsThisFrame.Remove(mouseButton);
        }

        private static void MouseDown(MouseButton mouseButton)
        {
            if (_currentlyPressedMouseButtons.Add(mouseButton)) _newMouseButtonsThisFrame.Add(mouseButton);
        }

        private static void KeyUp(Key key)
        {
            _currentlyPressedKeys.Remove(key);
            _newKeysThisFrame.Remove(key);
        }

        private static void KeyDown(Key key)
        {
            if (_currentlyPressedKeys.Add(key)) _newKeysThisFrame.Add(key);
        }
    }
}