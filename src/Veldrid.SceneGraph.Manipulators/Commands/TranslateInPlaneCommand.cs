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
        protected TranslateInPlaneCommand(IPlane plane)
        {
            Plane = plane;
        }

        public IPlane Plane { get; set; }

        public Vector3 ReferencePoint { get; set; }

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
            var inverse = Create(Plane);
            SetInverseProperties(inverse);
            return inverse;
        }

        public static ITranslateInPlaneCommand Create(IPlane plane)
        {
            return new TranslateInPlaneCommand(plane);
        }
    }
}