
using System.Numerics;
using Veldrid.SceneGraph.Manipulators.Commands;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IConstraint
    {
        bool Constrain(IMotionCommand motionCommand);
        bool Constrain(ITranslateInLineCommand command);
        bool Constrain(ITranslateInPlaneCommand command);
        bool Constrain(IScale1DCommand command);
        bool Constrain(IScale2DCommand command);
        bool Constrain(IScaleUniformCommand command);
        bool Constrain(IRotate3DCommand command);
    }
    
    public class Constraint : IConstraint
    {
        private Matrix4x4 _localToWorld = Matrix4x4.Identity;
        private Matrix4x4 _worldToLocal = Matrix4x4.Identity; 
        
        protected INode ReferenceNode { get; set; }
        
        protected Matrix4x4 GetLocalToWorld()
        {
            return _localToWorld;
        }

        protected Matrix4x4 GetWorldToLocal()
        {
            return _worldToLocal;
        }

        protected void ComputeLocalToWorldAndWorldToLocal()
        {
            
        }
        
        public virtual bool Constrain(IMotionCommand command)
        {
            return false;
        }

        public virtual bool Constrain(ITranslateInLineCommand command)
        {
            return Constrain(command as IMotionCommand);
        }

        public virtual bool Constrain(ITranslateInPlaneCommand command)
        {
            return Constrain(command as IMotionCommand);
        }

        public virtual bool Constrain(IScale1DCommand command)
        {
            return Constrain(command as IMotionCommand);
        }

        public virtual bool Constrain(IScale2DCommand command)
        {
            return Constrain(command as IMotionCommand);
        }

        public virtual bool Constrain(IScaleUniformCommand command)
        {
            return Constrain(command as IMotionCommand);
        }

        public virtual bool Constrain(IRotate3DCommand command)
        {
            return Constrain(command as IMotionCommand);
        }
    }
}