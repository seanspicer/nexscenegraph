using System;
using System.Runtime.InteropServices;

namespace Nsg.Viewer.OSXWindow
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CAMetalDrawable
    {
        public readonly IntPtr NativePtr;
        public bool IsNull => NativePtr == IntPtr.Zero;
        public MTLTexture texture => ObjectiveCRuntime.objc_msgSend<MTLTexture>(NativePtr, Selectors.texture);
    }
}