
using System.Collections.Generic;
using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface ICompositeDragger : IDragger
    {
        
    }
    
    public class CompositeDragger : Dragger, ICompositeDragger
    {
        protected List<IDragger> DraggerList { get; set; }
        
        protected CompositeDragger(Matrix4x4 matrix) : base(matrix)
        {
        }
    }
}