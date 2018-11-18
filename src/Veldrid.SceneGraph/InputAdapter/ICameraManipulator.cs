using System;

namespace Veldrid.SceneGraph.InputAdapter
{
    public interface ICameraManipulator : IInputEventHandler
    {
        event Action RequestRedrawAction;
        void UpdateCamera(ICamera camera);
    }
}