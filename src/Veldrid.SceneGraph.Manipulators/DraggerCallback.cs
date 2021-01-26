
namespace Veldrid.SceneGraph.Manipulators
{
    public interface IDraggerCallback
    {
        bool Receive(IMotionCommand command);
    }
    
    public class DraggerCallback : IDraggerCallback
    {
        public virtual bool Receive(IMotionCommand command)
        {
            return false;
        }
    }
}