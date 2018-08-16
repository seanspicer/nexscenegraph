using System;

namespace Nsg.Viewer.OSXWindow
{
    public struct MTLRenderPipelineState
    {
        public readonly IntPtr NativePtr;
        public MTLRenderPipelineState(IntPtr ptr) => NativePtr = ptr;
    }
}