using System;
using System.Runtime.InteropServices;

namespace Nsg.Viewer.OSXWindow
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MTLCommandBuffer
    {
        public readonly IntPtr NativePtr;

        public MTLRenderCommandEncoder renderCommandEncoderWithDescriptor(MTLRenderPassDescriptor desc)
        {
            return new MTLRenderCommandEncoder(
                ObjectiveCRuntime.IntPtr_objc_msgSend(NativePtr, sel_renderCommandEncoderWithDescriptor, desc.NativePtr));
        }

        public void presentDrawable(IntPtr drawable) => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_presentDrawable, drawable);

        public void commit() => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_commit);

        public MTLBlitCommandEncoder blitCommandEncoder()
            => ObjectiveCRuntime.objc_msgSend<MTLBlitCommandEncoder>(NativePtr, sel_blitCommandEncoder);

        public MTLComputeCommandEncoder computeCommandEncoder()
            => ObjectiveCRuntime.objc_msgSend<MTLComputeCommandEncoder>(NativePtr, sel_computeCommandEncoder);

        public void waitUntilCompleted() => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_waitUntilCompleted);

        public void addCompletedHandler(MTLCommandBufferHandler block)
            => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_addCompletedHandler, block);
        public void addCompletedHandler(IntPtr block)
            => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_addCompletedHandler, block);

        public MTLCommandBufferStatus status => (MTLCommandBufferStatus)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_status);

        private static readonly Selector sel_renderCommandEncoderWithDescriptor = "renderCommandEncoderWithDescriptor:";
        private static readonly Selector sel_presentDrawable = "presentDrawable:";
        private static readonly Selector sel_commit = "commit";
        private static readonly Selector sel_blitCommandEncoder = "blitCommandEncoder";
        private static readonly Selector sel_computeCommandEncoder = "computeCommandEncoder";
        private static readonly Selector sel_waitUntilCompleted = "waitUntilCompleted";
        private static readonly Selector sel_addCompletedHandler = "addCompletedHandler:";
        private static readonly Selector sel_status = "status";
    }
}