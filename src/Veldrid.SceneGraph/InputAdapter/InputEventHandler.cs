using System.Numerics;

namespace Veldrid.SceneGraph.InputAdapter
{
    public abstract class InputEventHandler : IInputEventHandler
    {
        protected InputStateTracker InputStateTracker { get; }= new InputStateTracker();

        public virtual void HandleInput(InputStateSnapshot snapshot)
        {
            InputStateTracker.UpdateFrameInput(snapshot);
        }

        protected Vector2 GetNormalizedMousePosition()
        {
            var xNorm = 2.0f*(InputStateTracker.MousePosition.Value.X / InputStateTracker.FrameSnapshot.WindowWidth)-1.0f;
            var yNorm = -2.0f*(InputStateTracker.MousePosition.Value.Y / InputStateTracker.FrameSnapshot.WindowHeight)+1.0f;

            return new Vector2(xNorm, yNorm);
        }
    }
    
}