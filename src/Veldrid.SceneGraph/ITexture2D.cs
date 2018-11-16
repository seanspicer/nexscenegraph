using AssetPrimitives;

namespace Veldrid.SceneGraph
{
    public interface ITexture2D
    {
        ProcessedTexture ProcessedTexture { get; }
        uint ResourceSetNo { get; set; }
        string TextureName { get; set; }
        string SamplerName { get; set; }
    }
}