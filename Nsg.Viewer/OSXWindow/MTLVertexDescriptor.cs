using System;

namespace Nsg.Viewer.OSXWindow
{
    public unsafe struct MTLVertexDescriptor
    {
        public readonly IntPtr NativePtr;

        public MTLVertexBufferLayoutDescriptorArray layouts
            => ObjectiveCRuntime.objc_msgSend<MTLVertexBufferLayoutDescriptorArray>(NativePtr, sel_layouts);

        public MTLVertexAttributeDescriptorArray attributes
            => ObjectiveCRuntime.objc_msgSend<MTLVertexAttributeDescriptorArray>(NativePtr, sel_attributes);

        private static readonly Selector sel_layouts = "layouts";
        private static readonly Selector sel_attributes = "attributes";
    }
}