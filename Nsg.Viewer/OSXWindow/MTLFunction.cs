using System;

namespace Nsg.Viewer.OSXWindow
{
    public struct MTLFunction
    {
        public readonly IntPtr NativePtr;
        public MTLFunction(IntPtr ptr) => NativePtr = ptr;

        public NSDictionary functionConstantsDictionary => ObjectiveCRuntime.objc_msgSend<NSDictionary>(NativePtr, sel_functionConstantsDictionary);

        private static readonly Selector sel_functionConstantsDictionary = "functionConstantsDictionary";
    }
}