
using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.SceneGraph.Util;
using Vulkan.Xlib;

namespace Veldrid.SceneGraph.InputAdapter
{
    public class InputSnapshotAdapter
    {
        private HashSet<Key> _currentlyPressedKeys = new HashSet<Key>();
        private HashSet<Key> _newKeysThisFrame = new HashSet<Key>();

        private HashSet<MouseButton> _currentlyPressedMouseButtons = new HashSet<MouseButton>();
        private HashSet<MouseButton> _pushedMouseButtonsThisFrame = new HashSet<MouseButton>();
        private HashSet<MouseButton> _releasedMouseButtonsThisFrame = new HashSet<MouseButton>();
        
        public System.Nullable<Vector2> MousePosition = null;
        public System.Nullable<Vector2> LastMousePosition = null;
        
        public IReadOnlyList<IUiEventAdapter> Adapt(InputSnapshot snapshot, float width, float height)
        {
            UpdateState(snapshot);
            
            var adaptedEvents = new List<IUiEventAdapter>();

            // Handle pushed mouse buttons
            if(IsMouseButtonPushed())
            {
                var adaptedEvent = UiEventAdapter.Create();
                adaptedEvent.EventType = IUiEventAdapter.EventTypeValue.Push;
                foreach (var button in _pushedMouseButtonsThisFrame)
                {
                    switch (button)
                    {
                        case MouseButton.Left:
                            adaptedEvent.MouseButtonMask |= IUiEventAdapter.MouseButtonMaskType.LeftMouseButton;
                            break;
                        case MouseButton.Middle:
                            adaptedEvent.MouseButtonMask |= IUiEventAdapter.MouseButtonMaskType.MiddleMouseButton;
                            break;
                        case MouseButton.Right:
                            adaptedEvent.MouseButtonMask |= IUiEventAdapter.MouseButtonMaskType.RightMouseButton;
                            break;
                    }
                }

                adaptedEvent.X = snapshot.MousePosition.X;
                adaptedEvent.Y = snapshot.MousePosition.Y;
                adaptedEvent.XMin = 0;
                adaptedEvent.XMax = width;
                adaptedEvent.YMin = 0;
                adaptedEvent.YMax = height;
                adaptedEvent.Time = DateTime.Now;  // Not sure this is really correct.
                adaptedEvents.Add(adaptedEvent);
            }

            // Handle released mouse buttons
            if(IsMouseButtonReleased())
            {
                var adaptedEvent = UiEventAdapter.Create();
                adaptedEvent.EventType = IUiEventAdapter.EventTypeValue.Release;
                foreach (var button in _releasedMouseButtonsThisFrame)
                {
                    switch (button)
                    {
                        case MouseButton.Left:
                            adaptedEvent.MouseButtonMask |= IUiEventAdapter.MouseButtonMaskType.LeftMouseButton;
                            break;
                        case MouseButton.Middle:
                            adaptedEvent.MouseButtonMask |= IUiEventAdapter.MouseButtonMaskType.MiddleMouseButton;
                            break;
                        case MouseButton.Right:
                            adaptedEvent.MouseButtonMask |= IUiEventAdapter.MouseButtonMaskType.RightMouseButton;
                            break;
                    }
                }

                adaptedEvent.X = snapshot.MousePosition.X;
                adaptedEvent.Y = snapshot.MousePosition.Y;
                adaptedEvent.XMin = 0;
                adaptedEvent.XMax = width;
                adaptedEvent.YMin = 0;
                adaptedEvent.YMax = height;
                adaptedEvent.Time = DateTime.Now;  // Not sure this is really correct.
                adaptedEvents.Add(adaptedEvent);
            }

            // Handle mouse button drag
            if(IsMouseMove())
            {
                var adaptedEvent = UiEventAdapter.Create();
                adaptedEvent.EventType = IUiEventAdapter.EventTypeValue.Drag;
                foreach (var button in _currentlyPressedMouseButtons)
                {
                    switch (button)
                    {
                        case MouseButton.Left:
                            adaptedEvent.MouseButtonMask |= IUiEventAdapter.MouseButtonMaskType.LeftMouseButton;
                            break;
                        case MouseButton.Middle:
                            adaptedEvent.MouseButtonMask |= IUiEventAdapter.MouseButtonMaskType.MiddleMouseButton;
                            break;
                        case MouseButton.Right:
                            adaptedEvent.MouseButtonMask |= IUiEventAdapter.MouseButtonMaskType.RightMouseButton;
                            break;
                    }
                }

                adaptedEvent.X = snapshot.MousePosition.X;
                adaptedEvent.Y = snapshot.MousePosition.Y;
                adaptedEvent.XMin = 0;
                adaptedEvent.XMax = width;
                adaptedEvent.YMin = 0;
                adaptedEvent.YMax = height;
                adaptedEvent.Time = DateTime.Now;  // Not sure this is really correct.
                adaptedEvents.Add(adaptedEvent);
            }
            
            return adaptedEvents;
        }

        private void UpdateState(InputSnapshot snapshot)
        {
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
            
            if ( System.Math.Abs(MousePosition.Value.X - LastMousePosition.Value.X) > 1e-2) return true;
            
            else if ( System.Math.Abs(MousePosition.Value.Y - LastMousePosition.Value.Y) > 1e-2) return true;

            return false;

        }
    }
}