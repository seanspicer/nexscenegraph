

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

        public void SetDirty()
        {
            _dirtyFlag = true;
        }
        
        public static IVertexBuffer<T> Create()
        {
            return new VertexBuffer<T>();
        }
        
        protected VertexBuffer()
        {
            
        }
        
        public T[] VertexData { get; set; }

        private int SizeOfVertexData => Unsafe.SizeOf<T>(); //Marshal.SizeOf(default(T));
        
        private Dictionary<GraphicsDevice, DeviceBuffer> _vertexBufferCache 
            = new Dictionary<GraphicsDevice, DeviceBuffer>();
        
        
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
            {
                return _vertexBufferCache[device];
            }
            else
            {
                throw new Exception("No vertex buffer for device");
            }
        }

        public void UpdateDeviceBuffers(GraphicsDevice device)
        {
            if (false == _dirtyFlag) return;
            
            if (_vertexBufferCache.TryGetValue(device, out var vbo))
            {
                device.UpdateBuffer(vbo, 0, VertexData);
            }
        }
    }
}