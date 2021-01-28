
using System.Numerics;

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
        public double Scale { get; set; } = 1.0;
        public double ScaleCenter { get; set; } = 0.0;
        public double ReferencePoint { get; set; } = 0.0;
        public double MinScale { get; set; } = 0.001;

        public static IScale1DCommand Create()
        {
            return new Scale1DCommand();
        }

        protected Scale1DCommand()
        {
            
        }
        
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
            throw new System.NotImplementedException();
        }

        public override IMotionCommand CreateCommandInverse()
        {
            throw new System.NotImplementedException();
        }
    }
}