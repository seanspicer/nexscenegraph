
using System.Numerics;

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
        public Vector2 Scale { get; set; } = Vector2.One;
        public Vector2 ScaleCenter { get; set; } = Vector2.Zero;
        public Vector2 ReferencePoint { get; set; } = Vector2.Zero;
        public Vector2 MinScale { get; set; } = new Vector2(0.001f, 0.001f);
        
        public override Matrix4x4 GetMotionMatrix()
        {
            throw new System.NotImplementedException();
        }

        public override IMotionCommand CreateCommandInverse()
        {
            throw new System.NotImplementedException();
        }
        
        public override void Accept(IConstraint constraint)
        {
            constraint.Constrain(this);
        }

        public override void Accept(IDraggerCallback callback)
        {
            callback.Receive(this);
        }
    }
}