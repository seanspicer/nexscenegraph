using Nsg.Core.Interfaces;

namespace Nsg.VeldridBackend
{
    public class ViewerFactory : IViewerFactory
    {
        public IViewerImpl GetViewer()
        {
            return new ViewerImpl();
        }
    }
}