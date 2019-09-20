using System.Collections.Generic;
using System.Numerics;
using System.Windows.Input;
using Veldrid.SceneGraph.InputAdapter;

namespace Veldrid.SceneGraph.Wpf
{
    public class WpfInputStateSnapshot : InputSnapshot, IInputStateSnapshot
    {
        public bool IsMouseDown(MouseButton button)
        {
            return _mouseDown[(int)button];
        }
        
        internal List<KeyEvent> KeyEventList { get; }
        public IReadOnlyList<KeyEvent> KeyEvents => KeyEventList;
        
        internal List<MouseEvent> MouseEventList { get; }
        public IReadOnlyList<MouseEvent> MouseEvents => MouseEventList;
        
        
        public IReadOnlyList<char> KeyCharPresses { get; }
        public Vector2 MousePosition { get; set; }
        public float WheelDelta { get; set; }
        public int WindowWidth { get; }
        public int WindowHeight { get; }

        private bool[] _mouseDown = new bool[13];
        public bool[] MouseDown => _mouseDown;
        
        internal WpfInputStateSnapshot()
        {
            MouseEventList = new List<MouseEvent>();
            KeyEventList = new List<KeyEvent>();
        }
    }
}