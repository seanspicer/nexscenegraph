using Nsg.Core.Interfaces;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Nsg.VeldridBackend
{
    public class ViewerImpl : IViewerImpl
    {
        private WindowCreateInfo _windowCI;
        private GraphicsDevice _graphicsDevice;
        
        public ViewerImpl()
        {
            
        }

        public void CreateWindow()
        {
            _windowCI = new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowTitle = "Veldrid Tutorial"
            };
        }

        public void Show()
        {
            Sdl2Window window = VeldridStartup.CreateWindow(ref _windowCI);
            _graphicsDevice = VeldridStartup.CreateGraphicsDevice(window);
            
            while (window.Exists)
            {
                window.PumpEvents();
            }

            DisposeResources();
        }
        
        private void DisposeResources()
        {
            _graphicsDevice.Dispose();
        }
    }
}