using System.Numerics;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Manipulators.Commands
{
    public interface IScale1DCommand : IMotionCommand
    {
        double Scale { get; set; }
        double ScaleCenter { get; set; }
        double ReferencePoint { get; set; }
        double MinScale { get; set; }
    }

    public class Scale1DCommand : MotionCommand, IScale1DCommand
    {
        protected Scale1DCommand()
        {
        }

        public double Scale { get; set; } = 1.0;
        public double ScaleCenter { get; set; } = 0.0;
        public double ReferencePoint { get; set; } = 0.0;
        public double MinScale { get; set; } = 0.001;

        public override void Accept(IConstraint constraint)
        {
            constraint.Constrain(this);
        }

        public override void Accept(IDraggerCallback callback)
        {
            callback.Receive(this);
        }

        public override Matrix4x4 GetMotionMatrix()
        {
            return Matrix4x4.CreateTranslation((float) -ScaleCenter, 0.0f, 0.0f)
                .PostMultiply(Matrix4x4.CreateScale((float) Scale, 1.0f, 1.0f))
                .PostMultiply(Matrix4x4.CreateTranslation((float) ScaleCenter, 0.0f, 0.0f));
        }

        public override IMotionCommand CreateCommandInverse()
        {
            var inverse = Create();
            inverse.ScaleCenter = ScaleCenter;
            inverse.ReferencePoint = ReferencePoint;
            inverse.Stage = Stage;
            inverse.MinScale = MinScale;
            inverse.Scale = 1.0 / Scale;
            inverse.SetLocalToWorldAndWorldToLocal(GetLocalToWorld(), GetWorldToLocal());
            return inverse;
        }

        public static IScale1DCommand Create()
        {
            return new Scale1DCommand();
        }
    }
}