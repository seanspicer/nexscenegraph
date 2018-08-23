using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Veldrid.SceneGraph.Viewer
{
    public class SimpleViewer
    {
        private WindowCreateInfo _windowCI;
        private GraphicsDevice _graphicsDevice;
        private Sdl2Window _window;
        private DrawVisitor _drawVisitor;
        
        public Node Root { get; set; }

        public ResourceFactory ResourceFactory => ResourceFactory;

        public GraphicsDevice GraphicsDevice => 
            GraphicsDevice;
        
        public SimpleViewer()
        {
            CreateWindow();
        }
        
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
                    Draw(_drawVisitor);
                    _drawVisitor.EndDraw();
                }
            }

            DisposeResources();
        }

        internal void Draw(DrawVisitor drawVisitor)
        {
            Root?.Accept(drawVisitor);
        }
        
        private void DisposeResources()
        {
            _drawVisitor.DisposeResources();
            _graphicsDevice.Dispose();
        }
    }
}