using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.DXGI;

namespace Veldrid.SceneGraph.Wpf.Element
{
    public static class DXUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T GetOrThrow<T>(this T obj)
            where T : class, IDisposable
        {
            if (obj == null)
                throw new ObjectDisposedException(typeof(T).Name);
            return obj;
        } 

        /// <summary>
        /// 
        /// </summary>
        public static SharpDX.Vector3 TransformNormal(this Matrix m, Vector3 v)
        {            
            var v2 = Multiply(m, v.X, v.Y, v.Z, 0);
            return new Vector3(v2.X, v2.Y, v2.Z);
        }

        /// <summary>
        /// 
        /// </summary>
        public static Vector3 TransformCoord(this Matrix m, Vector3 v)
        {
            var v2 = Multiply(m, v.X, v.Y, v.Z, 1);
            return new Vector3(v2.X, v2.Y, v2.Z);
        }

        /// <summary>
        /// 
        /// </summary>
        public static Vector3 Multiply(this Matrix m, float x, float y, float z, float w)
        {
            return new Vector3(
                m.M11 * x + m.M12 * y + m.M13 * z + m.M14 * w
                , m.M21 * x + m.M22 * y + m.M23 * z + m.M24 * w
                , m.M31 * x + m.M32 * y + m.M33 * z + m.M34 * w
                );
        }

        /// <summary>
        /// 
        /// </summary>
        public static float DEG2RAD(this float degrees)
        {
            return degrees * (float)Math.PI / 180.0f;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="flags"></param>
        /// <param name="format"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static SharpDX.Direct3D11.Texture2D CreateTexture2D(this SharpDX.Direct3D11.Device device,
            int w, int h,
            SharpDX.Direct3D11.BindFlags flags = SharpDX.Direct3D11.BindFlags.RenderTarget | SharpDX.Direct3D11.BindFlags.ShaderResource,
            Format format = Format.B8G8R8A8_UNorm,
            SharpDX.Direct3D11.ResourceOptionFlags options = SharpDX.Direct3D11.ResourceOptionFlags.Shared)
        {
            var colordesc = new SharpDX.Direct3D11.Texture2DDescription
            {
                BindFlags = flags,
                Format = format,
                Width = w,
                Height = h,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                OptionFlags = options,
                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                ArraySize = 1
            };
            return new SharpDX.Direct3D11.Texture2D(device, colordesc);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="device"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static SharpDX.Direct3D11.Buffer CreateBuffer<T>(this SharpDX.Direct3D11.Device device, T[] range)
            where T : struct
        {
            int sizeInBytes = Marshal.SizeOf(typeof(T));
            using (var stream = new DataStream(range.Length * sizeInBytes, true, true))
            {
                stream.WriteRange(range);
                return new SharpDX.Direct3D11.Buffer(device, stream, new SharpDX.Direct3D11.BufferDescription
                {
                    BindFlags = SharpDX.Direct3D11.BindFlags.VertexBuffer,
                    SizeInBytes = (int)stream.Length,
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                    OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                    StructureByteStride = 0,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                });
            }
        }
    }
}
