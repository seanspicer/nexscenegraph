using Examples.Common.Wpf;
using SharpDX.Win32;
using Veldrid.SceneGraph.InputAdapter;

namespace Lighting.Wpf
{
    public class LightingDemoViewModel : ViewModelBase
    {
        public LightingDemoViewModel() : base()
        {
            SceneRoot = Examples.Common.LightingExampleScene.Build();
            CameraManipulator = TrackballManipulator.Create();
        }
    }
}