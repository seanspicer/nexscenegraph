
using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators.Commands
{
    public interface ITranslateInLineCommand : IMotionCommand
    {
        Vector3 LineStart { get; }
        Vector3 LineEnd { get; }
        void SetLine(Vector3 start, Vector3 end);
        
        Vector3 Translation { get; set; }
    }

    public class TranslateInLineCommand : MotionCommand, ITranslateInLineCommand
    {
        private ILineSegment _lineSegment;

        public Vector3 LineStart => _lineSegment.Start;
        public Vector3 LineEnd => _lineSegment.End;

        public Vector3 Translation { get; set; }
        
        public static ITranslateInLineCommand Create(Vector3 start, Vector3 end)
        {
            return new TranslateInLineCommand(start, end);
        }

        protected TranslateInLineCommand(Vector3 start, Vector3 end)
        {
            SetLine(start, end);
        }

        public void SetLine(Vector3 start, Vector3 end)
        {
            _lineSegment = LineSegment.Create(start, end);
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
            return Matrix4x4.CreateTranslation(Translation);
        }

        public override IMotionCommand CreateCommandInverse()
        {
            var inverse = TranslateInLineCommand.Create(_lineSegment.Start, _lineSegment.End);
            inverse.Translation = -Translation;
            inverse.SetLocalToWorldAndWorldToLocal(GetLocalToWorld(), GetWorldToLocal());
            inverse.Stage = Stage;
            return inverse;
        }
    }
}