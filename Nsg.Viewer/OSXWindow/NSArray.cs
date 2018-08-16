using System;

namespace Nsg.Viewer.OSXWindow
{
    public struct NSArray
    {
        public readonly IntPtr NativePtr;
        public NSArray(IntPtr ptr) => NativePtr = ptr;

        public UIntPtr count => ObjectiveCRuntime.UIntPtr_objc_msgSend(NativePtr, sel_count);
        private static readonly Selector sel_count = "count";
    }
}