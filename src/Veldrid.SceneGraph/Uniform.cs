using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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

    public class Uniform<T> : Object, IUniform<T> where T : unmanaged
    {
        private uint _hostBufStride;
        private uint _modifiedCount;
        private DeviceBuffer _uniformBuffer;
        private T[] _uniformData;

        private readonly Dictionary<GraphicsDevice, DeviceBuffer> DeviceBufferCache =
            new Dictionary<GraphicsDevice, DeviceBuffer>();

        private readonly Dictionary<GraphicsDevice, DeviceBufferRange> DeviceBufferRangeCache =
            new Dictionary<GraphicsDevice, DeviceBufferRange>();

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

        private uint SizeOfUniformDataElement => (uint) Marshal.SizeOf(default(T));

        private BufferUsage BufferUsage { get; }
        private ShaderStages ShaderStages { get; }

        public DeviceBufferRange DeviceBufferRange { get; private set; }

        public T[] UniformData
        {
            get => _uniformData;
            set
            {
                _uniformData = value;
                Dirty();
            }
        }

        public string Name => ResourceLayoutElementDescription.Name;

        public BufferDescription BufferDescription { get; private set; }

        public ResourceLayoutElementDescription ResourceLayoutElementDescription { get; }

        public void Dirty()
        {
            _modifiedCount++;
        }

        public DeviceBufferRange GetDeviceBufferRange(GraphicsDevice device)
        {
            if (DeviceBufferRangeCache.TryGetValue(device, out var deviceBufferRange)) return deviceBufferRange;

            throw new ArgumentException("Invalid device");
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

            var bufsize = (uint) (uniformObjSizeInBytes * UniformData.Length);
            BufferDescription = new BufferDescription(bufsize, BufferUsage);

            if (false == DeviceBufferCache.TryGetValue(device, out var uniformBuffer))
            {
                _uniformBuffer = factory.CreateBuffer(BufferDescription);
                DeviceBufferCache.Add(device, _uniformBuffer);
            }

            UpdateDeviceBuffers(device, factory);

            if (false == DeviceBufferRangeCache.TryGetValue(device, out var deviceBufferRange))
            {
                deviceBufferRange = new DeviceBufferRange(_uniformBuffer, 0, uniformObjSizeInBytes);
                DeviceBufferRangeCache.Add(device, deviceBufferRange);
            }
        }

        public virtual void UpdateDeviceBuffers(GraphicsDevice device, ResourceFactory factory)
        {
            if (_modifiedCount == 0 || null == _uniformBuffer) return;

            var uniformBufferStaging = new T[UniformData.Length * _hostBufStride];
            for (var i = 0; i < UniformData.Length; ++i) uniformBufferStaging[i * _hostBufStride] = UniformData[i];

            foreach (var uniformBuffer in DeviceBufferCache.Values)
                device.UpdateBuffer(uniformBuffer, 0, uniformBufferStaging);

            _modifiedCount = 0;
        }

        public static IUniform<T> Create(string name,
            BufferUsage bufferUsage,
            ShaderStages shaderStages,
            ResourceLayoutElementOptions options = ResourceLayoutElementOptions.None)
        {
            return new Uniform<T>(name, bufferUsage, shaderStages, options);
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