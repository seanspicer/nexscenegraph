using Nsg.Core.Interfaces;
using Nsg.Core.Viewer;

namespace Nsg.VeldridBackend
{
    public class ViewerFactory : IViewerFactory
    {
        public IViewerImpl GetViewer(SimpleViewer viewer)
        {
            return new ViewerImpl(viewer);
        }
    }
}