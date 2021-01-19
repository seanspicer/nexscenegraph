using Veldrid.SceneGraph;
using Veldrid.SceneGraph.NodeKits.DirectVolumeRendering;

namespace Examples.Common
{
    public class SampledVolumeRenderingExampleScene
    {
        public static IGroup Build()
        {
            var root = Group.Create();
            root.AddChild(SampledVolumeNode.Create());
            return root;
        }
    }
}