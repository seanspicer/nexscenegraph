

using System.Linq;
using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators
{
    public class Util
    {
        internal static NodePath ComputeNodePathToRoot(INode node)
        {
            var result = new NodePath();
            
            var nodePaths = node.GetParentalNodePaths();
            if (!nodePaths.Any()) return result;
            
            result = nodePaths.First();
            if (nodePaths.Count > 1)
            {
                // TODO: Log this as degenerate case.
            }

            return result;
        }


    }
}