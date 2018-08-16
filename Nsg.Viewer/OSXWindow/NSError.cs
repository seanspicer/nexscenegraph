using System;

namespace Nsg.Viewer.OSXWindow
{
    public struct NSError
    {
        public readonly IntPtr NativePtr;
        public string domain => ObjectiveCRuntime.string_objc_msgSend(NativePtr, "domain");
        public string localizedDescription => ObjectiveCRuntime.string_objc_msgSend(NativePtr, "localizedDescription");
    }
}