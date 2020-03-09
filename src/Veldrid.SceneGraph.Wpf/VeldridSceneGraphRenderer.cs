using System;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Windows;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Viewer;
using Veldrid.SceneGraph.Wpf.Element;
using Veldrid.Utilities;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.DXGI.Device;
using Math = System.Math;

namespace Veldrid.SceneGraph.Wpf
{
    public class VeldridSceneGraphRenderer : BaseRenderer
    {
        private ISubject<IEndFrameEvent> _endFrameEvents;
        private ISubject<IInputStateSnapshot> _viewerInputEvents;

        private ISubject<float> _frameInfoSubject;
        public IObservable<float> FrameInfo => _frameInfoSubject;
        
        private IGroup _sceneData;
        public IGroup SceneData
        {
            get => _sceneData;
            set
            {
                _sceneData = value;
                if (null != _view)
                {
                    _view.SetSceneData(_sceneData);
                    CameraManipulator?.ViewAll();
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
                    _view.SetCameraManipulator(_cameraManipulator);
                    CameraManipulator?.ViewAll();
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
                    _view.AddInputEventHandler(_eventHandler);
                }
                
            }
        }

        private TextureSampleCount _fsaaCount;

        public TextureSampleCount FsaaCount
        {
            get => _fsaaCount;
            set
            {
                _fsaaCount = value;
                if (null != _graphicsDevice)
                {
                    _sceneContext.SetMainSceneSampleCount(_fsaaCount, _graphicsDevice, (uint)DisplaySettings.Instance.ScreenWidth, (uint)DisplaySettings.Instance.ScreenHeight);
                }
                
            }
        }

        private RgbaFloat _clearColor;

        public RgbaFloat ClearColor
        {
            get => _clearColor;
            set
            {
                _clearColor = value;
                if (null != _view)
                {
                    _view.Camera.SetClearColor(_clearColor);
                }
            }
        }
        
        
        public double DpiScale { get; set; }

        private Veldrid.SceneGraph.Viewer.IView _view;
        public Veldrid.SceneGraph.Viewer.IView View
        {
            get => _view;
        }
        
        public IObservable<IEndFrameEvent> EndFrameEvents => _endFrameEvents;
        public IObservable<IInputStateSnapshot> ViewerInputEvents => _viewerInputEvents;
        
        private WpfInputStateSnapshot _inputState;
        
        private CommandList _commandList;
        private GraphicsDevice _graphicsDevice;
        
        private SceneContext _sceneContext;
        
        private Framebuffer _offscreenFB;
        
        private Texture _offscreenColor;
        
        private Texture _offscreenDepth;

        private Fence _fence;
        
        private DisposeCollectorResourceFactory _factory;
        
        private event Action<GraphicsDevice, ResourceFactory> GraphicsDeviceOperations;
        private event Action<GraphicsDevice> GraphicsDeviceResize;
        
        private const uint NFramesInBuffer = 30;
        private ulong _frameCounter = 0;
        private ulong _globalFrameCounter = 0;
        private double _frameTimeAccumulator = 0.0;
        private double _fpsDrawTimeAccumulator = 0.0;
        private readonly double[] _frameTimeBuff = new double[NFramesInBuffer];
        private Stopwatch _stopwatch = null;
        private double _previousElapsed = 0;

        private bool _initialized;
        
        private readonly IUpdateVisitor _updateVisitor;
        
        public VeldridSceneGraphRenderer()
        {
            DpiScale = 1.0d;
            _initialized = false;
            _updateVisitor = UpdateVisitor.Create();
            _graphicsDevice = GraphicsDevice.CreateD3D11(new GraphicsDeviceOptions());
            
            if (_graphicsDevice.GetD3D11Info(out var backendInfo))
            {
                var d3d11Device = new SharpDX.Direct3D11.Device(backendInfo.Device);
                this.Renderer = new Element.D3D11(d3d11Device);
            }
            
            _factory = new DisposeCollectorResourceFactory(_graphicsDevice.ResourceFactory);
            _commandList = _graphicsDevice.ResourceFactory.CreateCommandList();
            _fence = _factory.CreateFence(false);
            _stopwatch = Stopwatch.StartNew();
            _previousElapsed = _stopwatch.Elapsed.TotalSeconds;
            _frameInfoSubject = new Subject<float>();
            
        }
        
        protected override void Attach()
        {
            if (Renderer == null)
                return;
            
            // Create Subjects
            _viewerInputEvents = new Subject<IInputStateSnapshot>();
            _endFrameEvents = new Subject<IEndFrameEvent>();
            _inputState = new WpfInputStateSnapshot();
            
            
        }

        protected override void Detach()
        {
            _viewerInputEvents.OnCompleted();
            _endFrameEvents.OnCompleted();
            
            _graphicsDevice.WaitForIdle();
            _factory.DisposeCollector.DisposeAll();
            
            // TODO - probably should dispose the graphics device here, need to figure out how to do it cleanly.
        }

        public void Initialize()
        {
            var view = Veldrid.SceneGraph.Viewer.View.Create();
            view.InputEvents = ViewerInputEvents;
            
            _sceneContext = new SceneContext(FsaaCount);
            _sceneContext.CreateDeviceObjects(_graphicsDevice, _factory);
            view.SceneContext = _sceneContext;

            if (null != _sceneData)
            {
                view.SetSceneData(_sceneData);
            }

            if (null != _cameraManipulator)
            {
                view.SetCameraManipulator(_cameraManipulator);
            }

            if (null != _eventHandler)
            {
                view.AddInputEventHandler(_eventHandler);
            }
            
            
            GraphicsDeviceOperations += view.Camera.Renderer.HandleOperation;
            GraphicsDeviceResize += view.Camera.Renderer.HandleResize;
            
            _view = view;
            
            
            CameraManipulator?.ViewAll();

            _initialized = true;
        }
        

        public void HandleInput(IInputStateSnapshot inputSnapshot)
        {
            _viewerInputEvents.OnNext(inputSnapshot);
        }

        protected override void ResetCore(DrawEventArgs args)
        {
            
            if (args.RenderSize.Width == 0 || args.RenderSize.Height == 0) return;
            
            double dpiScale = 1.0;  // TODO: Check this is okay
            uint width = (uint)(args.RenderSize.Width < 0 ? 0 : Math.Ceiling(args.RenderSize.Width * dpiScale));
            uint height = (uint)(args.RenderSize.Height < 0 ? 0 : Math.Ceiling(args.RenderSize.Height * dpiScale));

            DisplaySettings.Instance.SetScreenWidth(width);
            DisplaySettings.Instance.SetScreenHeight(height);

            //_graphicsDevice.ResizeMainWindow((uint) width, (uint) height);
            
            if (!_initialized)
            {
                DisplaySettings.Instance.SetScreenDistance(1000.0f);
                Initialize();
            }

            //_sceneContext.RecreateWindowSizedResources(_graphicsDevice, _factory, width, height);
            
            var mainColorDesc = TextureDescription.Texture2D(
                width,
                height,
                1,
                1,
                PixelFormat.B8_G8_R8_A8_UNorm,
                TextureUsage.RenderTarget);

            _offscreenColor = _factory.CreateTexture(ref mainColorDesc);
            
            _offscreenDepth = _factory.CreateTexture(TextureDescription.Texture2D(
                width, 
                height, 
                1, 
                1, 
                PixelFormat.D24_UNorm_S8_UInt, 
                TextureUsage.DepthStencil));
            
            _offscreenFB = _factory.CreateFramebuffer(new FramebufferDescription(_offscreenDepth, _offscreenColor));

            var od = _offscreenFB.OutputDescription;
            
            if (null != _view)
            {
                _sceneContext.SetOutputFramebufffer(_offscreenFB);
            }
            
            _sceneContext.RecreateWindowSizedResources(_graphicsDevice, _factory, width, height);
            
            GraphicsDeviceResize?.Invoke(_graphicsDevice);
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
                    var avgFps = (float)(NFramesInBuffer/_frameTimeAccumulator);
                
                    _frameInfoSubject.OnNext(avgFps);
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

            RenderingTraversal();

            _endFrameEvents.OnNext(new EndFrameEvent(deltaSeconds));
            
        }
        
        private void UpdateTraversal()
        {
            SceneData?.Accept(_updateVisitor);

            CameraManipulator?.UpdateCamera(_view.Camera);
        }

        private void RenderingTraversal()
        {
            GraphicsDeviceOperations?.Invoke(_graphicsDevice, _factory);
        }

        public override void RenderCore(DrawEventArgs args)
        {
            if (null != _view && null != _view.SceneContext && null != _view.SceneContext.OutputFramebuffer)
            {
                Frame();

                if (!_graphicsDevice.GetD3D11Info(out var backendInfo)) return;

                // Copy Render Target
                var d3d11RenderTarget =
                    new SharpDX.Direct3D11.Texture2D(backendInfo.GetTexturePointer(_offscreenColor));
                d3d11RenderTarget?.Device.ImmediateContext.CopyResource(d3d11RenderTarget, Renderer.RenderTarget);
            }
        }

        public void Resize(IResizedEvent resizeEvent)
        {
            _view.Camera.Resize(resizeEvent.Width, resizeEvent.Height, ResizeMask.ResizeDefault | ResizeMask.ResizeProjectionMatrix);
            
        }
    }
}