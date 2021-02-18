
using Veldrid.SceneGraph.Manipulators.Commands;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IDraggerCallback
    {
        bool Receive(IMotionCommand command);
        bool Receive(ITranslateInLineCommand command);
        bool Receive(ITranslateInPlaneCommand command);
        bool Receive(IScale1DCommand command);
        bool Receive(IScale2DCommand command);
        bool Receive(IScaleUniformCommand command);
        bool Receive(IRotate3DCommand command);
    }
    
    public class DraggerCallback : IDraggerCallback
    {
        public virtual bool Receive(IMotionCommand command)
        {
            return false;
        }
        
        public virtual bool Receive(ITranslateInLineCommand command)
        {
            return Receive(command as IMotionCommand);
        }

        public virtual bool Receive(ITranslateInPlaneCommand command)
        {
            return Receive(command as IMotionCommand);
        }

        public virtual bool Receive(IScale1DCommand command)
        {
            return Receive(command as IMotionCommand);
        }

        public virtual bool Receive(IScale2DCommand command)
        {
            return Receive(command as IMotionCommand);
        }

        public virtual bool Receive(IScaleUniformCommand command)
        {
            return Receive(command as IMotionCommand);
        }

        public virtual bool Receive(IRotate3DCommand command)
        {
            return Receive(command as IMotionCommand);
        }
    }
}