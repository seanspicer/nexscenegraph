using System;

namespace Nsg.Viewer.OSXWindow
{
    public struct MTLTextureDescriptor
    {
        private static readonly ObjCClass s_class = new ObjCClass(nameof(MTLTextureDescriptor));
        public readonly IntPtr NativePtr;
        public static MTLTextureDescriptor New() => s_class.AllocInit<MTLTextureDescriptor>();

        public MTLTextureType textureType
        {
            get => (MTLTextureType)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_textureType);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setTextureType, (uint)value);
        }

        public MTLPixelFormat pixelFormat
        {
            get => (MTLPixelFormat)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, Selectors.pixelFormat);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, Selectors.setPixelFormat, (uint)value);
        }

        public UIntPtr width
        {
            get => ObjectiveCRuntime.UIntPtr_objc_msgSend(NativePtr, sel_width);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setWidth, value);
        }

        public UIntPtr height
        {
            get => ObjectiveCRuntime.UIntPtr_objc_msgSend(NativePtr, sel_height);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setHeight, value);
        }

        public UIntPtr depth
        {
            get => ObjectiveCRuntime.UIntPtr_objc_msgSend(NativePtr, sel_depth);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setDepth, value);
        }

        public UIntPtr mipmapLevelCount
        {
            get => ObjectiveCRuntime.UIntPtr_objc_msgSend(NativePtr, sel_mipmapLevelCount);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setMipmapLevelCount, value);
        }

        public UIntPtr sampleCount
        {
            get => ObjectiveCRuntime.UIntPtr_objc_msgSend(NativePtr, sel_sampleCount);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setSampleCount, value);
        }

        public UIntPtr arrayLength
        {
            get => ObjectiveCRuntime.UIntPtr_objc_msgSend(NativePtr, sel_arrayLength);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setArrayLength, value);
        }

        public MTLResourceOptions resourceOptions
        {
            get => (MTLResourceOptions)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_resourceOptions);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setResourceOptions, (uint)value);
        }

        public MTLCPUCacheMode cpuCacheMode
        {
            get => (MTLCPUCacheMode)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_cpuCacheMode);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setCpuCacheMode, (uint)value);
        }

        public MTLStorageMode storageMode
        {
            get => (MTLStorageMode)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_storageMode);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setStorageMode, (uint)value);
        }

        public MTLTextureUsage textureUsage
        {
            get => (MTLTextureUsage)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_textureUsage);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setTextureUsage, (uint)value);
        }

        private static readonly Selector sel_textureType = "textureType";
        private static readonly Selector sel_setTextureType = "setTextureType:";
        private static readonly Selector sel_width = "width";
        private static readonly Selector sel_setWidth = "setWidth:";
        private static readonly Selector sel_height = "height";
        private static readonly Selector sel_setHeight = "setHeight:";
        private static readonly Selector sel_depth = "depth";
        private static readonly Selector sel_setDepth = "setDepth:";
        private static readonly Selector sel_mipmapLevelCount = "mipmapLevelCount";
        private static readonly Selector sel_setMipmapLevelCount = "setMipmapLevelCount:";
        private static readonly Selector sel_sampleCount = "sampleCount";
        private static readonly Selector sel_setSampleCount = "setSampleCount:";
        private static readonly Selector sel_arrayLength = "arrayLength";
        private static readonly Selector sel_setArrayLength = "setArrayLength:";
        private static readonly Selector sel_resourceOptions = "resourceOptions";
        private static readonly Selector sel_setResourceOptions = "setResourceOptions:";
        private static readonly Selector sel_cpuCacheMode = "cpuCacheMode";
        private static readonly Selector sel_setCpuCacheMode = "setCpuCacheMode:";
        private static readonly Selector sel_storageMode = "storageMode";
        private static readonly Selector sel_setStorageMode = "setStorageMode:";
        private static readonly Selector sel_textureUsage = "textureUsage";
        private static readonly Selector sel_setTextureUsage = "setTextureUsage:";
    }
}