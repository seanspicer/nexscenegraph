using System;
using System.Diagnostics;
using System.Numerics;
using Veldrid;
using Veldrid.SceneGraph.Viewer;
using WebAssembly;

namespace Veldrid.SceneGraph.Web
{
    public class WebViewer : Viewer.View, IViewer
    {
        const int CanvasWidth = 1280;
        const int CanvasHeight = 720;

        private readonly Action<double> _loop;
        private readonly AdvancedFrameLoop _frameLoop;
        private double _deltaSeconds;
        private double _previousMilliseconds;

        static JSObject _window;

        private uint _width;
        private uint _height;
        private InputState _inputState = new InputState();

        public Platform PlatformType { get; }
        public uint Width => MainSwapchain.Width;
        public uint Height => MainSwapchain.Height;
        
        private event Func<uint, GraphicsDevice, ResourceFactory, Framebuffer, CommandBuffer[]> HandleFrameDelegate;
        private event Action<GraphicsDevice> GraphicsDeviceResize;
        
        public void SetBackgroundColor(RgbaFloat color)
        {
            Camera.SetClearColor(color);
        }

        public void ViewAll()
        {
            CameraManipulator?.ViewAll(this);
        }

        void IViewer.Run()
        {
            Run();
        }

        public override void SetCamera(ICamera camera)
        {
            HandleFrameDelegate -= Camera.Renderer.HandleFrame;
            GraphicsDeviceResize -= Camera.Renderer.HandleResize;

            base.SetCamera(camera);

            HandleFrameDelegate += Camera.Renderer.HandleFrame;
            GraphicsDeviceResize += Camera.Renderer.HandleResize;
        }

        public void SetCameraOrthographic()
        {
            var camera = OrthographicCamera.Create(
                DisplaySettings.Instance(this).ScreenWidth,
                DisplaySettings.Instance(this).ScreenHeight,
                DisplaySettings.Instance(this).ScreenDistance);
            SetCamera(camera);
        }

        public void SetCameraPerspective()
        {
            var camera = PerspectiveCamera.Create(
                DisplaySettings.Instance(this).ScreenWidth,
                DisplaySettings.Instance(this).ScreenHeight,
                DisplaySettings.Instance(this).ScreenDistance);
            SetCamera(camera);
        }

        public GraphicsDevice Device { get; }

        private Stopwatch _sw;
        private Vector2 _previousMousePosition;

        public Swapchain MainSwapchain => Device.MainSwapchain;

        public uint FrameIndex => MainSwapchain.LastAcquiredImage;

        public uint BufferCount => MainSwapchain.BufferCount;

        public bool SupportsImGui => false;

        public event Action Resized;
        public event Action<double> Update;
        public event Func<double, CommandBuffer[]> Render;

        public static IViewer Create(string title, TextureSampleCount fsSampleCount = TextureSampleCount.Count1)
        {
            return new WebViewer(title);
        }
        
        public WebViewer(string title) : base(CanvasWidth, CanvasHeight, 1000.0f)
        {
            if (!IsWebGL2Supported())
            {
                HtmlHelper.AddParagraph("WebGL2 is required to run the Veldrid Sample Gallery.");
                return;
            }

            HtmlHelper.AddHeader1(title);

            string divCanvasName = $"div_canvas";
            string canvasName = $"canvas";
            JSObject canvas = HtmlHelper.AddCanvas(divCanvasName, canvasName, CanvasWidth, CanvasHeight);
            GraphicsDeviceOptions options = GetPreferredOptions();
            Device = GraphicsDevice.CreateWebGL(options, canvas);
            _loop = new Action<double>(Loop);
            _frameLoop = new AdvancedFrameLoop(Device, MainSwapchain);
            _window = (JSObject)Runtime.GetGlobalObject();

            HandleFrameDelegate += Camera.Renderer.HandleFrame;
            GraphicsDeviceResize += Camera.Renderer.HandleResize;
            
            canvas.Invoke("addEventListener", "mousemove", new Action<JSObject>((mouseEvent) =>
            {
                int mX = (int)mouseEvent.GetObjectProperty("clientX");
                int mY = (int)mouseEvent.GetObjectProperty("clientY");
                _inputState.MousePosition = new Vector2(mX, mY);

                mouseEvent.Dispose();
            }), false);
            canvas.Invoke("addEventListener", "mousedown", new Action<JSObject>((mouseEvent) =>
            {
                int button = (int)mouseEvent.GetObjectProperty("button");
                _inputState.MouseDown[button] = true;

                mouseEvent.Dispose();
            }), false);
            canvas.Invoke("addEventListener", "mouseup", new Action<JSObject>((mouseEvent) =>
            {
                int button = (int)mouseEvent.GetObjectProperty("button");
                _inputState.MouseDown[button] = false;

                mouseEvent.Dispose();
            }), false);
            using (var document = (JSObject)Runtime.GetGlobalObject("document"))
            {
                document.Invoke("addEventListener", "keydown", new Action<JSObject>((keyEvent) =>
                {
                    string keyStr = (string)keyEvent.GetObjectProperty("key");
                    if (TryParseKeyCode(keyStr, out Key key))
                    {
                        _inputState.KeyEvents.Add(new KeyEvent(key, true, ModifierKeys.None));
                    }

                    keyEvent.Dispose();
                }), false);
                document.Invoke("addEventListener", "keyup", new Action<JSObject>((keyEvent) =>
                {
                    string keyStr = (string)keyEvent.GetObjectProperty("key");
                    if (TryParseKeyCode(keyStr, out Key key))
                    {
                        _inputState.KeyEvents.Add(new KeyEvent(key, false, ModifierKeys.None));
                    }

                    keyEvent.Dispose();
                }), false);
            }
        }

        public static GraphicsDeviceOptions GetPreferredOptions()
        {
#if DEBUG
            bool isDebugBuild = true;
#else
            bool isDebugBuild = false;
#endif

            return new GraphicsDeviceOptions(
                debug: isDebugBuild,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: false,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true,
                swapchainSrgbFormat: true,
                enableCommandBuffers: true);

        }
        
        private bool TryParseKeyCode(string keyStr, out Key key)
        {
            if (Enum.TryParse(keyStr, ignoreCase: true, out key)) { return true; }

            switch (keyStr)
            {
                case "ArrowLeft":
                    key = Key.Left; return true;
                case "ArrowRight":
                    key = Key.Right; return true;
                case "ArrowUp":
                    key = Key.Up; return true;
                case "ArrowDown":
                    key = Key.Down; return true;
                case " ":
                    key = Key.Space; return true;

                default:
                    Console.WriteLine($"Couldn't parse key string: {keyStr}");
                    return false;
            }
        }

        public InputStateView GetInputState() => _inputState.View;

        internal void Run()
        {
            _sw = Stopwatch.StartNew();
            RequestAnimationFrame();
        }

        static bool IsWebGL2Supported()
        {
            if (_window == null)
            {
                _window = (JSObject)Runtime.GetGlobalObject();
            }

            // This is a very simple check for WebGL2 support.
            return _window.GetObjectProperty("WebGL2RenderingContext") != null;
        }

        private void Loop(double milliseconds)
        {
            _deltaSeconds = (milliseconds - _previousMilliseconds) / 1000;
            _previousMilliseconds = milliseconds;
            FlushInput();
            Update?.Invoke(_deltaSeconds);

            RequestAnimationFrame();
        }

        private void FlushInput()
        {
            _inputState.MouseDelta = _inputState.MousePosition - _previousMousePosition;
            _previousMousePosition = _inputState.MousePosition;
        }

        private void RequestAnimationFrame()
        {
            _frameLoop.RunFrame(HandleFrame);
            _window.Invoke("requestAnimationFrame", _loop);
        }

        private CommandBuffer[] HandleFrame(uint frameIndex, Framebuffer fb)
        {
            return HandleFrameDelegate?.Invoke(frameIndex, Device, Device.ResourceFactory, fb);
        }
        
    }
}