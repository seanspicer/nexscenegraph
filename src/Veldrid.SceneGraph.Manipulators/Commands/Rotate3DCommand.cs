using System;
using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators.Commands
{
    public interface IRotate3DCommand : IMotionCommand
    {
        Quaternion Rotation { get; set; }
    }

    public class Rotate3DCommand : MotionCommand, IRotate3DCommand
    {
        protected Rotate3DCommand()
        {
        }

        public Quaternion Rotation { get; set; } = Quaternion.Identity;

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

        public static IRotate3DCommand Create()
        {
            return new Rotate3DCommand();
        }
    }
}