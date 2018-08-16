using System;

namespace Nsg.Viewer.OSXWindow
{
    public unsafe struct NSWindow
    {
        public readonly IntPtr NativePtr;
        public NSWindow(IntPtr ptr)
        {
            NativePtr = ptr;
        }

        public NSView contentView => ObjectiveCRuntime.objc_msgSend<NSView>(NativePtr, "contentView");
    }
}