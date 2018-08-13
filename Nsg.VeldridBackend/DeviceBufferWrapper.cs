using Nsg.Core.Interfaces;

namespace Nsg.VeldridBackend
{
    internal class DeviceBufferWrapper : IDeviceBuffer
    {
        internal Veldrid.DeviceBuffer DeviceBuffer { get; }

        public string Name
        {
            get => DeviceBuffer.Name;
            set => DeviceBuffer.Name = Name;
        }

        public uint SizeInBytes => DeviceBuffer.SizeInBytes;

        internal DeviceBufferWrapper(Veldrid.DeviceBuffer deviceBuffer)
        {
            DeviceBuffer = deviceBuffer;
        }
        
        public void Dispose()
        {
            DeviceBuffer.Dispose();
        }
    }
}