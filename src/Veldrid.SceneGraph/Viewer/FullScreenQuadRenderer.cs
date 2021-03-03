using System.Collections.Generic;
using Veldrid.SceneGraph.RenderGraph;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Util;
using Veldrid.Utilities;

namespace Veldrid.SceneGraph.Viewer
{
    public class FullScreenQuadRenderer
    {
        private static readonly ushort[] s_quadIndices = {0, 1, 2, 0, 2, 3};

        private DisposeCollectorResourceFactory _factory;
        private DeviceBuffer _ib;
        private Pipeline _pipeline;
        private DeviceBuffer _vb;

        public void CreateDeviceObjects(GraphicsDevice gd, SceneContext sc)
        {
            if (null != _factory) _factory.DisposeCollector.DisposeAll();
            _factory = new DisposeCollectorResourceFactory(gd.ResourceFactory);

            var resourceLayout = _factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SourceTexture", ResourceKind.TextureReadOnly,
                    ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SourceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            (var vs, var fs) = RenderGroupState.GetShaders(gd, sc.OutputFramebuffer,
                FullScreenQuadShader.Instance.ShaderSet);

            var pd = new GraphicsPipelineDescription(
                new BlendStateDescription(
                    RgbaFloat.Black,
                    BlendAttachmentDescription.OverrideBlend),
                DepthStencilStateDescription.Disabled,
                new RasterizerStateDescription(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, true,
                    false),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(
                    new[]
                    {
                        new VertexLayoutDescription(
                            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate,
                                VertexElementFormat.Float2),
                            new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate,
                                VertexElementFormat.Float2))
                    },
                    new[] {vs, fs},
                    GetSpecializations(gd, sc)),
                new[] {resourceLayout},
                sc.OutputFramebuffer.OutputDescription);
            _pipeline = _factory.CreateGraphicsPipeline(ref pd);

            var verts = GetFullScreenQuadVerts(gd);

            _vb = _factory.CreateBuffer(new BufferDescription(verts.SizeInBytes() * sizeof(float),
                BufferUsage.VertexBuffer));
            gd.UpdateBuffer(_vb, 0, verts);

            _ib = _factory.CreateBuffer(
                new BufferDescription(s_quadIndices.SizeInBytes(), BufferUsage.IndexBuffer));
            gd.UpdateBuffer(_ib, 0, s_quadIndices);
        }

        private static SpecializationConstant[] GetSpecializations(GraphicsDevice gd, SceneContext sc)
        {
            var glOrGles = gd.BackendType == GraphicsBackend.OpenGL || gd.BackendType == GraphicsBackend.OpenGLES;

            var specializations = new List<SpecializationConstant>();
            specializations.Add(new SpecializationConstant(100, gd.IsClipSpaceYInverted));
            specializations.Add(new SpecializationConstant(101, glOrGles)); // TextureCoordinatesInvertedY
            specializations.Add(new SpecializationConstant(102, gd.IsDepthRangeZeroToOne));

            var swapchainFormat = sc.OutputFramebuffer.OutputDescription.ColorAttachments[0].Format;
            var swapchainIsSrgb = swapchainFormat == PixelFormat.B8_G8_R8_A8_UNorm_SRgb
                                  || swapchainFormat == PixelFormat.R8_G8_B8_A8_UNorm_SRgb;
            specializations.Add(new SpecializationConstant(103, false));

            return specializations.ToArray();
        }

        public void Render(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, sc.MainSceneViewResourceSet);
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);
            cl.DrawIndexed(6, 1, 0, 0, 0);
        }

        private static float[] GetFullScreenQuadVerts(GraphicsDevice gd)
        {
            if (gd.IsClipSpaceYInverted)
                return new float[]
                {
                    -1, -1, 0, 0,
                    1, -1, 1, 0,
                    1, 1, 1, 1,
                    -1, 1, 0, 1
                };

            if (gd.BackendType == GraphicsBackend.OpenGL)
                return new float[]
                {
                    -1, 1, 0, 1,
                    1, 1, 1, 1,
                    1, -1, 1, 0,
                    -1, -1, 0, 0
                };
            return new float[]
            {
                -1, 1, 0, 0,
                1, 1, 1, 0,
                1, -1, 1, 1,
                -1, -1, 0, 1
            };
        }
    }
}