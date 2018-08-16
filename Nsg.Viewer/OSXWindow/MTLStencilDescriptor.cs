using System;

namespace Nsg.Viewer.OSXWindow
{
    public struct MTLStencilDescriptor
    {
        public readonly IntPtr NativePtr;

        public MTLStencilOperation stencilFailureOperation
        {
            get => (MTLStencilOperation)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_stencilFailureOperation);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setStencilFailureOperation, (uint)value);
        }

        public MTLStencilOperation depthFailureOperation
        {
            get => (MTLStencilOperation)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_depthFailureOperation);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setDepthFailureOperation, (uint)value);
        }

        public MTLStencilOperation depthStencilPassOperation
        {
            get => (MTLStencilOperation)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_depthStencilPassOperation);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setDepthStencilPassOperation, (uint)value);
        }

        public MTLCompareFunction stencilCompareFunction
        {
            get => (MTLCompareFunction)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_stencilCompareFunction);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setStencilCompareFunction, (uint)value);
        }

        public uint readMask
        {
            get => ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_readMask);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setReadMask, value);
        }

        public uint writeMask
        {
            get => ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_writeMask);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setWriteMask, value);
        }

        private static readonly Selector sel_depthFailureOperation = "depthFailureOperation";
        private static readonly Selector sel_stencilFailureOperation = "stencilFailureOperation";
        private static readonly Selector sel_setStencilFailureOperation = "setStencilFailureOperation:";
        private static readonly Selector sel_setDepthFailureOperation = "setDepthFailureOperation:";
        private static readonly Selector sel_depthStencilPassOperation = "depthStencilPassOperation";
        private static readonly Selector sel_setDepthStencilPassOperation = "setDepthStencilPassOperation:";
        private static readonly Selector sel_stencilCompareFunction = "stencilCompareFunction";
        private static readonly Selector sel_setStencilCompareFunction = "setStencilCompareFunction:";
        private static readonly Selector sel_readMask = "readMask";
        private static readonly Selector sel_setReadMask = "setReadMask:";
        private static readonly Selector sel_writeMask = "writeMask";
        private static readonly Selector sel_setWriteMask = "setWriteMask:";
    }
}