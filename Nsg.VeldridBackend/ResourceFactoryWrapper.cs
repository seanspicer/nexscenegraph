using Nsg.Core;
using Nsg.Core.Interfaces;

namespace Nsg.VeldridBackend
{
    public class ResourceFactoryWrapper : IResourceFactory
    {
        private readonly Veldrid.ResourceFactory _resourceFactory = null;
        
        internal ResourceFactoryWrapper(Veldrid.ResourceFactory resourceFactory)
        {
            _resourceFactory = resourceFactory;
        }

        public IDeviceBuffer CreateBuffer(BufferDescription description)
        {
            var vbd = new Veldrid.BufferDescription(description.SizeInBytes, (Veldrid.BufferUsage)description.Usage);
            return new DeviceBufferWrapper(_resourceFactory.CreateBuffer(vbd));
        }
    }
}