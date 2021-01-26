
namespace Veldrid.SceneGraph.Manipulators
{
    public interface IConstraint
    {
        bool Constrain(IMotionCommand motionCommand);
    }
    
    public class Constraint : IConstraint
    {
        public virtual bool Constrain(IMotionCommand command)
        {
            return false;
        }
    }
}