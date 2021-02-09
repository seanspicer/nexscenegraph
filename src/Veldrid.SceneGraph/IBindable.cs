namespace Veldrid.SceneGraph
{
    public interface IBindable : IObject
    {
        BufferDescription BufferDescription { get; }
        
        ResourceLayoutElementDescription ResourceLayoutElementDescription { get; }
        
        DeviceBufferRange DeviceBufferRange { get; }
        
        void ConfigureDeviceBuffers(GraphicsDevice device, ResourceFactory factory);
    }
}