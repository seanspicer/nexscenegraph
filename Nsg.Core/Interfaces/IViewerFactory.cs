
using Nsg.Core.Interfaces;

namespace Nsg.Core.Interfaces
{
    public interface IViewerFactory
    {
        IViewerImpl GetViewer();
    }
}