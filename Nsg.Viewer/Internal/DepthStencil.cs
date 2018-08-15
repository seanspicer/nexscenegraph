using Vulkan;

namespace Nsg.Viewer.Internal
{
    public struct DepthStencil
    {
        public VkImage Image;
        public VkDeviceMemory Mem;
        public VkImageView View;
    }
}