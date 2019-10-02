using System;
using System.Linq;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace Veldrid.SceneGraph
{
    public class Uniform<T> : IUniform<T> where T : struct
    {
        public T[] UniformData { get; set; }
        
        private int SizeOfUniformDataElement => Marshal.SizeOf(default(T));

        public string Name => ResourceLayoutElementDescription.Name;
        
        public BufferDescription BufferDescription { get; private set; }
        
        private BufferUsage BufferUsage { get; set; }
        private ShaderStages ShaderStages { get; set; }
        
        public ResourceLayoutElementDescription ResourceLayoutElementDescription { get; private set; }
        
        public DeviceBufferRange DeviceBufferRange { get; private set; }

        public static IUniform<T> Create(string name, 
            BufferUsage bufferUsage, 
            ShaderStages shaderStages)
        {
            return new Uniform<T>(name, bufferUsage, shaderStages);
        }

        internal Uniform(string name, 
            BufferUsage bufferUsage, 
            ShaderStages shaderStages)
        {
            BufferUsage = bufferUsage;
            ShaderStages = shaderStages;
            
            ResourceLayoutElementDescription = new ResourceLayoutElementDescription(
                name, 
                ResourceKind.UniformBuffer, 
                ShaderStages);

        }


        public DeviceBuffer ConfigureDeviceBuffers(GraphicsDevice device, ResourceFactory factory)
        {
            var bufsize = (uint)(SizeOfUniformDataElement * UniformData.Length);
            BufferDescription = new BufferDescription(bufsize, BufferUsage);

            var uniformBuffer = factory.CreateBuffer(BufferDescription);
            
            device.UpdateBuffer(uniformBuffer, 0, UniformData.ToArray());
            
            DeviceBufferRange = new DeviceBufferRange(uniformBuffer, 0, bufsize);
            
            return uniformBuffer;
        }

        private Tuple<uint, uint> CalculateMultiplierAndStride(GraphicsDevice graphicsDevice)
        {
            var nElements = UniformData.Length;
            var alignment = graphicsDevice.UniformBufferMinOffsetAlignment;
            var sizeOfElement = SizeOfUniformDataElement;
            
            var stride = 1u;
            var multiplier = 64u;
            if (alignment > 64u)
            {
                multiplier = alignment;
                stride = alignment / 64u;
            }
            
            return new Tuple<uint, uint>(multiplier, stride);
        }
    }
}