using System;
using System.Runtime.InteropServices;

namespace Nsg.Viewer.OSXWindow
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MTLTexture
    {
        public readonly IntPtr NativePtr;

        public MTLTexture(IntPtr ptr) => NativePtr = ptr;
        public bool IsNull => NativePtr == IntPtr.Zero;

        public void replaceRegion(
            MTLRegion region,
            UIntPtr mipmapLevel,
            UIntPtr slice,
            void* pixelBytes,
            UIntPtr bytesPerRow,
            UIntPtr bytesPerImage)
        {
            ObjectiveCRuntime.objc_msgSend(NativePtr, sel_replaceRegion,
                region,
                mipmapLevel,
                slice,
                (IntPtr)pixelBytes,
                bytesPerRow,
                bytesPerImage);
        }

        public MTLTexture newTextureView(
            MTLPixelFormat pixelFormat,
            MTLTextureType textureType,
            NSRange levelRange,
            NSRange sliceRange)
        {
            IntPtr ret = ObjectiveCRuntime.IntPtr_objc_msgSend(NativePtr, sel_newTextureView,
                (uint)pixelFormat, (uint)textureType, levelRange, sliceRange);
            return new MTLTexture(ret);
        }

        private static readonly Selector sel_replaceRegion = "replaceRegion:mipmapLevel:slice:withBytes:bytesPerRow:bytesPerImage:";
        private static readonly Selector sel_newTextureView = "newTextureViewWithPixelFormat:textureType:levels:slices:";
    }
}