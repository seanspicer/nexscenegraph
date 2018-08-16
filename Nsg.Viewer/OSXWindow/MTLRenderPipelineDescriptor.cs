using System;
using System.Runtime.InteropServices;

namespace Nsg.Viewer.OSXWindow
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MTLRenderPipelineDescriptor
    {
        public readonly IntPtr NativePtr;

        public MTLRenderPipelineDescriptor(IntPtr ptr) => NativePtr = ptr;

        public static MTLRenderPipelineDescriptor New()
        {
            var cls = new ObjCClass("MTLRenderPipelineDescriptor");
            var ret = cls.AllocInit<MTLRenderPipelineDescriptor>();
            return ret;
        }

        public MTLFunction vertexFunction
        {
            get => ObjectiveCRuntime.objc_msgSend<MTLFunction>(NativePtr, sel_vertexFunction);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setVertexFunction, value.NativePtr);
        }

        public MTLFunction fragmentFunction
        {
            get => ObjectiveCRuntime.objc_msgSend<MTLFunction>(NativePtr, sel_fragmentFunction);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setFragmentFunction, value.NativePtr);
        }

        public MTLRenderPipelineColorAttachmentDescriptorArray colorAttachments
            => ObjectiveCRuntime.objc_msgSend<MTLRenderPipelineColorAttachmentDescriptorArray>(NativePtr, sel_colorAttachments);

        public MTLPixelFormat depthAttachmentPixelFormat
        {
            get => (MTLPixelFormat)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_depthAttachmentPixelFormat);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setDepthAttachmentPixelFormat, (uint)value);
        }

        public MTLPixelFormat stencilAttachmentPixelFormat
        {
            get => (MTLPixelFormat)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_stencilAttachmentPixelFormat);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setStencilAttachmentPixelFormat, (uint)value);
        }

        public UIntPtr sampleCount
        {
            get => ObjectiveCRuntime.UIntPtr_objc_msgSend(NativePtr, sel_sampleCount);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setSampleCount, value);
        }

        public MTLVertexDescriptor vertexDescriptor => ObjectiveCRuntime.objc_msgSend<MTLVertexDescriptor>(NativePtr, sel_vertexDescriptor);

        private static readonly Selector sel_vertexFunction = "vertexFunction";
        private static readonly Selector sel_setVertexFunction = "setVertexFunction:";
        private static readonly Selector sel_fragmentFunction = "fragmentFunction";
        private static readonly Selector sel_setFragmentFunction = "setFragmentFunction:";
        private static readonly Selector sel_colorAttachments = "colorAttachments";
        private static readonly Selector sel_depthAttachmentPixelFormat = "depthAttachmentPixelFormat";
        private static readonly Selector sel_setDepthAttachmentPixelFormat = "setDepthAttachmentPixelFormat:";
        private static readonly Selector sel_stencilAttachmentPixelFormat = "stencilAttachmentPixelFormat";
        private static readonly Selector sel_setStencilAttachmentPixelFormat = "setStencilAttachmentPixelFormat:";
        private static readonly Selector sel_sampleCount = "sampleCount";
        private static readonly Selector sel_setSampleCount = "setSampleCount:";
        private static readonly Selector sel_vertexDescriptor = "vertexDescriptor";
    }
}