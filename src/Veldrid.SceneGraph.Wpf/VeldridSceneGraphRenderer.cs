using System;
using System.Windows;
using Veldrid.SceneGraph.Wpf.Element;
using Veldrid.Utilities;

namespace Veldrid.SceneGraph.Wpf
{
    public class VeldridSceneGraphRenderer : IDirect3D
    {
        private Swapchain _sc;
        private CommandList _cl;
        private GraphicsDevice _gd;
        
        private Framebuffer _offscreenFB;
        private Texture _offscreenColor;
        
        private DisposeCollectorResourceFactory _factory;
        
        public VeldridSceneGraphRenderer()
        {
            _gd = GraphicsDevice.CreateD3D11(new GraphicsDeviceOptions());
            _factory = new DisposeCollectorResourceFactory(_gd.ResourceFactory);
            _cl = _gd.ResourceFactory.CreateCommandList();
        }
        
        public void Reset(DrawEventArgs args)
        {
            if (args.RenderSize.Width == 0 || args.RenderSize.Width == 0) return;
            
            double dpiScale = 1.0;  // TODO: Check this is okay
            uint width = (uint)(args.RenderSize.Width < 0 ? 0 : Math.Ceiling(args.RenderSize.Width * dpiScale));
            uint height = (uint)(args.RenderSize.Height < 0 ? 0 : Math.Ceiling(args.RenderSize.Height * dpiScale));

            _offscreenColor = _factory.CreateTexture(TextureDescription.Texture2D(
                width, height, 1, 1,
                PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled));
            Texture offscreenDepth = _factory.CreateTexture(TextureDescription.Texture2D(
                width, height, 1, 1, PixelFormat.R16_UNorm, TextureUsage.DepthStencil));
            _offscreenFB = _factory.CreateFramebuffer(new FramebufferDescription(offscreenDepth, _offscreenColor));
        }

        public void Render(DrawEventArgs args)
        {
            _cl.Begin();
            _cl.SetFramebuffer(_offscreenFB);
            Random r = new Random();
            _cl.ClearColorTarget(
                0,
                new RgbaFloat((float)r.NextDouble(), 0, 0, 1));
            _cl.ClearDepthStencil(1);

            // Do your rendering here (or call a subclass, etc.)

            _cl.End();
            _gd.SubmitCommands(_cl);
            
            // Now just need to copy the offscreen buffer to the render target (I think!)
        }
    }
}