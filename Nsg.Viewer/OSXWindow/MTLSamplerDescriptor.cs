using System;

namespace Nsg.Viewer.OSXWindow
{
    public struct MTLSamplerDescriptor
    {
        private static readonly ObjCClass s_class = new ObjCClass(nameof(MTLSamplerDescriptor));
        public readonly IntPtr NativePtr;
        public static MTLSamplerDescriptor New() => s_class.AllocInit<MTLSamplerDescriptor>();

        public MTLSamplerAddressMode rAddressMode
        {
            get => (MTLSamplerAddressMode)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_rAddressMode);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setRAddressMode, (uint)value);
        }

        public MTLSamplerAddressMode sAddressMode
        {
            get => (MTLSamplerAddressMode)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_sAddressMode);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setSAddressMode, (uint)value);
        }

        public MTLSamplerAddressMode tAddressMode
        {
            get => (MTLSamplerAddressMode)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_tAddressMode);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setTAddressMode, (uint)value);
        }

        public MTLSamplerMinMagFilter minFilter
        {
            get => (MTLSamplerMinMagFilter)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_minFilter);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setMinFilter, (uint)value);
        }

        public MTLSamplerMinMagFilter magFilter
        {
            get => (MTLSamplerMinMagFilter)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_magFilter);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setMagFilter, (uint)value);
        }

        public MTLSamplerMipFilter mipFilter
        {
            get => (MTLSamplerMipFilter)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_mipFilter);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setMipFilter, (uint)value);
        }

        public float lodMinClamp
        {
            get => ObjectiveCRuntime.float_objc_msgSend(NativePtr, sel_lodMinClamp);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setLodMinClamp, value);
        }

        public float lodMaxClamp
        {
            get => ObjectiveCRuntime.float_objc_msgSend(NativePtr, sel_lodMaxClamp);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setLodMaxClamp, value);
        }

        public Bool8 lodAverage
        {
            get => ObjectiveCRuntime.bool8_objc_msgSend(NativePtr, sel_lodAverage);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setLodAverage, value);
        }

        public UIntPtr maxAnisotropy
        {
            get => ObjectiveCRuntime.UIntPtr_objc_msgSend(NativePtr, sel_maxAnisotropy);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setMaAnisotropy, value);
        }

        public MTLCompareFunction compareFunction
        {
            get => (MTLCompareFunction)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_compareFunction);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setCompareFunction, (uint)value);
        }

        public MTLSamplerBorderColor borderColor
        {
            get => (MTLSamplerBorderColor)ObjectiveCRuntime.uint_objc_msgSend(NativePtr, sel_borderColor);
            set => ObjectiveCRuntime.objc_msgSend(NativePtr, sel_setBorderColor, (uint)value);
        }

        private static readonly Selector sel_rAddressMode = "rAddressMode";
        private static readonly Selector sel_setRAddressMode = "setRAddressMode:";
        private static readonly Selector sel_sAddressMode = "sAddressMode";
        private static readonly Selector sel_setSAddressMode = "setSAddressMode:";
        private static readonly Selector sel_tAddressMode = "tAddressMode";
        private static readonly Selector sel_setTAddressMode = "setTAddressMode:";
        private static readonly Selector sel_minFilter = "minFilter";
        private static readonly Selector sel_setMinFilter = "setMinFilter:";
        private static readonly Selector sel_magFilter = "magFilter";
        private static readonly Selector sel_setMagFilter = "setMagFilter:";
        private static readonly Selector sel_mipFilter = "mipFilter";
        private static readonly Selector sel_setMipFilter = "setMipFilter:";
        private static readonly Selector sel_lodMinClamp = "lodMinClamp";
        private static readonly Selector sel_setLodMinClamp = "setLodMinClamp:";
        private static readonly Selector sel_lodMaxClamp = "lodMaxClamp";
        private static readonly Selector sel_setLodMaxClamp = "setLodMaxClamp:";
        private static readonly Selector sel_lodAverage = "lodAverage";
        private static readonly Selector sel_setLodAverage = "setLodAverage:";
        private static readonly Selector sel_maxAnisotropy = "maxAnisotropy";
        private static readonly Selector sel_setMaAnisotropy = "setMaxAnisotropy:";
        private static readonly Selector sel_compareFunction = "compareFunction";
        private static readonly Selector sel_setCompareFunction = "setCompareFunction:";
        private static readonly Selector sel_borderColor = "borderColor";
        private static readonly Selector sel_setBorderColor = "setBorderColor:";
    }
}