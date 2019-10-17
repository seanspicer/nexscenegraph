using System;
using System.Data.Common;
using System.Diagnostics;
using System.Numerics;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Veldrid;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Viewer;
using Veldrid.SceneGraph.Wpf.Controls;
using Veldrid.Utilities;
using InputEventHandler = System.Windows.Input.InputEventHandler;
using PixelFormat = Veldrid.PixelFormat;

//using PixelFormat = System.Windows.Media.PixelFormat;

namespace Veldrid.SceneGraph.Wpf
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
    
    // This extends from the "Win32HwndControl" from the SharpDX example code.
    public class VeldridSceneGraphComponent : HwndWrapper
    {
        private IGroup _sceneData;
        public IGroup SceneData
        {
            get => _sceneData;
            set
            {
                _sceneData = value;
                if (null != _view)
                {
                    ((Veldrid.SceneGraph.Viewer.View) _view).SceneData = _sceneData;
                }
            }
        }

        private ICameraManipulator _cameraManipulator;

        public ICameraManipulator CameraManipulator
        {
            get => _cameraManipulator;
            set
            {
                _cameraManipulator = value;
                if (null != _view)
                {
                    ((Veldrid.SceneGraph.Viewer.View) _view).CameraManipulator = _cameraManipulator;
                }
            }
        }

        private IInputEventHandler _eventHandler;

        public IInputEventHandler EventHandler
        {
            get => _eventHandler;
            set
            {
                _eventHandler = value;
                if (null != _view)
                {
                    _eventHandler.SetView((Veldrid.SceneGraph.Viewer.View)_view);
                    ((Veldrid.SceneGraph.Viewer.View) _view).AddInputEventHandler(_eventHandler);
                }
                
            }
        }

        public IView View
        {
            get => (Veldrid.SceneGraph.Viewer.View)_view;
        }
        
        public IObservable<IResizedEvent> ResizeEvents => _resizeEvents;
        public IObservable<IEndFrameEvent> EndFrameEvents => _endFrameEvents;
        public IObservable<IInputStateSnapshot> ViewerInputEvents => _viewerInputEvents;
        
        //private Swapchain _sc;
        //private CommandList _cl;
        //private GraphicsDevice _gd;

        private string _windowTitle = string.Empty;

        private ISubject<IResizedEvent> _resizeEvents;
        private ISubject<IEndFrameEvent> _endFrameEvents;
        private ISubject<IInputStateSnapshot> _viewerInputEvents;
        
        private GraphicsDevice _graphicsDevice;
        private DisposeCollectorResourceFactory _factory;
        private Stopwatch _stopwatch = null;
        private double _previousElapsed = 0;
        private GraphicsBackend _preferredBackend = DisplaySettings.Instance.GraphicsBackend;
        private Veldrid.SceneGraph.Viewer.IView _view;
        private event Action<GraphicsDevice, ResourceFactory> GraphicsDeviceOperations;
        
        private const uint NFramesInBuffer = 30;
        private ulong _frameCounter = 0;
        private ulong _globalFrameCounter = 0;
        private double _frameTimeAccumulator = 0.0;
        private double _fpsDrawTimeAccumulator = 0.0;
        private readonly double[] _frameTimeBuff = new double[NFramesInBuffer];

        public bool Rendering { get; private set; }

        private WpfInputStateSnapshot _inputState;
        
        protected override sealed void Initialize()
        {
            // Create Subjects
            _viewerInputEvents = new Subject<IInputStateSnapshot>();
            _endFrameEvents = new Subject<IEndFrameEvent>();
            _resizeEvents = new Subject<IResizedEvent>();
            _inputState = new WpfInputStateSnapshot();
            
            var options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: PixelFormat.R32_Float,
                syncToVerticalBlank: true,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true,
                swapchainSrgbFormat: false);

            SwapchainSource source = GetSwapchainSource();
            
            double dpiScale = GetDpiScale();
            uint width = (uint)(ActualWidth < 0 ? 0 : Math.Ceiling(ActualWidth * dpiScale));
            uint height = (uint)(ActualHeight < 0 ? 0 : Math.Ceiling(ActualHeight * dpiScale));

            SwapchainDescription swapchainDesc = new SwapchainDescription(
                source,
                width, height,
                options.SwapchainDepthFormat,
                options.SyncToVerticalBlank,
                options.SwapchainSrgbFormat);
            
            _graphicsDevice = GraphicsDevice.CreateD3D11(options, swapchainDesc);
            
            _factory = new DisposeCollectorResourceFactory(_graphicsDevice.ResourceFactory);
            _stopwatch = Stopwatch.StartNew();
            _previousElapsed = _stopwatch.Elapsed.TotalSeconds;
            
            //_cl = _gd.ResourceFactory.CreateCommandList();
            
            DisplaySettings.Instance.ScreenWidth = width;
            DisplaySettings.Instance.ScreenHeight = height;
            DisplaySettings.Instance.ScreenDistance = 1000.0f;
            
            var view = Veldrid.SceneGraph.Viewer.View.Create(_resizeEvents);
            view.InputEvents = ViewerInputEvents;
            view.SceneData = _sceneData;
            view.CameraManipulator = _cameraManipulator;
            _eventHandler.SetView((Veldrid.SceneGraph.Viewer.View)view);
            view.AddInputEventHandler(_eventHandler);

            GraphicsDeviceOperations += view.Camera.Renderer.HandleOperation;
            
            _view = view;
            
            Rendering = true;
            
            CameraManipulator.ViewAll();

            // Left Button
            HwndLButtonDown += OnMouseLButtonDown;
            HwndLButtonUp += OnMouseLButtonUp;
            
            // Right Button
            HwndRButtonDown += OnMouseRButtonDown;
            HwndRButtonUp += OnMouseRButtonUp;
            
            HwndMouseMove += OnMouseMove;
            HwndMouseWheel += OnMouseWheel;
        }

        private void OnMouseWheel(object sender, HwndMouseEventArgs e)
        {
            _inputState.WheelDelta += e.WheelDelta;
            ProcessEvents();
        }

        private void OnMouseMove(object sender, HwndMouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            _inputState.MousePosition = new Vector2((float)pos.X, (float)pos.Y);
            ProcessEvents();
        }

        private void OnMouseLButtonDown(object sender, HwndMouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            _inputState.MousePosition = new Vector2((float)pos.X, (float)pos.Y);
            
            var mouseEvent = new MouseEvent(MouseButton.Left, true);
            _inputState.MouseDown[(int) MouseButton.Left] = true;
            _inputState.MouseEventList.Add(mouseEvent);
             ProcessEvents();
        }
        
        private void OnMouseLButtonUp(object sender, HwndMouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            _inputState.MousePosition = new Vector2((float)pos.X, (float)pos.Y);
            
            var mouseEvent = new MouseEvent(MouseButton.Left, false);
            _inputState.MouseDown[(int) MouseButton.Left] = false;
            _inputState.MouseEventList.Add(mouseEvent);
            ProcessEvents();
        }
        
        private void OnMouseRButtonDown(object sender, HwndMouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            _inputState.MousePosition = new Vector2((float)pos.X, (float)pos.Y);
            
            var mouseEvent = new MouseEvent(MouseButton.Right, true);
            _inputState.MouseDown[(int) MouseButton.Right] = true;
            _inputState.MouseEventList.Add(mouseEvent);
            ProcessEvents();
        }
        
        private void OnMouseRButtonUp(object sender, HwndMouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            _inputState.MousePosition = new Vector2((float)pos.X, (float)pos.Y);

            var mouseEvent = new MouseEvent(MouseButton.Right, false);
            _inputState.MouseDown[(int) MouseButton.Right] = false;
            _inputState.MouseEventList.Add(mouseEvent);
            ProcessEvents();
        }

        private void ProcessEvents()
        {
            double dpiScale = GetDpiScale();
            int width =  (ActualWidth < 0 ? 0 : (int)Math.Ceiling(ActualWidth * dpiScale));
            int height = (ActualHeight < 0 ? 0 : (int)Math.Ceiling(ActualHeight * dpiScale));

            var inputStateSnap = InputStateSnapshot.Create(_inputState, width, height);
            _viewerInputEvents.OnNext(inputStateSnap);
            _inputState.MouseEventList.Clear();
            _inputState.KeyEventList.Clear();
            _inputState.WheelDelta = 0;
        }

        private ModifierKeys _modifierKeys = ModifierKeys.None;
        
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            var key = e.Key;
            if (key == System.Windows.Input.Key.System)
            {
                key = e.SystemKey;
            }

            switch (key)
            {
                case System.Windows.Input.Key.LeftShift:
                case System.Windows.Input.Key.RightShift:
                    _modifierKeys |= ModifierKeys.Shift;
                    break;
                case System.Windows.Input.Key.LeftCtrl:
                case System.Windows.Input.Key.RightCtrl:
                    _modifierKeys |= ModifierKeys.Control;
                    break;
                case System.Windows.Input.Key.LeftAlt:
                case System.Windows.Input.Key.RightAlt:
                    _modifierKeys |= ModifierKeys.Alt;
                    break;
                default:
                {
                    var keyEvent = new KeyEvent(MapKey(e), e.IsDown, _modifierKeys);
                    _inputState.KeyEventList.Add(keyEvent);
                    ProcessEvents();
                    break;
                }
            }
        }
        
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            var key = e.Key;
            if (key == System.Windows.Input.Key.System)
            {
                key = e.SystemKey;
            }
            
            switch (key)
            {
                case System.Windows.Input.Key.LeftShift:
                case System.Windows.Input.Key.RightShift:
                    _modifierKeys &= ~ModifierKeys.Shift;
                    break;
                case System.Windows.Input.Key.LeftCtrl:
                case System.Windows.Input.Key.RightCtrl:
                    _modifierKeys &= ~ModifierKeys.Control;
                    break;
                case System.Windows.Input.Key.LeftAlt:
                case System.Windows.Input.Key.RightAlt:
                    _modifierKeys &= ~ModifierKeys.Alt;
                    break;
                default:
                {
                    var keyEvent = new KeyEvent(MapKey(e), e.IsDown, _modifierKeys);
                    _inputState.KeyEventList.Add(keyEvent);
                    ProcessEvents();
                    break;
                }
            }
        }

        private Key MapKey(KeyEventArgs e)
        {
            // Convert A - Z
            if ((int) e.Key >= 44 && (int) e.Key <= 69)
            {
                return (Key) ((int) e.Key + 39);
            }
            // Convert 0 - 9
            else if ((int) e.Key >= 34 && (int) e.Key <= 43)
            {
                return (Key) ((int) e.Key + 75);
            }
            // Convert F1 - F12
            else if ((int) e.Key >= 90 && (int) e.Key <= 101)
            {
                return (Key) ((int) e.Key - 80);
            }
            

            return Key.Unknown;
        }
        
        public void ViewAll()
        {
            ((Veldrid.SceneGraph.Viewer.View)_view).CameraManipulator?.ViewAll();
        }
        
        public void SetSceneData(IGroup root)
        {
            ((Veldrid.SceneGraph.Viewer.View)_view).SceneData = root;
        }

        public void SetCameraManipulator(ICameraManipulator cameraManipulator)
        {
            ((Veldrid.SceneGraph.Viewer.View)_view).CameraManipulator = cameraManipulator;
        }

        public void AddInputEventHandler(IInputEventHandler handler)
        {
            ((Veldrid.SceneGraph.Viewer.View)_view).AddInputEventHandler(handler);
        }

        protected override sealed void Uninitialize()
        {
            Rendering = false;
            
            DestroySwapchain();
            
            _viewerInputEvents.OnCompleted();
            _endFrameEvents.OnCompleted();
            _resizeEvents.OnCompleted();

            DisposeResources();
        }
        
        protected void DisposeResources()
        {
            _graphicsDevice.WaitForIdle();
            _factory.DisposeCollector.DisposeAll();
            _graphicsDevice.Dispose();
            _graphicsDevice = null;
        }

        protected sealed override void Resized()
        {
            ResizeSwapchain();
        }

        protected override void Render(IntPtr windowHandle)
        {
            if (!Rendering)
                return;

            Frame();
        }

        private double GetDpiScale()
        {
            PresentationSource source = PresentationSource.FromVisual(this);

            return source.CompositionTarget.TransformToDevice.M11;
        }

        protected virtual SwapchainSource GetSwapchainSource()
        {
            Module mainModule = typeof(VeldridSceneGraphComponent).Module;
            IntPtr hinstance = Marshal.GetHINSTANCE(mainModule);
            return SwapchainSource.CreateWin32(Hwnd, hinstance);
//            SwapchainDescription scDesc = new SwapchainDescription(win32Source, width, height, PixelFormat.R32_Float, true);
//
//            _sc = _gd.ResourceFactory.CreateSwapchain(scDesc);
        }

        protected virtual void DestroySwapchain()
        {
            //_sc.Dispose();
        }

        private void ResizeSwapchain()
        {
            double dpiScale = GetDpiScale();
            uint width = (uint) (ActualWidth < 0 ? 0 : Math.Ceiling(ActualWidth * dpiScale));
            uint height = (uint) (ActualHeight < 0 ? 0 : Math.Ceiling(ActualHeight * dpiScale));
            _graphicsDevice.ResizeMainWindow(width, height);
            DisplaySettings.Instance.ScreenWidth = width;
            DisplaySettings.Instance.ScreenHeight = height;
            _resizeEvents.OnNext(new ResizedEvent((int)width, (int)height));
        }

        protected virtual void Frame()
        {
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
                
                    //_window.Title = _windowTitle + ": FPS: " + avgFps.ToString("#.0");
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

            GraphicsDeviceOperations?.Invoke(_graphicsDevice, _factory);

            _endFrameEvents.OnNext(new EndFrameEvent(deltaSeconds));
            
//            _cl.Begin();
//            _cl.SetFramebuffer(_gd.SwapchainFramebuffer);
//            Random r = new Random();
//            _cl.ClearColorTarget(
//                0,
//                new RgbaFloat((float)r.NextDouble(), 0, 0, 1));
//            _cl.ClearDepthStencil(1);
//
//            // Do your rendering here (or call a subclass, etc.)
//
//            _cl.End();
//            _gd.SubmitCommands(_cl);
//            _gd.SwapBuffers();
        }
        
        
    }
}
    