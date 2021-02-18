
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Veldrid.SceneGraph.Util;
using Vulkan.Xlib;

namespace Veldrid.SceneGraph.InputAdapter
{
    public class InputSnapshotAdapter
    {
        private HashSet<Key> _currentlyPressedKeys = new HashSet<Key>();
        private HashSet<Key> _pushedKeysThisFrame = new HashSet<Key>();
        private HashSet<Key> _releasedKeysThisFrame = new HashSet<Key>();

        private HashSet<MouseButton> _currentlyPressedMouseButtons = new HashSet<MouseButton>();
        private HashSet<MouseButton> _pushedMouseButtonsThisFrame = new HashSet<MouseButton>();
        private HashSet<MouseButton> _releasedMouseButtonsThisFrame = new HashSet<MouseButton>();

        private IUiEventAdapter.ModKeyMaskType ModKeyMask { get; set; }
        
        public System.Nullable<Vector2> MousePosition = null;
        public System.Nullable<Vector2> LastMousePosition = null;

        private Dictionary<Key, IUiEventAdapter.KeySymbol> KeyMap { get; } =
            new Dictionary<Key, IUiEventAdapter.KeySymbol>();

        public InputSnapshotAdapter()
        {
            BuildKeyMap();
            ModKeyMask = 0;
        }

        // For unit testing
        public IUiEventAdapter.KeySymbol MapKey(Key key)
        {
            if (KeyMap.TryGetValue(key, out var result))
            {
                return result;
            }

            return IUiEventAdapter.KeySymbol.Unknown;
        }
        
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
                
                adaptedEvent.ModKeyMask = ModKeyMask;
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
            else if(IsMouseButtonReleased())
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
                
                adaptedEvent.ModKeyMask = ModKeyMask;
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
            else if(IsMouseMove())
            {
                var adaptedEvent = UiEventAdapter.Create();
                adaptedEvent.EventType = IsMouseButtonDown() ? 
                    IUiEventAdapter.EventTypeValue.Drag : IUiEventAdapter.EventTypeValue.Move;
                
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

                adaptedEvent.ModKeyMask = ModKeyMask;
                adaptedEvent.X = snapshot.MousePosition.X;
                adaptedEvent.Y = snapshot.MousePosition.Y;
                adaptedEvent.XMin = 0;
                adaptedEvent.XMax = width;
                adaptedEvent.YMin = 0;
                adaptedEvent.YMax = height;
                adaptedEvents.Add(adaptedEvent);
            }
            
            if(snapshot.WheelDelta != 0)
            {
                var adaptedEvent = UiEventAdapter.Create();
                adaptedEvent.EventType = IUiEventAdapter.EventTypeValue.Scroll;
                adaptedEvent.ModKeyMask = ModKeyMask;
                if (snapshot.WheelDelta > 0)
                {
                    adaptedEvent.ScrollingMotion = IUiEventAdapter.ScrollingMotionType.ScrollUp;
                }
                else
                {
                    adaptedEvent.ScrollingMotion = IUiEventAdapter.ScrollingMotionType.ScrollDown;
                }
                adaptedEvent.X = snapshot.MousePosition.X;
                adaptedEvent.Y = snapshot.MousePosition.Y;
                adaptedEvent.XMin = 0;
                adaptedEvent.XMax = width;
                adaptedEvent.YMin = 0;
                adaptedEvent.YMax = height;
                adaptedEvents.Add(adaptedEvent);
            }

            if (IsKeyPressed())
            {
                foreach (var key in _pushedKeysThisFrame)
                {
                    if(KeyMap.TryGetValue(key, out var keyVal))
                    {
                        var adaptedEvent = UiEventAdapter.Create();
                        adaptedEvent.EventType = IUiEventAdapter.EventTypeValue.KeyDown;
                        adaptedEvent.ModKeyMask = ModKeyMask;
                        adaptedEvent.Key = keyVal;
                        adaptedEvent.X = snapshot.MousePosition.X;
                        adaptedEvent.Y = snapshot.MousePosition.Y;
                        adaptedEvent.XMin = 0;
                        adaptedEvent.XMax = width;
                        adaptedEvent.YMin = 0;
                        adaptedEvent.YMax = height;
                        adaptedEvents.Add(adaptedEvent);
                    }
                }
            }
            if (IsKeyReleased())
            {
                foreach (var key in _pushedKeysThisFrame)
                {
                    if(KeyMap.TryGetValue(key, out var keyVal))
                    {
                        var adaptedEvent = UiEventAdapter.Create();
                        adaptedEvent.EventType = IUiEventAdapter.EventTypeValue.KeyUp;
                        adaptedEvent.ModKeyMask = ModKeyMask;
                        adaptedEvent.Key = keyVal;
                        adaptedEvent.X = snapshot.MousePosition.X;
                        adaptedEvent.Y = snapshot.MousePosition.Y;
                        adaptedEvent.XMin = 0;
                        adaptedEvent.XMax = width;
                        adaptedEvent.YMin = 0;
                        adaptedEvent.YMax = height;
                        adaptedEvents.Add(adaptedEvent);
                    }
                }
            }
            
            return adaptedEvents;
        }

        private void UpdateState(InputSnapshot snapshot)
        {
            _pushedKeysThisFrame.Clear();
            _releasedKeysThisFrame.Clear();
            
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
            _pushedKeysThisFrame.Remove(key);
            _releasedKeysThisFrame.Add(key);
            
            switch (key)
            {
                case Key.ShiftLeft:
                    ModKeyMask &= ~IUiEventAdapter.ModKeyMaskType.ModKeyLeftShift;
                    break;
                case Key.ShiftRight:
                    ModKeyMask &= ~IUiEventAdapter.ModKeyMaskType.ModKeyRightShift;
                    break;
                case Key.ControlLeft:
                    ModKeyMask &= ~IUiEventAdapter.ModKeyMaskType.ModKeyLeftCtl;
                    break;
                case Key.ControlRight:
                    ModKeyMask &= ~IUiEventAdapter.ModKeyMaskType.ModKeyRightCtl;
                    break;
                case Key.AltLeft:
                    ModKeyMask &= ~IUiEventAdapter.ModKeyMaskType.ModKeyLeftAlt;
                    break;
                case Key.AltRight:
                    ModKeyMask &= ~IUiEventAdapter.ModKeyMaskType.ModKeyRightAlt;
                    break;
            }
        }

        private void KeyDown(Key key)
        {
            if (_currentlyPressedKeys.Add(key))
            {
                _pushedKeysThisFrame.Add(key);
            }

            switch (key)
            {
                case Key.ShiftLeft:
                    ModKeyMask |= IUiEventAdapter.ModKeyMaskType.ModKeyLeftShift;
                    break;
                case Key.ShiftRight:
                    ModKeyMask |= IUiEventAdapter.ModKeyMaskType.ModKeyRightShift;
                    break;
                case Key.ControlLeft:
                    ModKeyMask |= IUiEventAdapter.ModKeyMaskType.ModKeyLeftCtl;
                    break;
                case Key.ControlRight:
                    ModKeyMask |= IUiEventAdapter.ModKeyMaskType.ModKeyRightCtl;
                    break;
                case Key.AltLeft:
                    ModKeyMask |= IUiEventAdapter.ModKeyMaskType.ModKeyLeftAlt;
                    break;
                case Key.AltRight:
                    ModKeyMask |= IUiEventAdapter.ModKeyMaskType.ModKeyRightAlt;
                    break;
            }
        }
        
        public bool GetKey(Key key)
        {
            return _currentlyPressedKeys.Contains(key);
        }

        public bool GetKeyDown(Key key)
        {
            return _pushedKeysThisFrame.Contains(key);
        }

        public bool IsKeyPressed()
        {
            return _pushedKeysThisFrame.Count > 0;
        }

        public bool IsKeyReleased()
        {
            return _releasedKeysThisFrame.Count > 0;
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
        
        private void BuildKeyMap()
        {
            KeyMap.Add(Key.Unknown, IUiEventAdapter.KeySymbol.Unknown);
            KeyMap.Add(Key.ShiftLeft, IUiEventAdapter.KeySymbol.KeyShiftL);
            KeyMap.Add(Key.ShiftRight, IUiEventAdapter.KeySymbol.KeyShiftR);
            KeyMap.Add(Key.ControlLeft, IUiEventAdapter.KeySymbol.KeyControlL);
            KeyMap.Add(Key.ControlRight, IUiEventAdapter.KeySymbol.KeyControlR);
            KeyMap.Add(Key.AltLeft, IUiEventAdapter.KeySymbol.KeyAltL);
            KeyMap.Add(Key.AltRight, IUiEventAdapter.KeySymbol.KeyAltR);
            KeyMap.Add(Key.Menu, IUiEventAdapter.KeySymbol.KeyMenu);
            KeyMap.Add(Key.Space, IUiEventAdapter.KeySymbol.KeySpace);
            
            // Add Function Keys
            for (var i = 0; i < 35; ++i)
            {
                KeyMap.Add(Key.F1 + i, IUiEventAdapter.KeySymbol.KeyF1 + i);
            }

            // Add Letter Keys
            for (var i = 0; i < 26; ++i)
            {
                KeyMap.Add(Key.A + i, IUiEventAdapter.KeySymbol.KeyA + i);
            }
            
            // Add Number Keys
            for (var i = 0; i < 10; ++i)
            {
                KeyMap.Add(Key.Number0 + i, IUiEventAdapter.KeySymbol.Key0 + i);
            }
        }
    }
}