using Veldrid.SceneGraph;
using Veldrid.SceneGraph.Math.IsoSurface;
using Veldrid.SceneGraph.NodeKits.DirectVolumeRendering;

namespace Examples.Common
{
    public class VoxelVolume : IVoxelVolume
    {
        public double[,,] Values { get; }
        public double[,,] XValues { get; }
        public double[,,] YValues { get; }
        public double[,,] ZValues { get; }

        public VoxelVolume()
        {
            Values = new double[2, 2, 2];
            XValues = new double[2, 2, 2];
            YValues = new double[2, 2, 2];
            ZValues = new double[2, 2, 2];
        }
    }
    
    public class CornerVoxelVolume : VoxelVolume
    {
        public CornerVoxelVolume() : base()
        {
            for (var z = 0; z < 2; ++z)
            {
                for (var y = 0; y < 2; ++y)
                {
                    for (var x = 0; x < 2; ++x)
                    {
                        XValues[x, y, z] = 2*x-1;
                        YValues[x, y, z] = 2*y-1;
                        ZValues[x, y, z] = 2*z-1;
                        Values[x, y, z] = 0.0;
                    }
                }
            }

            Values[0, 0, 0] = 1.0;
            Values[1, 1, 1] = 1.0;
        }
    }
    
    public class SampledVolumeRenderingExampleScene
    {
        public static IGroup Build()
        {
            var root = Group.Create();
            root.AddChild(SampledVolumeNode.Create(new CornerVoxelVolume()));
            return root;
        }
    }
}