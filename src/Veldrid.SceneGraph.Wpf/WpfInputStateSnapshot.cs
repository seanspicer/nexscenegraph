using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid.SceneGraph.InputAdapter;

namespace Veldrid.SceneGraph.Wpf
{
    public class WpfInputStateSnapshot : InputSnapshot, IInputStateSnapshot
    {
        public bool IsMouseDown(MouseButton button)
        {
            return (_mouseDown & button) != 0;
        }

        internal List<KeyEvent> KeyEventList { get; }
        public ReadOnlySpan<KeyEvent> KeyEvents => KeyEventList.ToArray();

        internal List<MouseButtonEvent> MouseEventList { get; }
        public ReadOnlySpan<MouseButtonEvent> MouseEvents => MouseEventList.ToArray();

        internal List<Rune> InputEventList { get; }
        public ReadOnlySpan<Rune> InputEvents => InputEventList.ToArray();

        public Vector2 MousePosition { get; set; }
        public Vector2 WheelDelta { get; set; }
        public int WindowWidth { get; }
        public int WindowHeight { get; }
        public Matrix4x4 ProjectionMatrix { get; }
        public Matrix4x4 ViewMatrix { get; }

        private MouseButton _mouseDown;
        public MouseButton MouseDown => _mouseDown;

        internal void SetMouseDown(MouseButton button, bool down)
        {
            if (down)
                _mouseDown |= button;
            else
                _mouseDown &= ~button;
        }

        internal WpfInputStateSnapshot()
        {
            MouseEventList = new List<MouseButtonEvent>();
            KeyEventList = new List<KeyEvent>();
            InputEventList = new List<Rune>();
        }
    }
}
