using System;
using System.Reactive.Subjects;
using System.Windows;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Viewer;
using Veldrid.SceneGraph.Wpf.Element;
using Veldrid.Utilities;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.DXGI.Device;

namespace Veldrid.SceneGraph.Wpf
{
    public class VeldridSceneGraphRenderer : BaseRenderer
    {
        private ISubject<IResizedEvent> _resizeEvents;
        private ISubject<IEndFrameEvent> _endFrameEvents;
        private ISubject<IInputStateSnapshot> _viewerInputEvents;
        
        private IGroup _sceneData;
        public IGroup SceneData
        {
            get => _sceneData;
            set
            {
                _sceneData = value;
                if (null != _view)
                {
                    ((View) _view).SceneData = _sceneData;
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
                    ((View) _view).CameraManipulator = _cameraManipulator;
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
                    _eventHandler.SetView(_view);
                    ((View) _view).AddInputEventHandler(_eventHandler);
                }
                
            }
        }

        private IView _view;
        public IView View
        {
            get => _view;
        }
        
        public IObservable<IResizedEvent> ResizeEvents => _resizeEvents;
        public IObservable<IEndFrameEvent> EndFrameEvents => _endFrameEvents;
        public IObservable<IInputStateSnapshot> ViewerInputEvents => _viewerInputEvents;
        
        private WpfInputStateSnapshot _inputState;
        
        private Swapchain _sc;
        private CommandList _cl;
        private GraphicsDevice _gd;
        
        private Framebuffer _offscreenFB;
        private Texture _offscreenColor;

        private Fence _fence;
        
        private DisposeCollectorResourceFactory _factory;
        
        public VeldridSceneGraphRenderer()
        {
            _gd = GraphicsDevice.CreateD3D11(new GraphicsDeviceOptions());
            
            if (_gd.GetD3D11Info(out var backendInfo))
            {
                var d3d11Device = new SharpDX.Direct3D11.Device(backendInfo.Device);
                this.Renderer = new Element.D3D11(d3d11Device);
            }
            
            _factory = new DisposeCollectorResourceFactory(_gd.ResourceFactory);
            _cl = _gd.ResourceFactory.CreateCommandList();
            _fence = _factory.CreateFence(false);
            
            // Create Subjects
            _viewerInputEvents = new Subject<IInputStateSnapshot>();
            _endFrameEvents = new Subject<IEndFrameEvent>();
            _resizeEvents = new Subject<IResizedEvent>();
            _inputState = new WpfInputStateSnapshot();
        }
        
        
        protected override void Attach()
        {
            if (Renderer == null)
                return;
            
        }

        protected override void Detach()
        {
            _factory.DisposeCollector.DisposeAll();
        }

        protected override void ResetCore(DrawEventArgs args)
        {
            if (args.RenderSize.Width == 0 || args.RenderSize.Width == 0) return;
            
            double dpiScale = 1.0;  // TODO: Check this is okay
            uint width = (uint)(args.RenderSize.Width < 0 ? 0 : Math.Ceiling(args.RenderSize.Width * dpiScale));
            uint height = (uint)(args.RenderSize.Height < 0 ? 0 : Math.Ceiling(args.RenderSize.Height * dpiScale));

            _offscreenColor = _factory.CreateTexture(TextureDescription.Texture2D(
                width, height, 1, 1,
                PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled));
            
            Texture offscreenDepth = _factory.CreateTexture(TextureDescription.Texture2D(
                width, height, 1, 1, PixelFormat.R16_UNorm, TextureUsage.DepthStencil));
            _offscreenFB = _factory.CreateFramebuffer(new FramebufferDescription(offscreenDepth, _offscreenColor));
        }

        public override void RenderCore(DrawEventArgs args)
        {
            _fence.Reset();
            _cl.Begin();
            _cl.SetFramebuffer(_offscreenFB);
            Random r = new Random();
            _cl.ClearColorTarget(
                0,
                new RgbaFloat((float)r.NextDouble(), 0, 0, 1));
            _cl.ClearDepthStencil(1);

            // Do your rendering here (or call a subclass, etc.)

            _cl.End();
            _gd.SubmitCommands(_cl, _fence);

            if (!_gd.GetD3D11Info(out var backendInfo)) return;
            
            var d3d11Texture = new SharpDX.Direct3D11.Texture2D(backendInfo.GetTexturePointer(_offscreenColor));
                
            _gd.WaitForFence(_fence);
            d3d11Texture?.Device.ImmediateContext.CopyResource(d3d11Texture, Renderer.RenderTarget);

        }
    }
}