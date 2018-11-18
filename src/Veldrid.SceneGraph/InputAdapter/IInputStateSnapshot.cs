using System.Collections.Generic;
using System.Numerics;

namespace Veldrid.SceneGraph.InputAdapter
{
    public interface IInputStateSnapshot
    {
        bool IsMouseDown(MouseButton button);
        IReadOnlyList<KeyEvent> KeyEvents { get; }
        IReadOnlyList<MouseEvent> MouseEvents { get; }
        IReadOnlyList<char> KeyCharPresses { get; }
        Vector2 MousePosition { get; }
        float WheelDelta { get; }
        int WindowWidth { get; }
        int WindowHeight { get; }
    }
}