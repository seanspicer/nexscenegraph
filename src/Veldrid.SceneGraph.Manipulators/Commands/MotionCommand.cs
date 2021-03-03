using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators.Commands
{
    public interface IMotionCommand
    {
        /**
         * Motion command are based on click-drag-release actions. So each
         * command needs to indicate which stage of the motion the command
         * represents.
         */
        enum MotionStage
        {
            None,

            /**
             * Click or pick start.
             */
            Start,

            /**
             * Drag or pick move.
             */
            Move,

            /**
             * Release or pick finish.
             */
            Finish
        }

        MotionStage Stage { get; set; }

        Matrix4x4 GetLocalToWorld();
        Matrix4x4 GetWorldToLocal();

        Matrix4x4 GetMotionMatrix();

        void SetLocalToWorldAndWorldToLocal(Matrix4x4 localToWorld, Matrix4x4 worldToLocal);

        void Accept(IConstraint constraint);
        void Accept(IDraggerCallback callback);
        IMotionCommand CreateCommandInverse();
    }

    public abstract class MotionCommand : IMotionCommand
    {
        private Matrix4x4 _localToWorld = Matrix4x4.Identity;
        private Matrix4x4 _worldToLocal = Matrix4x4.Identity;
        public abstract Matrix4x4 GetMotionMatrix();

        public Matrix4x4 GetLocalToWorld()
        {
            return _localToWorld;
        }

        public Matrix4x4 GetWorldToLocal()
        {
            return _worldToLocal;
        }

        public void SetLocalToWorldAndWorldToLocal(Matrix4x4 localToWorld, Matrix4x4 worldToLocal)
        {
            _localToWorld = localToWorld;
            _worldToLocal = worldToLocal;
        }

        public abstract IMotionCommand CreateCommandInverse();

        public IMotionCommand.MotionStage Stage { get; set; }

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