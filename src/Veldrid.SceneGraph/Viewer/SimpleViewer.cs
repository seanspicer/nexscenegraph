//
// Copyright 2018-2019 Sean Spicer 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
//using Common.Logging;
using Veldrid;
using Veldrid.OpenGLBinding;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Util;
using Veldrid.Sdl2;
using Veldrid.Utilities;
using Veldrid.StartupUtilities;
using static Veldrid.Sdl2.Sdl2Native;

namespace Veldrid.SceneGraph.Viewer
{
    internal class EndFrameEvent : IEndFrameEvent
    {
        public float FrameTime { get; }

        internal EndFrameEvent(float frameTime)
        {
            FrameTime = frameTime;
        }
    }
    
    internal class ResizedEvent : IResizedEvent
    {
        public int Width { get; }
        public int Height { get; }
        
        internal ResizedEvent(int width, int height)
        {
            Width = width;
            Height = height;
        }

    }
    
    public class SimpleViewer : View, IViewer
    {
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //
        // PUBLIC Properties
        //
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        public uint Width => (uint) _window.Width;
        public uint Height => (uint) _window.Height;

        public ResourceFactory ResourceFactory => _factory;
        public GraphicsDevice GraphicsDevice => _graphicsDevice;
        public GraphicsBackend Backend => GraphicsDevice.ResourceFactory.BackendType;
        public Platform PlatformType { get; }

        //public IObservable<IResizedEvent> ResizeEvents => _resizeEvents;
        //public IObservable<IEndFrameEvent> EndFrameEvents => _endFrameEvents;
        //public IObservable<IInputStateSnapshot> ViewerInputEvents => _viewerInputEvents;

        
        
        //public IObservable<
        
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //
        // PRIVATE Properties
        //
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        private string _windowTitle = string.Empty;

        //private ISubject<IResizedEvent> _resizeEvents;
        //private ISubject<IEndFrameEvent> _endFrameEvents;
        private ISubject<IInputStateSnapshot> _viewerInputEvents;
        
        private GraphicsDevice _graphicsDevice;
        private DisposeCollectorResourceFactory _factory;
        private readonly Sdl2Window _window;
        private bool _windowResized = true;
        private bool _firstFrame = true;
        private Stopwatch _stopwatch = null;
        private double _previousElapsed = 0;
        private GraphicsBackend _preferredBackend = DisplaySettings.Instance.GraphicsBackend;

        private SceneContext _sceneContext;

        private event Action<GraphicsDevice, ResourceFactory> GraphicsDeviceOperations;
        private event Action<GraphicsDevice> GraphicsDeviceResize;
        
        
        private const uint NFramesInBuffer = 30;
        private ulong _frameCounter = 0;
        private ulong _globalFrameCounter = 0;
        private double _frameTimeAccumulator = 0.0;
        private double _fpsDrawTimeAccumulator = 0.0;
        private readonly double[] _frameTimeBuff = new double[NFramesInBuffer];

        private SDL_EventFilter ResizeEventFilter = null;

        private IUpdateVisitor _updateVisitor;
        
        //private ILog _logger;
        

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //
        // PUBLIC Methods
        //
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        public static IViewer Create(string title, TextureSampleCount fsaaCount=TextureSampleCount.Count1)
        {
            return new SimpleViewer(title, fsaaCount);
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title"></param>
        //
        // TODO: remove unsafe once Veldrid.SDL2 implements resize fix.
        //
        protected unsafe SimpleViewer(string title, TextureSampleCount fsaaCount)
        {
            //_logger = LogManager.GetLogger<SimpleViewer>();
            
            // Create Subjects
            _viewerInputEvents = new Subject<IInputStateSnapshot>();
            //_endFrameEvents = new Subject<IEndFrameEvent>();
            //_resizeEvents = new Subject<IResizedEvent>();
            
            InputEvents = new Subject<IInputStateSnapshot>();
            
            _updateVisitor = UpdateVisitor.Create();
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
            DisplaySettings.Instance.SetScreenWidth(wci.WindowWidth);
            DisplaySettings.Instance.SetScreenHeight(wci.WindowHeight);
            DisplaySettings.Instance.SetScreenDistance(1000.0f);

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

            _sceneContext = new SceneContext(fsaaCount);
            _window.KeyDown += OnKeyDown;
            GraphicsDeviceOperations += Camera.Renderer.HandleOperation;
            GraphicsDeviceResize += Camera.Renderer.HandleResize;
            InputEvents = _viewerInputEvents;
            SceneContext = _sceneContext;

        }

        public ICamera GetCamera()
        {
            return Camera;
        }
        
        public void ViewAll()
        {
            Home();
        }

        public void Home()
        {
            CameraManipulator?.Home(
                InputStateSnapshot.CreateEmpty(
                    (int)DisplaySettings.Instance.ScreenWidth,
                    (int)DisplaySettings.Instance.ScreenHeight), 
                this);
        }
        
        

        public void SetBackgroundColor(RgbaFloat color)
        {
            // TODO - fix this nasty cast
            Camera.SetClearColor(color);
        }

        public void AddInputEventHandler(IInputEventHandler handler)
        {
            AddInputEventHandler(handler);
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
        // TODO: This runs continuously, probably should have a mode that runs one-frame-at-a-time.
        // 
        public void Run(GraphicsBackend? preferredBackend = null)
        {
            if (preferredBackend.HasValue)
            {
                _preferredBackend = preferredBackend.Value;
            }

            _windowTitle = _windowTitle + " (" + _preferredBackend + ") ";

            while (_window.Exists)
            {
                var inputSnapshot = _window.PumpEvents();
                
                // TODO: Can remove InputTracker?
                //InputTracker.UpdateFrameInput(inputSnapshot);

                var inputStateSnap = InputStateSnapshot.Create(inputSnapshot, _window.Width, _window.Height);
                
                _viewerInputEvents.OnNext(inputStateSnap);
                
                Frame();
            }
            
            //_viewerInputEvents.OnCompleted();
            //_endFrameEvents.OnCompleted();
            //_resizeEvents.OnCompleted();

            DisposeResources();
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //
        // PROTECTED Methods
        //
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        //
        // Dispose Properly
        // 
        protected void DisposeResources()
        {
            _graphicsDevice.WaitForIdle();
            _factory.DisposeCollector.DisposeAll();
            _graphicsDevice.Dispose();
            _graphicsDevice = null;
        }

        // 
        // Invoke Keyboard events.
        //
        protected void OnKeyDown(KeyEvent keyEvent)
        {
            //KeyPressed?.Invoke(keyEvent);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //
        // PRIVATE Methods
        //
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

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
                _window.PumpEvents();
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
                swapchainDepthFormat: PixelFormat.R32_Float,
                syncToVerticalBlank: true,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true,
                swapchainSrgbFormat: false);
            
#if DEBUG
            options.Debug = true;
#endif
            //_logger.Info(m => m($"Creating Graphics Device with {_preferredBackend} Backend"));
            
            _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, options, _preferredBackend);

            bool isDirect3DSupported = GraphicsDevice.IsBackendSupported(GraphicsBackend.Direct3D11);
            _factory = new DisposeCollectorResourceFactory(_graphicsDevice.ResourceFactory);
            _stopwatch = Stopwatch.StartNew();
            _previousElapsed = _stopwatch.Elapsed.TotalSeconds;
            
            _sceneContext.SetOutputFramebufffer(_graphicsDevice.SwapchainFramebuffer);
            _sceneContext.CreateDeviceObjects(_graphicsDevice, _factory);
            
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

            UpdateTraversal();
            
            RenderingTraversals();

            //_endFrameEvents.OnNext(new EndFrameEvent(deltaSeconds));
        }

        private void UpdateTraversal()
        {
            //_updateVisitor.Reset();
            //_updateVisitor.SetFrameStamp(GetFrameStamp());
            //_updateVisitor.SetTraversalNumber(GetFrameStamp().GetFrameNumber());
            
            SceneData?.Accept(_updateVisitor);

            if (null != CameraManipulator)
            {
                CameraManipulator.UpdateCamera(Camera);
            }
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
                DisplaySettings.Instance.SetScreenWidth(_window.Width);
                DisplaySettings.Instance.SetScreenHeight(_window.Height);
                
                Camera.Resize(_window.Width, _window.Height);
                
                //_resizeEvents.OnNext(new ResizedEvent(_window.Width, _window.Height));

                _sceneContext.SetOutputFramebufffer(_graphicsDevice.SwapchainFramebuffer);
                
                _sceneContext.RecreateWindowSizedResources(
                    _graphicsDevice, 
                    _factory);
                
                GraphicsDeviceResize?.Invoke(_graphicsDevice);
                
                Framebuffer = _graphicsDevice.SwapchainFramebuffer;
            }
            
            GraphicsDeviceOperations?.Invoke(_graphicsDevice, _factory);
        }

    }
}