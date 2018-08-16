using System.Runtime.InteropServices;

namespace Nsg.Viewer.OSXWindow
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MTLCommandBufferHandler(MTLCommandBuffer buffer);
}