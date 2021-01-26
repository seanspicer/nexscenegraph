

using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IProjector
    {
        (bool, Vector3) Project(IPointerInfo pi);
    }
    
    public class Projector : IProjector
    {
        public virtual (bool, Vector3) Project(IPointerInfo pi)
        {
            throw new System.NotImplementedException();
        }
    }
}