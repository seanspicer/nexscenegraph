using Nsg.Core.Interfaces;
using Nsg.Core.Viewer;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Nsg.VeldridBackend
{
    public class ViewerImpl : IViewerImpl
    {
        private WindowCreateInfo _windowCI;
        private GraphicsDevice _graphicsDevice;
        private GraphicsDeviceWrapper _graphicsDeviceWrapper = null;
        private Sdl2Window _window;
        private SimpleViewer _viewer;
        private DrawVisitor _drawVisitor;
        
        public ViewerImpl(SimpleViewer viewer)
        {
            _viewer = viewer;
        }

        public IGraphicsDevice GraphicsDevice => _graphicsDeviceWrapper;
        public IResourceFactory ResourceFactory => _graphicsDeviceWrapper.ResourceFactory;

        public void CreateWindow()
        {
            _windowCI = new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowTitle = "HelloNsg"
            };
            
            _window = VeldridStartup.CreateWindow(ref _windowCI);
            _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, GraphicsBackend.Vulkan);
            _graphicsDeviceWrapper = new GraphicsDeviceWrapper(_graphicsDevice);
        }

        public void Show()
        {
            _drawVisitor = new DrawVisitor(_graphicsDevice);
            while (_window.Exists)
            {
                _window.PumpEvents();
                
                if (_window.Exists)
                {
                    _drawVisitor.BeginDraw();
                    _viewer.Draw(_drawVisitor);
                    _drawVisitor.EndDraw();
                }
            }

            DisposeResources();
        }
        
        private void DisposeResources()
        {
            _graphicsDevice.Dispose();
            _drawVisitor.DisposeResources();
        }
    }
}