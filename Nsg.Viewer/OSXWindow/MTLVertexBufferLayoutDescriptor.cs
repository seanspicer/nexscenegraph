using System;

namespace Nsg.Viewer.OSXWindow
{
    public struct MTLVertexBufferLayoutDescriptor
    {
        public readonly IntPtr NativePtr;

        public MTLVertexBufferLayoutDescriptor(IntPtr ptr) => NativePtr = ptr;

        public MTLVertexStepFunction stepFunction
        {
            get => (MTLVertexStepFunction)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_stepFunction);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setStepFunction, (uint)value);
        }

        public UIntPtr stride
        {
            get => ObjectiveCRuntime.UIntPtr_objc_msgSend(NativePtr, sel_stride);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setStride, value);
        }

        public UIntPtr stepRate
        {
            get => ObjectiveCRuntime.UIntPtr_objc_msgSend(NativePtr, sel_stepRate);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setStepRate, value);
        }

        private static readonly Selector sel_stepFunction = "stepFunction";
        private static readonly Selector sel_setStepFunction = "setStepFunction:";
        private static readonly Selector sel_stride = "stride";
        private static readonly Selector sel_setStride = "setStride:";
        private static readonly Selector sel_stepRate = "stepRate";
        private static readonly Selector sel_setStepRate = "setStepRate:";
    }
}