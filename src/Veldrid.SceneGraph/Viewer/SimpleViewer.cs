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

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Veldrid;
using Veldrid.OpenGLBinding;
using Veldrid.Sdl2;
using Veldrid.Utilities;
using Veldrid.StartupUtilities;
using static Veldrid.Sdl2.Sdl2Native;

namespace Veldrid.SceneGraph.Viewer
{
    public class SimpleViewer : IViewer
    {
        private GraphicsDevice _graphicsDevice;
        private DisposeCollectorResourceFactory _factory;
        private readonly Sdl2Window _window;
        private DrawVisitor _drawVisitor;
        private bool _windowResized = true;

        public uint Width => (uint)_window.Width;
        public uint Height => (uint)_window.Height;
        
        public Node Root { get; set; }

        public ResourceFactory ResourceFactory => ResourceFactory;

        public GraphicsDevice GraphicsDevice => 
            GraphicsDevice;
        
        public Platform PlatformType { get; }
        public event Action<float> Rendering;
        public event Action<GraphicsDevice, ResourceFactory, Swapchain> GraphicsDeviceCreated;
        public event Action GraphicsDeviceDestroyed;
        public event Action Resized;
        public event Action<KeyEvent> KeyPressed;
        
        public unsafe SimpleViewer(string title)
        {
            var wci = new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowTitle = title
            };
            
            _window = VeldridStartup.CreateWindow(ref wci);

            //
            // TODO: Get Eric to Review
            // This is a "trick" to get continuous resize behavior
            // On Windows.  This should probably be integrated into
            // Veldrid.SDL2
            //
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SDL_AddEventWatch(ResizingEventWatcher, null);
            }

            _window.Resized += () =>
            {
                _windowResized = true;
                if (null != _drawVisitor && null != _graphicsDevice)
                {
                    RenderFrame();
                }
            };


            _window.KeyDown += OnKeyDown;
        }

        //
        // TODO: Get Eric to Review
        // This is a "trick" to get continuous resize behavior
        // On Windows.  This should probably be integrated into
        // Veldrid.SDL2
        //
        private unsafe int ResizingEventWatcher(void *data, SDL_Event *@event) 
        {
            if (@event->type == SDL_EventType.WindowEvent)
            {
                var windowEvent = Unsafe.Read<SDL_WindowEvent>(@event);
                if (windowEvent.@event == SDL_WindowEventID.Resized)
                {
                    var inputSnapshot = _window.PumpEvents();
                    InputTracker.UpdateFrameInput(inputSnapshot);
                }
            }

            return 0;
        }
        
        public void Run(GraphicsBackend preferredBackend = GraphicsBackend.Vulkan)
        {
            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: true,
                resourceBindingModel: ResourceBindingModel.Improved);
#if DEBUG
            options.Debug = true;
#endif
            _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, options, preferredBackend);
            _factory = new DisposeCollectorResourceFactory(_graphicsDevice.ResourceFactory);
            GraphicsDeviceCreated?.Invoke(_graphicsDevice, _factory, _graphicsDevice.MainSwapchain);
            
            _drawVisitor = new DrawVisitor(_graphicsDevice);
            
            var sw = Stopwatch.StartNew();
            var previousElapsed = sw.Elapsed.TotalSeconds;

            while (_window.Exists)
            {
                var newElapsed = sw.Elapsed.TotalSeconds;
                var deltaSeconds = (float)(newElapsed - previousElapsed);
                
                InputSnapshot inputSnapshot = _window.PumpEvents();
                InputTracker.UpdateFrameInput(inputSnapshot);
                
                if (_window.Exists)
                {
                    previousElapsed = newElapsed;

                    RenderFrame();
                    
                    Rendering?.Invoke(deltaSeconds);
                }
            }

            DisposeResources();
        }

        private void RenderFrame()
        {
            if (_windowResized)
            {
                _windowResized = false;
                _graphicsDevice.ResizeMainWindow((uint)_window.Width, (uint)_window.Height);
                Resized?.Invoke();
            }
                    
            _drawVisitor.BeginDraw();
            Draw(_drawVisitor);
            _drawVisitor.EndDraw();
        }

        internal void Draw(DrawVisitor drawVisitor)
        {
            Root?.Accept(drawVisitor);
        }
        
        protected void DisposeResources()
        {
            _graphicsDevice.WaitForIdle();
            _factory.DisposeCollector.DisposeAll();
            _drawVisitor.DisposeResources();
            _graphicsDevice.Dispose();
            GraphicsDeviceDestroyed?.Invoke();
        }
        
        protected void OnKeyDown(KeyEvent keyEvent)
        {
            KeyPressed?.Invoke(keyEvent);
        }



    }
}