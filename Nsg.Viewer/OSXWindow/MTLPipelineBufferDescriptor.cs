using System;

namespace Nsg.Viewer.OSXWindow
{
    public struct MTLPipelineBufferDescriptor
    {
        public readonly IntPtr NativePtr;

        public MTLPipelineBufferDescriptor(IntPtr ptr) => NativePtr = ptr;

        public MTLMutability mutability
        {
            get => (MTLMutability)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_mutability);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setMutability, (uint)value);
        }

        private static readonly Selector sel_mutability = "mutability";
        private static readonly Selector sel_setMutability = "setMutability:";
    }
}