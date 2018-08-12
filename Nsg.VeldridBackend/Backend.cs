using System.Composition;
using Nsg.Core.Interfaces;

namespace Nsg.VeldridBackend
{   
    [Export(typeof(IBackend))]
    public class Backend : IBackend
    {
        public IViewerFactory ViewerFactory { get; private set; }

        public Backend()
        {
            ViewerFactory = new ViewerFactory();
        }
    }
}