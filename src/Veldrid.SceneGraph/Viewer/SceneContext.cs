using System;
using System.Linq;
using Veldrid.Utilities;

namespace Veldrid.SceneGraph.Viewer
{
    public class SceneContext
    {
        public ResourceLayout TextureSamplerResourceLayout { get; private set; }
        
        public Texture MainSceneColorTexture { get; private set; }
        public Texture MainSceneDepthTexture { get; private set; }
        public Texture MainSceneResolvedColorTexture { get; private set; }
        public Framebuffer MainSceneFramebuffer { get; private set; }

        public ResourceSet MainSceneViewResourceSet { get; private set; }
        
        public TextureView MainSceneResolvedColorView { get; private set; }
        
        public Framebuffer OutputFramebuffer { get; private set; }

        public TextureSampleCount MainSceneSampleCount { get; internal set; }

        public SceneContext(TextureSampleCount sampleCount)
        {
            MainSceneSampleCount = sampleCount;
        }

        private DisposeCollectorResourceFactory _factory;
        
        public void CreateDeviceObjects(GraphicsDevice gd, ResourceFactory factory)
        {
            TextureSamplerResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SourceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SourceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
        }
        
        public void SetOutputFramebufffer(Framebuffer outputFramebuffer)
        {
            OutputFramebuffer = outputFramebuffer;
        }

        public void RecreateWindowSizedResources(GraphicsDevice gd, ResourceFactory factory)
        {
            RecreateWindowSizedResources(gd, factory, gd.SwapchainFramebuffer.Width, gd.SwapchainFramebuffer.Height);
        }
        
        public void RecreateWindowSizedResources(GraphicsDevice gd, ResourceFactory foo, uint width, uint height)
        {
            _factory?.DisposeCollector.DisposeAll();
            
            _factory = new DisposeCollectorResourceFactory(gd.ResourceFactory);
            
            
            var colorTargetPixelFormat = PixelFormat.R16_G16_B16_A16_Float;
            //OutputFramebuffer.OutputDescription.ColorAttachments.First().Format;
            var depthTargetPixelFormat = PixelFormat.R32_Float;
//            if (OutputFramebuffer.OutputDescription.DepthAttachment.HasValue)
//            {
//                depthTargetPixelFormat = OutputFramebuffer.OutputDescription.DepthAttachment.Value.Format;
//            }
//            else
//            {
//                throw new Exception("bad depth format");
//            }
            
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

            MainSceneColorTexture = _factory.CreateTexture(ref mainColorDesc);
            if (sampleCount != TextureSampleCount.Count1)
            {
                mainColorDesc.SampleCount = TextureSampleCount.Count1;
                MainSceneResolvedColorTexture = _factory.CreateTexture(ref mainColorDesc);
            }
            else
            {
                MainSceneResolvedColorTexture = MainSceneColorTexture;
            }
            MainSceneResolvedColorView = _factory.CreateTextureView(MainSceneResolvedColorTexture);
            MainSceneDepthTexture = _factory.CreateTexture(TextureDescription.Texture2D(
                width,
                height,
                1,
                1,
                depthTargetPixelFormat,
                TextureUsage.DepthStencil,
                sampleCount));
            
            MainSceneFramebuffer = _factory.CreateFramebuffer(new FramebufferDescription(MainSceneDepthTexture, MainSceneColorTexture));
            MainSceneViewResourceSet = _factory.CreateResourceSet(new ResourceSetDescription(TextureSamplerResourceLayout, MainSceneResolvedColorView, gd.PointSampler));

        }
        
    }
    
    
}