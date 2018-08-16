using System;

namespace Nsg.Viewer.OSXWindow
{
    public struct MTLRenderPassStencilAttachmentDescriptor
    {
        public readonly IntPtr NativePtr;

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

        public uint clearStencil
        {
            get => ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_clearStencil);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setClearStencil, value);
        }

        public UIntPtr slice
        {
            get => ObjectiveCRuntime.UIntPtr_objc_msgSend(NativePtr, Selectors.slice);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, Selectors.setSlice, value);
        }

        private static readonly Selector sel_clearStencil = "clearStencil";
        private static readonly Selector sel_setClearStencil = "setClearStencil:";
    }
}