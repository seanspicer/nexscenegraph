using System;
using Nsg.Viewer.Internal;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Nsg.Viewer.OSXWindow
{
    public unsafe class OSXUtils
    {
        public static VkSurfaceKHR CreateNSWindow(VkInstance instance, IntPtr nsWindow)
        {
            CAMetalLayer metalLayer = CAMetalLayer.New();
            NSWindow nswindow = new NSWindow(nsWindow);
            NSView contentView = nswindow.contentView;
            contentView.wantsLayer = true;
            contentView.layer = metalLayer.NativePtr;

            VkMacOSSurfaceCreateInfoMVK surfaceCI = VkMacOSSurfaceCreateInfoMVK.New();
            surfaceCI.pView = contentView.NativePtr.ToPointer();
            VkResult result = vkCreateMacOSSurfaceMVK(instance, ref surfaceCI, null, out VkSurfaceKHR surface);
            Util.CheckResult(result);
            return surface;
        }
    }
}