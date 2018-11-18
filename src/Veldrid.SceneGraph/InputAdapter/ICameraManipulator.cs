using System;

namespace Veldrid.SceneGraph.InputAdapter
{
    public interface ICameraManipulator : IInputEventHandler
    {
        void SetCamera(ICamera camera);
        void ViewAll();
    }
}