using Nsg.Core;
using Nsg.Core.Interfaces;

namespace Nsg.Core.Viewer
{
    public class SimpleViewer
    {
        public Node Root { get; set; }

        public IResourceFactory ResourceFactory => Impl.ResourceFactory;

        public IGraphicsDevice GraphicsDevice => Impl.GraphicsDevice;
        
        private IViewerImpl Impl { get; set; }

        public SimpleViewer()
        {
            Impl = Config.Instance.Backend.ViewerFactory.GetViewer(this);
            Impl.CreateWindow();
        }
        
        public void Show()
        {
            Impl.Show();
        }

        public void Draw(IDrawVisitor drawVisitor)
        {
            Root?.Accept(drawVisitor);
        }
    }
}