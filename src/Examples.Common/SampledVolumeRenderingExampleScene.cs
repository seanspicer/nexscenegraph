using System.Numerics;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.Math.IsoSurface;
using Veldrid.SceneGraph.NodeKits.DirectVolumeRendering;

namespace Examples.Common
{
    public class VoxelVolume : IVoxelVolume
    {
        protected int XDim = 4;
        protected int YDim = 2;
        protected int ZDim = 2;
        
        public double[,,] Values { get; }
        public double[,,] XValues { get; }
        public double[,,] YValues { get; }
        public double[,,] ZValues { get; }

        
        
        public VoxelVolume()
        {
            Values = new double[XDim, YDim, ZDim];
            XValues = new double[XDim, YDim, ZDim];
            YValues = new double[XDim, YDim, ZDim];
            ZValues = new double[XDim, YDim, ZDim];
        }
    }
    
    public class CornerVoxelVolume : VoxelVolume
    {
        
        public CornerVoxelVolume() : base()
        {
            for (var z = 0; z < ZDim; ++z)
            {
                for (var y = 0; y < YDim; ++y)
                {
                    for (var x = 0; x < XDim; ++x)
                    {
                        XValues[x, y, z] = x;
                        YValues[x, y, z] = y;
                        ZValues[x, y, z] = z;
                        Values[x, y, z] = 0;
                    }
                }
            }
            //
            // Values[0, 0, 0] = 1.0;
            // Values[1, 1, 1] = 1.0;
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