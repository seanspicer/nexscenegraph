

using System.Collections.Generic;

namespace Veldrid.SceneGraph.Math.IsoSurface
{
    public interface IIsoSurfaceGenerator
    {
        IIsoSurface CreateIsoSurface(IVoxelVolume voxelVolume, double isoValue);
    }
}