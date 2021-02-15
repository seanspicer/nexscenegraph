namespace Veldrid.SceneGraph
{
    public interface IBindable : IObject
    {
        BufferDescription BufferDescription { get; }
        
        ResourceLayoutElementDescription ResourceLayoutElementDescription { get; }

        DeviceBufferRange GetDeviceBufferRange(GraphicsDevice device);
        
        void ConfigureDeviceBuffers(GraphicsDevice device, ResourceFactory factory);
    }
}