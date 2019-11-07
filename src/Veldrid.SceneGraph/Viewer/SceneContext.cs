using System;
using System.Linq;
using Veldrid.Utilities;

namespace Veldrid.SceneGraph.Viewer
{
    public class SceneContext
    {
        public Texture MainSceneColorTexture { get; private set; }
        public Texture MainSceneDepthTexture { get; private set; }
        public Texture MainSceneResolvedColorTexture { get; private set; }
        public Framebuffer MainSceneFramebuffer { get; private set; }
        
        public Framebuffer OutputFramebuffer { get; private set; }

        public TextureSampleCount MainSceneSampleCount { get; internal set; }

        public SceneContext(TextureSampleCount sampleCount)
        {
            MainSceneSampleCount = sampleCount;
        }

        public void SetOutputFramebufffer(Framebuffer outputFramebuffer)
        {
            OutputFramebuffer = outputFramebuffer;
        }

        public void RecreateWindowSizedResources(GraphicsDevice gd, DisposeCollectorResourceFactory factory)
        {
            RecreateWindowSizedResources(gd, factory, gd.SwapchainFramebuffer.Width, gd.SwapchainFramebuffer.Height);
        }
        
        public void RecreateWindowSizedResources(GraphicsDevice gd, DisposeCollectorResourceFactory factory, uint width, uint height)
        {
            var colorTargetPixelFormat = OutputFramebuffer.OutputDescription.ColorAttachments.First().Format;
            var depthTargetPixelFormat = PixelFormat.D24_UNorm_S8_UInt;
            if (OutputFramebuffer.OutputDescription.DepthAttachment.HasValue)
            {
                depthTargetPixelFormat = OutputFramebuffer.OutputDescription.DepthAttachment.Value.Format;
            }
            else
            {
                throw new Exception("bad depth format");
            }
            
            gd.GetPixelFormatSupport(
                colorTargetPixelFormat,
                TextureType.Texture2D,
                TextureUsage.RenderTarget,
                out PixelFormatProperties properties);

            TextureSampleCount sampleCount = MainSceneSampleCount;
            while (!properties.IsSampleCountSupported(sampleCount))
            {
                sampleCount = sampleCount - 1;
            }

            TextureDescription mainColorDesc = TextureDescription.Texture2D(
                width,
                height,
                1,
                1,
                colorTargetPixelFormat,
                TextureUsage.RenderTarget | TextureUsage.Sampled,
                sampleCount);

            MainSceneColorTexture = factory.CreateTexture(ref mainColorDesc);
            if (sampleCount != TextureSampleCount.Count1)
            {
                mainColorDesc.SampleCount = TextureSampleCount.Count1;
                MainSceneResolvedColorTexture = factory.CreateTexture(ref mainColorDesc);
            }
            else
            {
                MainSceneResolvedColorTexture = MainSceneColorTexture;
            }
            //MainSceneResolvedColorView = factory.CreateTextureView(MainSceneResolvedColorTexture);
            MainSceneDepthTexture = factory.CreateTexture(TextureDescription.Texture2D(
                width,
                height,
                1,
                1,
                depthTargetPixelFormat,
                TextureUsage.DepthStencil,
                sampleCount));
            
            MainSceneFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(MainSceneDepthTexture, MainSceneColorTexture));
        }
        
    }
    
    
}