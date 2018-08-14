using System;
using Vulkan;

namespace Nsg.Viewer.Internal
{
    internal static class Util
    {
        public static void CheckResult(VkResult result)
        {
            if (result != VkResult.Success)
            {
                throw new InvalidOperationException("Call failed.");
            }
        }
        
        public static float DegreesToRadians(float degrees)
        {
            return degrees * (float)Math.PI / 180f;
        }
    }
}