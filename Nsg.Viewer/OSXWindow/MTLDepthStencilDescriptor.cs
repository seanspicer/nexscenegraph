using System;

namespace Nsg.Viewer.OSXWindow
{
    public struct MTLDepthStencilDescriptor
    {
        public readonly IntPtr NativePtr;
        public MTLDepthStencilDescriptor(IntPtr ptr) => NativePtr = ptr;

        public MTLCompareFunction depthCompareFunction
        {
            get => (MTLCompareFunction)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_depthCompareFunction);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setDepthCompareFunction, (uint)value);
        }

        public Bool8 depthWriteEnabled
        {
            get => ObjectiveCRuntime.bool8_objc_msgSend(NativePtr, sel_isDepthWriteEnabled);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setDepthWriteEnabled, value);
        }

        public MTLStencilDescriptor backFaceStencil
        {
            get => ObjectiveCRuntime.objc_msgSend<MTLStencilDescriptor>(NativePtr, sel_backFaceStencil);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setBackFaceStencil, value.NativePtr);
        }

        public MTLStencilDescriptor frontFaceStencil
        {
            get => ObjectiveCRuntime.objc_msgSend<MTLStencilDescriptor>(NativePtr, sel_frontFaceStencil);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setFrontFaceStencil, value.NativePtr);
        }

        private static readonly Selector sel_depthCompareFunction = "depthCompareFunction";
        private static readonly Selector sel_setDepthCompareFunction = "setDepthCompareFunction:";
        private static readonly Selector sel_isDepthWriteEnabled = "isDepthWriteEnabled";
        private static readonly Selector sel_setDepthWriteEnabled = "setDepthWriteEnabled:";
        private static readonly Selector sel_backFaceStencil = "backFaceStencil";
        private static readonly Selector sel_setBackFaceStencil = "setBackFaceStencil:";
        private static readonly Selector sel_frontFaceStencil = "frontFaceStencil";
        private static readonly Selector sel_setFrontFaceStencil = "setFrontFaceStencil:";
    }
}