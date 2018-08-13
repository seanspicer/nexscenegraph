
using Nsg.Core.Interfaces;
using Nsg.Core.Viewer;

namespace Nsg.Core.Interfaces
{
    public interface IViewerFactory
    {
        IViewerImpl GetViewer(SimpleViewer viewer);
    }
}