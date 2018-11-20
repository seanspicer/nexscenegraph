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
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Veldrid;
using Veldrid.OpenGLBinding;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Logging;
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

        public IGroup SceneData
        {
            get => _view?.SceneData;
            set => _view.SceneData = value;
        }
        
        public IView View
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

        #endregion

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //
        // PRIVATE Properties
        //
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        #region PRIVATE_PROPERTIES

        private string _windowTitle = string.Empty;
        
        private GraphicsDevice _graphicsDevice;
        private DisposeCollectorResourceFactory _factory;
        private readonly Sdl2Window _window;
        private bool _windowResized = true;
        private bool _firstFrame = true;
        private Stopwatch _stopwatch = null;
        private double _previousElapsed = 0;
        private GraphicsBackend _preferredBackend = DisplaySettings.Instance.GraphicsBackend;
        private IView _view;

        private event Action<GraphicsDevice, ResourceFactory> GraphicsDeviceOperations;

        private event Action<IInputStateSnapshot> InputSnapshotEvent;

        private const uint NFramesInBuffer = 30;
        private ulong _frameCounter = 0;
        private ulong _globalFrameCounter = 0;
        private double _frameTimeAccumulator = 0.0;
        private double _fpsDrawTimeAccumulator = 0.0;
        private readonly double[] _frameTimeBuff = new double[NFramesInBuffer];

        private SDL_EventFilter ResizeEventFilter = null;

        private IVeldridSceneGraphLogger _logger;
        
        #endregion


        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //
        // PUBLIC Methods
        //
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        #region PUBLIC_METHODS

        public static IViewer Create(string title)
        {
            return new SimpleViewer(title);
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title"></param>
        //
        // TODO: remove unsafe once Veldrid.SDL2 implements resize fix.
        //
        protected unsafe SimpleViewer(string title)
        {
            _logger = LoggingService.Instance.GetLogger();
            
            _windowTitle = title;
            
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
            // This is a "trick" to get continuous resize behavior
            // On Windows.  This should probably be integrated into
            // Veldrid.SDL2
            //
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ResizeEventFilter = ResizingEventWatcher;
                SDL_AddEventWatch(ResizeEventFilter, null);
            }

            _window.Resized += () =>
            {
                _windowResized = true;
                Frame();
            };


            _window.KeyDown += OnKeyDown;
            _view = Viewer.View.Create();
            GraphicsDeviceOperations += _view.Camera.Renderer.HandleOperation;
            InputSnapshotEvent += _view.OnInputEvent;
            
        }

        public void ViewAll()
        {
            _view.CameraManipulator?.ViewAll();
        }

        public void SetSceneData(IGroup root)
        {
            _view.SceneData = root;
        }

        public void SetCameraManipulator(ICameraManipulator cameraManipulator)
        {
            _view.CameraManipulator = cameraManipulator;
        }

        public void AddInputEventHandler(IInputEventHandler handler)
        {
            _view.AddInputEventHandler(handler);
        }
        
        public void Run()
        {
            Run(null);
        }

        /// <summary>
        /// Run the viewer
        /// </summary>
        /// <param name="preferredBackend"></param>
        //
        // TODO: This runs continuously, probably shoudl have a mode that runs one-frame-at-a-time.
        // 
        public void Run(GraphicsBackend? preferredBackend = null)
        {
            if (preferredBackend.HasValue)
            {
                _preferredBackend = preferredBackend.Value;
            }
            

            while (_window.Exists)
            {
                var inputSnapshot = _window.PumpEvents();
                
                // TODO: Can remove InputTracker?
                //InputTracker.UpdateFrameInput(inputSnapshot);
                
                InputSnapshotEvent?.Invoke(InputStateSnapshot.Create(inputSnapshot, _window.Width, _window.Height));
                
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
            //KeyPressed?.Invoke(keyEvent);
        }

        #endregion


        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //
        // PRIVATE Methods
        //
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        #region PRIVATE_METHODS

        //
        // This is a "trick" to get continuous resize behavior
        // On Windows.  This should probably be integrated into
        // Veldrid.SDL2
        //
        private unsafe int ResizingEventWatcher(void* data, SDL_Event* @event)
        {
            if (@event->type != SDL_EventType.WindowEvent) return 0;
            
            var windowEvent = Unsafe.Read<SDL_WindowEvent>(@event);
            if (windowEvent.@event == SDL_WindowEventID.Resized)
            {
                var inputSnapshot = _window.PumpEvents();
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
            _logger.Info(() => $"Creating Graphics Device with {_preferredBackend} Backend");
            
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
            
            //
            // Rudimentary FPS Calc
            // 
            {

                _frameTimeAccumulator -= _frameTimeBuff[_frameCounter];
                _frameTimeBuff[_frameCounter] = deltaSeconds;
                _frameTimeAccumulator += deltaSeconds;

                _fpsDrawTimeAccumulator += deltaSeconds;
                if (_fpsDrawTimeAccumulator > 0.03333)
                {
                    var avgFps = (NFramesInBuffer/_frameTimeAccumulator);
                
                    _window.Title = _windowTitle + ": FPS: " + avgFps.ToString("#.0");
                    _fpsDrawTimeAccumulator = 0.0;
                }
                
                // RingBuffer
                if (_frameCounter == NFramesInBuffer - 1)
                {
                    _frameCounter = 0;
                    
                }
                else
                {
                    _frameCounter++;
                }
            }

            _globalFrameCounter++;
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