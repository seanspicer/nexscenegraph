namespace Veldrid.SceneGraph.Math.IsoSurface
{
    public interface IVoxelVolume
    {
        double[,,] Values { get; }
        double[,,] XValues { get; }
        double[,,] YValues { get; }
        double[,,] ZValues { get; }
    }
}