using System;

namespace Nsg.Viewer.OSXWindow
{
    public struct MTLPipelineBufferDescriptorArray
    {
        public readonly IntPtr NativePtr;

        public MTLPipelineBufferDescriptor this[uint index]
        {
            get
            {
                IntPtr value = ObjectiveCRuntime.IntPtr_objc_msgSend(NativePtr, Selectors.objectAtIndexedSubscript, (UIntPtr)index);
                return new MTLPipelineBufferDescriptor(value);
            }
            set
            {
                ObjectiveCRuntime.objc_msgSend(NativePtr, Selectors.setObjectAtIndexedSubscript, value.NativePtr, (UIntPtr)index);
            }
        }
    }
}