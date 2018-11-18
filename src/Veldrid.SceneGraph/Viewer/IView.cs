using Veldrid.SceneGraph.InputAdapter;

namespace Veldrid.SceneGraph.Viewer
{
    public interface IView
    {
        IGroup SceneData { get; set; }
        ICameraManipulator CameraManipulator { get; set; }
        ICamera Camera { get; set; }
        void AddInputEventHandler(IInputEventHandler handler);
        void OnInputEvent(IInputStateSnapshot snapshot);
    }
}