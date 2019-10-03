using System;
using System.Linq;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace Veldrid.SceneGraph
{
    public class Uniform<T> : IUniform<T> where T : struct
    {
        public T[] UniformData { get; set; }
        
        private uint SizeOfUniformDataElement => (uint) Marshal.SizeOf(default(T));

        public string Name => ResourceLayoutElementDescription.Name;
        
        public BufferDescription BufferDescription { get; private set; }
        
        private BufferUsage BufferUsage { get; set; }
        private ShaderStages ShaderStages { get; set; }
        
        public ResourceLayoutElementDescription ResourceLayoutElementDescription { get; private set; }
        
        public DeviceBufferRange DeviceBufferRange { get; private set; }

        public static IUniform<T> Create(string name, 
            BufferUsage bufferUsage, 
            ShaderStages shaderStages,
            ResourceLayoutElementOptions options = ResourceLayoutElementOptions.None)
        {
            return new Uniform<T>(name, bufferUsage, shaderStages, options);
        }

        internal Uniform(string name, 
            BufferUsage bufferUsage, 
            ShaderStages shaderStages,
            ResourceLayoutElementOptions options)
        {
            BufferUsage = bufferUsage;
            ShaderStages = shaderStages;
            
            ResourceLayoutElementDescription = new ResourceLayoutElementDescription(
                name, 
                ResourceKind.UniformBuffer, 
                ShaderStages, 
                options);

        }
        
        public DeviceBuffer ConfigureDeviceBuffers(GraphicsDevice device, ResourceFactory factory)
        {
            var alignment = device.UniformBufferMinOffsetAlignment;
            
            var uniformObjSizeInBytes = SizeOfUniformDataElement;
            var hostBuffStride = 1u;
            if (alignment > SizeOfUniformDataElement)
            {
                hostBuffStride = alignment / SizeOfUniformDataElement;
                uniformObjSizeInBytes = alignment;
            }

            var bufsize = (uint)(uniformObjSizeInBytes * UniformData.Length);
            BufferDescription = new BufferDescription(bufsize, BufferUsage);

            var uniformBuffer = factory.CreateBuffer(BufferDescription);

            var uniformBufferStaging = new T[UniformData.Length * hostBuffStride];
            for (var i = 0; i < UniformData.Length; ++i)
            {
                uniformBufferStaging[i * hostBuffStride] = UniformData[i];
            }
            
            device.UpdateBuffer(uniformBuffer, 0, uniformBufferStaging);
            
            DeviceBufferRange = new DeviceBufferRange(uniformBuffer, 0, uniformObjSizeInBytes);
            
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