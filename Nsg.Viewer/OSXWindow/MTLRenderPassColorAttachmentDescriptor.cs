using System;

namespace Nsg.Viewer.OSXWindow
{
    public struct MTLRenderPassColorAttachmentDescriptor
    {
        public readonly IntPtr NativePtr;
        public MTLRenderPassColorAttachmentDescriptor(IntPtr ptr) => NativePtr = ptr;

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

        public MTLTexture resolveTexture
        {
            get => ObjectiveCRuntime.objc_msgSend<MTLTexture>(NativePtr, Selectors.resolveTexture);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, Selectors.setResolveTexture, value.NativePtr);
        }

        public MTLClearColor clearColor
        {
            get
            {
                if (ObjectiveCRuntime.UseStret<MTLClearColor>())
                {
                    return ObjectiveCRuntime.objc_msgSend_stret<MTLClearColor>(NativePtr, sel_clearColor);
                }
                else
                {
                    return ObjectiveCRuntime.MTLClearColor_objc_msgSend(NativePtr,sel_clearColor);
                }
            }
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setClearColor, value);
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

        private static readonly Selector sel_clearColor = "clearColor";
        private static readonly Selector sel_setClearColor = "setClearColor:";
    }
}