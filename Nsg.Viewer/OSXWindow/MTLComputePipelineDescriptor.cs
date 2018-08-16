using System;

namespace Nsg.Viewer.OSXWindow
{
    public struct MTLComputePipelineDescriptor
    {
        public readonly IntPtr NativePtr;

        public MTLFunction computeFunction
        {
            get => ObjectiveCRuntime.objc_msgSend<MTLFunction>(NativePtr, sel_computeFunction);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setComputeFunction, value.NativePtr);
        }

        public MTLPipelineBufferDescriptorArray buffers
            => ObjectiveCRuntime.objc_msgSend<MTLPipelineBufferDescriptorArray>(NativePtr, sel_buffers);

        private static readonly Selector sel_computeFunction = "computeFunction";
        private static readonly Selector sel_setComputeFunction = "setComputeFunction:";
        private static readonly Selector sel_buffers = "buffers";
    }
}