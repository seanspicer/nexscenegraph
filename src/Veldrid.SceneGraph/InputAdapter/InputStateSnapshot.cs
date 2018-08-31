using System.Collections.Generic;
using System.Numerics;

namespace Veldrid.SceneGraph.InputAdapter
{
    public class InputStateSnapshot : InputSnapshot
    {
        private InputSnapshot _snapshot;
        
        public InputStateSnapshot(InputSnapshot snapshot, int width, int height)
        {
            WindowWidth = width;
            WindowHeight = height;
            _snapshot = snapshot;
        }

        public bool IsMouseDown(MouseButton button)
        {
            return _snapshot.IsMouseDown(button);
        }

        public IReadOnlyList<KeyEvent> KeyEvents => _snapshot.KeyEvents;
        public IReadOnlyList<MouseEvent> MouseEvents => _snapshot.MouseEvents;
        public IReadOnlyList<char> KeyCharPresses => _snapshot.KeyCharPresses;
        public Vector2 MousePosition => _snapshot.MousePosition;
        public float WheelDelta => _snapshot.WheelDelta;
        
        public int WindowWidth { get; private set; }
        public int WindowHeight { get; private set; }
    }
}