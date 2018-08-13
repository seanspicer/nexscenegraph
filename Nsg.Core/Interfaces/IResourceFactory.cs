namespace Nsg.Core.Interfaces
{
    public interface IResourceFactory
    {
        IDeviceBuffer CreateBuffer(BufferDescription description);
    }
}