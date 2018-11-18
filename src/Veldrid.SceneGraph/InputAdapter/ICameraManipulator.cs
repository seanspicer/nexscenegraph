using System;

namespace Veldrid.SceneGraph.InputAdapter
{
    public interface ICameraManipulator
    {
        event Action RequestRedrawAction;
        void UpdateCamera(ICamera camera);
    }
}