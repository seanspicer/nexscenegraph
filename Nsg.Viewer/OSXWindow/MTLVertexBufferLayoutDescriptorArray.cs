using System;

namespace Nsg.Viewer.OSXWindow
{
    public struct MTLVertexBufferLayoutDescriptorArray
    {
        public readonly IntPtr NativePtr;

        public MTLVertexBufferLayoutDescriptor this[uint index]
        {
            get
            {
                IntPtr value = ObjectiveCRuntime.IntPtr_objc_msgSend(NativePtr, Selectors.objectAtIndexedSubscript, index);
                return new MTLVertexBufferLayoutDescriptor(value);
            }
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, Selectors.setObjectAtIndexedSubscript, value.NativePtr, index);
        }
    }
}