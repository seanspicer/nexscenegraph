using Veldrid.SceneGraph.Math.IsoSurface;

namespace Veldrid.SceneGraph.NodeKits.DirectVolumeRendering
{
    public interface ITextureVoxelVolume : IVoxelVolume
    {
        ITexture3D TextureData { get; }
    }
}