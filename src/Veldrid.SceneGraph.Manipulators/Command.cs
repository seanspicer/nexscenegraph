
using System.Data;
using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IMotionCommand
    {
        void Accept(IConstraint constraint);
        void Accept(IDraggerCallback callback);
    }
    
    public abstract class MotionCommand : IMotionCommand
    {
        public abstract Matrix4x4 GetMotionMatrix();

        public virtual void Accept(IConstraint constraint)
        {
            constraint.Constrain(this);
        }

        public virtual void Accept(IDraggerCallback callback)
        {
            callback.Receive(this);
        }
    }
}