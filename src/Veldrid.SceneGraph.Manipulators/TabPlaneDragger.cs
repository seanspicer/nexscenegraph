

using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface ITabPlaneDragger : ICompositeDragger
    {
        
    }
    
    public class TabPlaneDragger : CompositeDragger, ITabPlaneDragger
    {
        protected TabPlaneDragger(Matrix4x4 matrix) : base(matrix)
        {
        }
    }
}