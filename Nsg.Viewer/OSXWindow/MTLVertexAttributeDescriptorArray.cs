using System;

namespace Nsg.Viewer.OSXWindow
{
    public struct MTLVertexAttributeDescriptorArray
    {
        public readonly IntPtr NativePtr;

        public MTLVertexAttributeDescriptor this[uint index]
        {
            get
            {
                IntPtr value = ObjectiveCRuntime.IntPtr_objc_msgSend(NativePtr, Selectors.objectAtIndexedSubscript, index);
                return new MTLVertexAttributeDescriptor(value);
            }
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, Selectors.setObjectAtIndexedSubscript, value.NativePtr, index);
        }
    }
}