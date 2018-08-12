using Nsg.Core;
using Nsg.Core.Interfaces;

namespace Nsg.Core.Viewer
{
    public class SimpleViewer
    {
        public Node Root { get; set; }
        
        private IViewerImpl Impl { get; set; }

        public SimpleViewer()
        {
            Impl = Config.Instance.Backend.ViewerFactory.GetViewer();
            Impl.CreateWindow();
        }

        public void Show()
        {
            Impl.Show();
        }
    }
}