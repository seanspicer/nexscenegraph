﻿//
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Veldrid;
using Veldrid.OpenGLBinding;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.Sdl2;
using Veldrid.Utilities;
using Veldrid.StartupUtilities;
using static Veldrid.Sdl2.Sdl2Native;

namespace Veldrid.SceneGraph.Viewer
{
    public class SimpleViewer : IViewer
    {
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //
        // PUBLIC Properties
        //
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        #region PUBLIC_PROPERTIES

        public uint Width => (uint) _window.Width;
        public uint Height => (uint) _window.Height;

        public Group SceneData
        {
            get => _view?.SceneData;
            set => _view.SceneData = value;
        }
        
        public View View
        {
            get => _view;
        }

        public ResourceFactory ResourceFactory => _factory;
        public GraphicsDevice GraphicsDevice => _graphicsDevice;
        public GraphicsBackend Backend => GraphicsDevice.ResourceFactory.BackendType;
        public Platform PlatformType { get; }
        public event Action<float> Rendering;
        public event Action<GraphicsDevice, ResourceFactory, Swapchain> GraphicsDeviceCreated;
        public event Action GraphicsDeviceDestroyed;
        public event Action Resized;
        public event Action<KeyEvent> KeyPressed;

        #endregion

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //
        // PRIVATE Properties
        //
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        #region PRIVATE_PROPERTIES

        private GraphicsDevice _graphicsDevice;
        private DisposeCollectorResourceFactory _factory;
        private readonly Sdl2Window _window;
        private bool _windowResized = true;
        private bool _firstFrame = true;
        private Stopwatch _stopwatch = null;
        private double _previousElapsed = 0;
        private GraphicsBackend _preferredBackend = GraphicsBackend.Vulkan;
        private View _view;

        private event Action<GraphicsDevice, ResourceFactory> GraphicsDeviceOperations;

        private event Action<InputStateSnapshot> InputSnapshotEvent;
        
        #endregion


        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //
        // PUBLIC Methods
        //
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        #region PUBLIC_METHODS

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title"></param>
        //
        // TODO: remove unsafe once Veldrid.SDL2 implements resize fix.
        //
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
            DisplaySettings.Instance.ScreenWidth = wci.WindowWidth;
            DisplaySettings.Instance.ScreenHeight = wci.WindowHeight;
            DisplaySettings.Instance.ScreenDistance = 1000.0f;

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
                Frame();
            };


            _window.KeyDown += OnKeyDown;
            _view = new View();
            GraphicsDeviceOperations += _view.Camera.Renderer.HandleOperation;
            InputSnapshotEvent += _view.OnInputEvent;
        }

        /// <summary>
        /// Run the viewer
        /// </summary>
        /// <param name="preferredBackend"></param>
        //
        // TODO: This runs continuously, probably shoudl have a mode that runs one-frame-at-a-time.
        // 
        public void Run(GraphicsBackend preferredBackend = GraphicsBackend.Vulkan)
        {
            _preferredBackend = preferredBackend;

            while (_window.Exists)
            {
                var inputSnapshot = _window.PumpEvents();
                
                // TODO: Can remove InputTracker?
                InputTracker.UpdateFrameInput(inputSnapshot);
                
                InputSnapshotEvent?.Invoke(new InputStateSnapshot(inputSnapshot, _window.Width, _window.Height));
                
                Frame();
            }

            DisposeResources();
        }

        #endregion

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //
        // PROTECTED Methods
        //
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        #region PROTECTED_METHODS

        //
        // Dispose Properly
        // 
        protected void DisposeResources()
        {
            _graphicsDevice.WaitForIdle();
            _factory.DisposeCollector.DisposeAll();
            _graphicsDevice.Dispose();
            _graphicsDevice = null;
            GraphicsDeviceDestroyed?.Invoke();
        }

        // 
        // Invoke Keyboard events.
        //
        protected void OnKeyDown(KeyEvent keyEvent)
        {
            KeyPressed?.Invoke(keyEvent);
        }

        #endregion


        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //
        // PRIVATE Methods
        //
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        #region PRIVATE_METHODS

        //
        // TODO: Get Eric to Review
        // This is a "trick" to get continuous resize behavior
        // On Windows.  This should probably be integrated into
        // Veldrid.SDL2
        //
        private unsafe int ResizingEventWatcher(void* data, SDL_Event* @event)
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


        // 
        // Initialize the viewer
        //
        private void ViewerInit()
        {
            var options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: true,
                resourceBindingModel: ResourceBindingModel.Improved);
#if DEBUG
            options.Debug = true;
#endif
            _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, options, _preferredBackend);
            _factory = new DisposeCollectorResourceFactory(_graphicsDevice.ResourceFactory);
            GraphicsDeviceCreated?.Invoke(_graphicsDevice, _factory, _graphicsDevice.MainSwapchain);
            _stopwatch = Stopwatch.StartNew();
            _previousElapsed = _stopwatch.Elapsed.TotalSeconds;
        }

        // 
        // Draw a frame
        // 
        private void Frame()
        {
            if (_firstFrame)
            {
                ViewerInit();
                _firstFrame = false;
            }

            if (!_window.Exists) return;

            var newElapsed = _stopwatch.Elapsed.TotalSeconds;
            var deltaSeconds = (float) (newElapsed - _previousElapsed);

            _previousElapsed = newElapsed;

            if (null == _graphicsDevice) return;

            RenderingTraversals();

            Rendering?.Invoke(deltaSeconds);
        }

        //
        // Run the traversals.
        //
        private void RenderingTraversals()
        {
            if (_windowResized)
            {
                _windowResized = false;
                _graphicsDevice.ResizeMainWindow((uint) _window.Width, (uint) _window.Height);
                DisplaySettings.Instance.ScreenWidth = _window.Width;
                DisplaySettings.Instance.ScreenHeight = _window.Height;
                Resized?.Invoke();
            }

            // TODO: Implement Update Traversal
            // TODO: Implement Update Uniforms Traversal
            // TODO: Implement Cull Traversal

            // Get the view, the associated camera, its renderer, and draw.
            GraphicsDeviceOperations?.Invoke(_graphicsDevice, _factory);
        }

        #endregion
    }
}