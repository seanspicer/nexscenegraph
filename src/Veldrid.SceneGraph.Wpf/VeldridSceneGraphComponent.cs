using System;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using Veldrid;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Viewer;
using Veldrid.Utilities;
using PixelFormat = Veldrid.PixelFormat;

//using PixelFormat = System.Windows.Media.PixelFormat;

namespace Veldrid.SceneGraph.Wpf
{
    // This extends from the "Win32HwndControl" from the SharpDX example code.
    public class VeldridSceneGraphComponent : Win32HwndControl
    {
        public IGroup SceneData
        {
            get => ((View)_view)?.SceneData;
            set => ((View)_view).SceneData = value;
        }
        
        public IView View
        {
            get => _view;
        }
        
        public IObservable<IResizedEvent> ResizeEvents => _resizeEvents;
        public IObservable<IEndFrameEvent> EndFrameEvents => _endFrameEvents;
        public IObservable<IInputStateSnapshot> ViewerInputEvents => _viewerInputEvents;
        
        //private Swapchain _sc;
        private CommandList _cl;
        private GraphicsDevice _gd;

        private string _windowTitle = string.Empty;

        private ISubject<IResizedEvent> _resizeEvents;
        private ISubject<IEndFrameEvent> _endFrameEvents;
        private ISubject<IInputStateSnapshot> _viewerInputEvents;
        
        private GraphicsDevice _graphicsDevice;
        private DisposeCollectorResourceFactory _factory;
        private bool _windowResized = true;
        private bool _firstFrame = true;
        private Stopwatch _stopwatch = null;
        private double _previousElapsed = 0;
        private GraphicsBackend _preferredBackend = DisplaySettings.Instance.GraphicsBackend;
        private IView _view;
        private event Action<GraphicsDevice, ResourceFactory> GraphicsDeviceOperations;
        
        private const uint NFramesInBuffer = 30;
        private ulong _frameCounter = 0;
        private ulong _globalFrameCounter = 0;
        private double _frameTimeAccumulator = 0.0;
        private double _fpsDrawTimeAccumulator = 0.0;
        private readonly double[] _frameTimeBuff = new double[NFramesInBuffer];

        public bool Rendering { get; private set; }

        protected override sealed void Initialize()
        {
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
            
            _gd = GraphicsDevice.CreateD3D11(options, swapchainDesc);
            _cl = _gd.ResourceFactory.CreateCommandList();
            
            DisplaySettings.Instance.ScreenWidth = width;
            DisplaySettings.Instance.ScreenHeight = height;
            DisplaySettings.Instance.ScreenDistance = 1000.0f;
            
            //_view = Veldrid.SceneGraph.Wpf.View.Create(_resizeEvents);
            //GraphicsDeviceOperations += _view.Camera.Renderer.HandleOperation;
            //((View)_view).InputEvents = ViewerInputEvents;
            
            Rendering = true;
            CompositionTarget.Rendering += OnCompositionTargetRendering;
        }
        
        public void ViewAll()
        {
            ((View)_view).CameraManipulator?.ViewAll();
        }
        
        public void SetSceneData(IGroup root)
        {
            ((View)_view).SceneData = root;
        }

        public void SetCameraManipulator(ICameraManipulator cameraManipulator)
        {
            ((View)_view).CameraManipulator = cameraManipulator;
        }

        public void AddInputEventHandler(IInputEventHandler handler)
        {
            ((View)_view).AddInputEventHandler(handler);
        }

        protected override sealed void Uninitialize()
        {
            Rendering = false;
            CompositionTarget.Rendering -= OnCompositionTargetRendering;

            DestroySwapchain();
        }

        protected sealed override void Resized()
        {
            ResizeSwapchain();
        }

        private void OnCompositionTargetRendering(object sender, EventArgs eventArgs)
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
            uint width = (uint)(ActualWidth < 0 ? 0 : Math.Ceiling(ActualWidth * dpiScale));
            uint height = (uint)(ActualHeight < 0 ? 0 : Math.Ceiling(ActualHeight * dpiScale));
            _gd.ResizeMainWindow(width, height);
        }

        protected virtual void Frame()
        {
            _cl.Begin();
            _cl.SetFramebuffer(_gd.SwapchainFramebuffer);
            Random r = new Random();
            _cl.ClearColorTarget(
                0,
                new RgbaFloat((float)r.NextDouble(), 0, 0, 1));
            _cl.ClearDepthStencil(1);

            // Do your rendering here (or call a subclass, etc.)

            _cl.End();
            _gd.SubmitCommands(_cl);
            _gd.SwapBuffers();
        }
    }
}
    