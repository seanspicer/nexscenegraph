namespace Veldrid.SceneGraph
{
    public interface IBindable : IObject
    {
        BufferDescription BufferDescription { get; }
        
        ResourceLayoutElementDescription ResourceLayoutElementDescription { get; }
        
        DeviceBufferRange DeviceBufferRange { get; }
        
        DeviceBuffer ConfigureDeviceBuffers(GraphicsDevice device, ResourceFactory factory);
    }
}