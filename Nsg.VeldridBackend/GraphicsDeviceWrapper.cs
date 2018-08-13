using Nsg.Core.Interfaces;
using Veldrid;

namespace Nsg.VeldridBackend
{
    internal class GraphicsDeviceWrapper : IGraphicsDevice
    {
        private ResourceFactoryWrapper _resourceFactoryWrapper = null;
        public IResourceFactory ResourceFactory
        {
            get
            {
                return _resourceFactoryWrapper ??
                       (_resourceFactoryWrapper = new ResourceFactoryWrapper(_graphicsDevice.ResourceFactory));
            }
        }

        private readonly Veldrid.GraphicsDevice _graphicsDevice = null;
        
        internal GraphicsDeviceWrapper(Veldrid.GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }
        
        public void UpdateBuffer<T>(IDeviceBuffer buffer, uint bufferOffsetInBytes, T[] source) where T : struct
        {
            var dBufWrap = (DeviceBufferWrapper) buffer;
            _graphicsDevice.UpdateBuffer(dBufWrap.DeviceBuffer, bufferOffsetInBytes, source);
        }
    }
}