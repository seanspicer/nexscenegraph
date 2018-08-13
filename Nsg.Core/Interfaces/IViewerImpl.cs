namespace Nsg.Core.Interfaces
{
    public interface IViewerImpl
    {
        IGraphicsDevice GraphicsDevice { get; }
        IResourceFactory ResourceFactory { get; }
        
        void CreateWindow();
        void Show();
    }
}