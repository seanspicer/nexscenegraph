using System;

namespace Nsg.Viewer.OSXWindow
{
    public struct CALayer
    {
        public readonly IntPtr NativePtr;
        public CALayer(IntPtr ptr) => NativePtr = ptr;

        public void addSublayer(IntPtr layer)
        {
            ObjectiveCRuntime.objc_msgSend(NativePtr, "addSublayer:", layer);
        }
    }
}