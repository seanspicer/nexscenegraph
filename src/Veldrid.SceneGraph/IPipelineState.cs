using System.Collections.Generic;

namespace Veldrid.SceneGraph
{
    public interface IPipelineState
    {
        ShaderDescription? VertexShaderDescription { get; set; }
        ShaderDescription? FragmentShaderDescription { get; set; }
        IReadOnlyList<ITexture2D> TextureList { get; }
        BlendStateDescription BlendStateDescription { get; set; }
        DepthStencilStateDescription DepthStencilState { get; set; }
        RasterizerStateDescription RasterizerStateDescription { get; set; }

        void AddTexture(ITexture2D texture);
    }
}