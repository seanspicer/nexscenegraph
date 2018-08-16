using System;
using System.Runtime.InteropServices;

namespace Nsg.Viewer.OSXWindow
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MTLRenderPassDescriptor
    {
        private static readonly ObjCClass s_class = new ObjCClass(nameof(MTLRenderPassDescriptor));
        public readonly IntPtr NativePtr;
        public static MTLRenderPassDescriptor New() => s_class.AllocInit<MTLRenderPassDescriptor>();

        public MTLRenderPassColorAttachmentDescriptorArray colorAttachments
            => ObjectiveCRuntime.objc_msgSend<MTLRenderPassColorAttachmentDescriptorArray>(NativePtr, sel_colorAttachments);

        public MTLRenderPassDepthAttachmentDescriptor depthAttachment
            => ObjectiveCRuntime.objc_msgSend<MTLRenderPassDepthAttachmentDescriptor>(NativePtr, sel_depthAttachment);

        public MTLRenderPassStencilAttachmentDescriptor stencilAttachment
            => ObjectiveCRuntime.objc_msgSend<MTLRenderPassStencilAttachmentDescriptor>(NativePtr, sel_stencilAttachment);

        private static readonly Selector sel_colorAttachments = "colorAttachments";
        private static readonly Selector sel_depthAttachment = "depthAttachment";
        private static readonly Selector sel_stencilAttachment = "stencilAttachment";
    }
}