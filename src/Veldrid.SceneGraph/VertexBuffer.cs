using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Veldrid.SceneGraph
{
    public interface IVertexBuffer
    {
        void ConfigureDeviceBuffers(GraphicsDevice device, ResourceFactory factory);
        DeviceBuffer GetVertexBufferForDevice(GraphicsDevice device);
        void UpdateDeviceBuffers(GraphicsDevice device);
        void SetDirty();
    }

    public interface IVertexBuffer<T> : IVertexBuffer where T : struct
    {
        T[] VertexData { get; set; }
    }

    public class VertexBuffer<T> : IVertexBuffer<T> where T : struct
    {
        private bool _dirtyFlag;

        private readonly Dictionary<GraphicsDevice, DeviceBuffer> _vertexBufferCache
            = new Dictionary<GraphicsDevice, DeviceBuffer>();

        protected VertexBuffer()
        {
        }

        private int SizeOfVertexData => Unsafe.SizeOf<T>(); //Marshal.SizeOf(default(T));

        public void SetDirty()
        {
            _dirtyFlag = true;
        }

        public T[] VertexData { get; set; }


        public void ConfigureDeviceBuffers(GraphicsDevice device, ResourceFactory factory)
        {
            if (false == _vertexBufferCache.TryGetValue(device, out var vbo))
            {
                var vtxBufferDesc =
                    new BufferDescription((uint) (VertexData.Length * SizeOfVertexData), BufferUsage.VertexBuffer);

                vbo = factory.CreateBuffer(vtxBufferDesc);

                device.UpdateBuffer(vbo, 0, VertexData);

                _vertexBufferCache.Add(device, vbo);
            }
            else
            {
                UpdateDeviceBuffers(device);
            }

            _dirtyFlag = false;
        }

        public DeviceBuffer GetVertexBufferForDevice(GraphicsDevice device)
        {
            if (_vertexBufferCache.ContainsKey(device))
                return _vertexBufferCache[device];
            throw new Exception("No vertex buffer for device");
        }

        public void UpdateDeviceBuffers(GraphicsDevice device)
        {
            if (false == _dirtyFlag) return;

            if (_vertexBufferCache.TryGetValue(device, out var vbo))
            {
                device.UpdateBuffer(vbo, 0, VertexData);
            }

            _dirtyFlag = false;
        }

        public static IVertexBuffer<T> Create()
        {
            return new VertexBuffer<T>();
        }
    }
}