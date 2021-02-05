
using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators.Commands
{
    public interface ITranslateInPlaneCommand : ITranslateCommand
    {
        IPlane Plane { get; set; }
        Vector3 ReferencePoint { get; set; }
    }

    public class TranslateInPlaneCommand : TranslateCommand, ITranslateInPlaneCommand
    {
        public IPlane Plane { get; set; }
        
        public Vector3 ReferencePoint { get; set; }
        
        public static ITranslateInPlaneCommand Create(IPlane plane)
        {
            return new TranslateInPlaneCommand(plane);
        }

        protected TranslateInPlaneCommand(IPlane plane)
        {
            Plane = plane;
        }
        
        public override void Accept(IConstraint constraint)
        {
            constraint.Constrain(this);
        }

        public override void Accept(IDraggerCallback callback)
        {
            callback.Receive(this);
        }
        
        public override IMotionCommand CreateCommandInverse()
        {
            var inverse = TranslateInPlaneCommand.Create(Plane);
            SetInverseProperties(inverse);
            return inverse;
        }
    }
}