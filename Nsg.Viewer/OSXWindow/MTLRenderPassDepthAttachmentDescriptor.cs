using System;

namespace Nsg.Viewer.OSXWindow
{
    public struct MTLRenderPassDepthAttachmentDescriptor
    {
        public readonly IntPtr NativePtr;

        public MTLRenderPassDepthAttachmentDescriptor(IntPtr ptr) => NativePtr = ptr;

        public MTLTexture texture
        {
            get => ObjectiveCRuntime.objc_msgSend<MTLTexture>(NativePtr, Selectors.texture);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, Selectors.setTexture, value.NativePtr);
        }

        public MTLLoadAction loadAction
        {
            get => (MTLLoadAction)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, Selectors.loadAction);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, Selectors.setLoadAction, (uint)value);
        }

        public MTLStoreAction storeAction
        {
            get => (MTLStoreAction)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, Selectors.storeAction);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, Selectors.setStoreAction, (uint)value);
        }

        public double clearDepth
        {
            get => ObjectiveCRuntime.double_objc_msgSend(NativePtr, sel_clearDepth);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setClearDepth, value);
        }

        public UIntPtr slice
        {
            get => ObjectiveCRuntime.UIntPtr_objc_msgSend(NativePtr, Selectors.slice);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, Selectors.setSlice, value);
        }

        public UIntPtr level
        {
            get => ObjectiveCRuntime.UIntPtr_objc_msgSend(NativePtr, Selectors.level);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, Selectors.setLevel, value);
        }

        private static readonly Selector sel_clearDepth = "clearDepth";
        private static readonly Selector sel_setClearDepth = "setClearDepth:";
    }
}