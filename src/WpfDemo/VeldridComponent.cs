using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using Veldrid;
using PixelFormat = Veldrid.PixelFormat;

//using PixelFormat = System.Windows.Media.PixelFormat;

namespace WpfDemo
{
    // This extends from the "Win32HwndControl" from the SharpDX example code.
    public class VeldridComponent : Win32HwndControl
    {
        //private Swapchain _sc;
        private CommandList _cl;
        private GraphicsDevice _gd;

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
            
            Rendering = true;
            CompositionTarget.Rendering += OnCompositionTargetRendering;
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

            Render();
        }

        private double GetDpiScale()
        {
            PresentationSource source = PresentationSource.FromVisual(this);

            return source.CompositionTarget.TransformToDevice.M11;
        }

        protected virtual SwapchainSource GetSwapchainSource()
        {
            Module mainModule = typeof(VeldridComponent).Module;
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

        protected virtual void Render()
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
    