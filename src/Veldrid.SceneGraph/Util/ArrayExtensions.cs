using System.Runtime.InteropServices;

namespace Veldrid.SceneGraph.Util
{
    public static class ArrayExtensions
    {
        public static uint SizeInBytes<T>(this T[] array) where T : struct
        {
            var sizeOfVertexData = Marshal.SizeOf(default(T));
            return (uint)(array.Length * sizeOfVertexData);
        }
    }
}