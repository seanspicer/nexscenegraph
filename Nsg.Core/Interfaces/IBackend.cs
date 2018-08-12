namespace Nsg.Core.Interfaces
{
    public interface IBackend
    {
        IViewerFactory ViewerFactory { get; }
    }
}