//
// Copyright (c) 2018 Sean Spicer
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

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