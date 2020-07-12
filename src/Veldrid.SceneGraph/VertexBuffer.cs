//
// This file is part of IMAGEFrac (R) and related technologies.
//
// Copyright (c) 2017-2020 Reveal Energy Services.  All Rights Reserved.
//
// LEGAL NOTICE:
// IMAGEFrac contains trade secrets and otherwise confidential information
// owned by Reveal Energy Services. Access to and use of this information is 
// strictly limited and controlled by the Company. This file may not be copied,
// distributed, or otherwise disclosed outside of the Company's facilities 
// except under appropriate precautions to maintain the confidentiality hereof, 
// and may not be used in any way not expressly authorized by the Company.
//

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
    }
    
    public interface IVertexBuffer<T> : IVertexBuffer where T : struct
    {
        T[] VertexData { get; set; }
    }
    
    public class VertexBuffer<T> : IVertexBuffer<T> where T : struct
    {
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
            if (_vertexBufferCache.ContainsKey(device)) return;
            
            var vtxBufferDesc =
                new BufferDescription((uint) (VertexData.Length * SizeOfVertexData), BufferUsage.VertexBuffer);
            var vbo = factory.CreateBuffer(vtxBufferDesc);
            device.UpdateBuffer(vbo, 0, VertexData);
            
            _vertexBufferCache.Add(device, vbo);
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
    }
}