
using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators.Commands
{
    public interface ITranslateInPlaneCommand : IMotionCommand
    {
        IPlane Plane { get; set; }
        Vector3 Translation { get; set; }
        Vector3 ReferencePoint { get; set; }
    }

    public class TranslateInPlaneCommand : MotionCommand, ITranslateInPlaneCommand
    {
        public IPlane Plane { get; set; }
        
        public Vector3 Translation { get; set; }
        
        public Vector3 ReferencePoint { get; set; }
        
        public static ITranslateInPlaneCommand Create(IPlane plane)
        {
            return new TranslateInPlaneCommand(plane);
        }

        protected TranslateInPlaneCommand(IPlane plane)
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