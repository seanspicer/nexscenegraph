using System;
using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators.Commands
{
    public interface IScaleUniformCommand : IMotionCommand
    {
        double Scale { get; set; }
        Vector3 ScaleCenter { get; set; }
    }

    public class ScaleUniformCommand : MotionCommand, IScaleUniformCommand
    {
        protected ScaleUniformCommand()
        {
        }

        public double Scale { get; set; } = 1.0;

        public Vector3 ScaleCenter { get; set; } = Vector3.Zero;

        public override Matrix4x4 GetMotionMatrix()
        {
            throw new NotImplementedException();
        }

        public override IMotionCommand CreateCommandInverse()
        {
            throw new NotImplementedException();
        }

        public override void Accept(IConstraint constraint)
        {
            constraint.Constrain(this);
        }

        public override void Accept(IDraggerCallback callback)
        {
            callback.Receive(this);
        }

        public static IScaleUniformCommand Create()
        {
            return new ScaleUniformCommand();
        }
    }
}