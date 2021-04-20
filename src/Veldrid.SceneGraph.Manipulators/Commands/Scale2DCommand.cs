using System.Numerics;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Manipulators.Commands
{
    public interface IScale2DCommand : IMotionCommand
    {
        Vector2 Scale { get; set; }
        Vector2 ScaleCenter { get; set; }
        Vector2 ReferencePoint { get; set; }
        Vector2 MinScale { get; set; }
    }

    public class Scale2DCommand : MotionCommand, IScale2DCommand
    {
        protected Scale2DCommand()
        {
        }

        public Vector2 Scale { get; set; } = Vector2.One;
        public Vector2 ScaleCenter { get; set; } = Vector2.Zero;
        public Vector2 ReferencePoint { get; set; } = Vector2.Zero;
        public Vector2 MinScale { get; set; } = new Vector2(0.001f, 0.001f);

        public override Matrix4x4 GetMotionMatrix()
        {
            return Matrix4x4.CreateTranslation(-ScaleCenter.X, 0.0f, -ScaleCenter.Y)
                .PostMultiply(Matrix4x4.CreateScale(Scale.X, 1.0f, Scale.Y))
                .PostMultiply(Matrix4x4.CreateTranslation(ScaleCenter.X, 0.0f, ScaleCenter.Y));
        }

        public override IMotionCommand CreateCommandInverse()
        {
            var inverse = Create();
            inverse.ScaleCenter = ScaleCenter;
            inverse.ReferencePoint = ReferencePoint;
            inverse.Stage = Stage;
            inverse.MinScale = MinScale;
            inverse.Scale = new Vector2(1.0f / Scale.X, 1.0f / Scale.Y);
            inverse.SetLocalToWorldAndWorldToLocal(GetLocalToWorld(), GetWorldToLocal());
            return inverse;
        }

        public override void Accept(IConstraint constraint)
        {
            constraint.Constrain(this);
        }

        public override void Accept(IDraggerCallback callback)
        {
            callback.Receive(this);
        }

        public static IScale2DCommand Create()
        {
            return new Scale2DCommand();
        }
    }
}