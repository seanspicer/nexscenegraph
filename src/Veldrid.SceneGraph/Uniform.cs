using System;
using System.Linq;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace Veldrid.SceneGraph
{
    public interface IUniform : IBindable
    {
        string Name { get; }
        void Dirty();
        void UpdateDeviceBuffers(GraphicsDevice device, ResourceFactory factory);
    }
    
    public interface IUniform<T> : IUniform where T : struct
    {
        T[] UniformData { get; set; }
        
    }
    
    internal class Uniform<T> : Object, IUniform<T> where T : struct
    {
        private T[] _uniformData;

        public T[] UniformData
        {
            get => _uniformData;
            set
            {
                _uniformData = value;
                Dirty();
            }
        }
        
        private uint SizeOfUniformDataElement => (uint) Marshal.SizeOf(default(T));

        public string Name => ResourceLayoutElementDescription.Name;
        
        public BufferDescription BufferDescription { get; private set; }
        
        private BufferUsage BufferUsage { get; set; }
        private ShaderStages ShaderStages { get; set; }
        
        public ResourceLayoutElementDescription ResourceLayoutElementDescription { get; private set; }
        
        public DeviceBufferRange DeviceBufferRange { get; private set; }

        private uint _hostBufStride = 0;
        private DeviceBuffer _uniformBuffer = null;
        private uint _modifiedCount = 0;
        
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

        public void Dirty()
        {
            _modifiedCount++;
        }
        
        public void ConfigureDeviceBuffers(GraphicsDevice device, ResourceFactory factory)
        {
            var alignment = device.UniformBufferMinOffsetAlignment;
            
            var uniformObjSizeInBytes = SizeOfUniformDataElement;
            _hostBufStride = 1u;
            if (alignment > SizeOfUniformDataElement)
            {
                _hostBufStride = alignment / SizeOfUniformDataElement;
                uniformObjSizeInBytes = alignment;
            }

            var bufsize = (uint)(uniformObjSizeInBytes * UniformData.Length);
            BufferDescription = new BufferDescription(bufsize, BufferUsage);

            _uniformBuffer = factory.CreateBuffer(BufferDescription);

            UpdateDeviceBuffers(device, factory);
            
            DeviceBufferRange = new DeviceBufferRange(_uniformBuffer, 0, uniformObjSizeInBytes);
            
        }

        public virtual void UpdateDeviceBuffers(GraphicsDevice device, ResourceFactory factory)
        {
            if (_modifiedCount == 0) return;
            
            var uniformBufferStaging = new T[UniformData.Length * _hostBufStride];
            for (var i = 0; i < UniformData.Length; ++i)
            {
                uniformBufferStaging[i * _hostBufStride] = UniformData[i];
            }
            
            device.UpdateBuffer(_uniformBuffer, 0, uniformBufferStaging);

            _modifiedCount = 0;
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