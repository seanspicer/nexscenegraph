using System;

namespace Nsg.Viewer.OSXWindow
{
    public unsafe struct NSView
    {
        public readonly IntPtr NativePtr;
        public NSView(IntPtr ptr) => NativePtr = ptr;

        public Bool8 wantsLayer
        {
            get => ObjectiveCRuntime.bool8_objc_msgSend(NativePtr, "wantsLayer");
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, "setWantsLayer:", value);
        }

        public IntPtr layer
        {
            get => ObjectiveCRuntime.IntPtr_objc_msgSend(NativePtr, "layer");
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, "setLayer:", value);
        }

        public CGRect frame
        {
            get => ObjectiveCRuntime.objc_msgSend_stret<CGRect>(NativePtr, "frame");
        }
    }
}