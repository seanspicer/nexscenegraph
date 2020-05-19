using Veldrid.SceneGraph.Wpf.Win32;

namespace Veldrid.SceneGraph.Wpf.Controls
{
    public static class HwndMouse
    {
        public static System.Windows.Point GetCursorPosition()
        {
            var point = new Veldrid.SceneGraph.Wpf.Win32.NativeMethods.NativePoint();
            Veldrid.SceneGraph.Wpf.Win32.NativeMethods.GetCursorPos(ref point);
            return new System.Windows.Point(point.X, point.Y);
        }

        public static void SetCursorPosition(System.Windows.Point point)
        {
            Veldrid.SceneGraph.Wpf.Win32.NativeMethods.SetCursorPos((int) point.X, (int) point.Y);
        }

        public static void ShowCursor()
        {
            Veldrid.SceneGraph.Wpf.Win32.NativeMethods.ShowCursor(true);
        }

        public static void HideCursor()
        {
            Veldrid.SceneGraph.Wpf.Win32.NativeMethods.ShowCursor(false);
        }
    }
}