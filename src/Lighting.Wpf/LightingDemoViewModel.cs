using Examples.Common.Wpf;
using SharpDX.Win32;
using Veldrid.SceneGraph.InputAdapter;

namespace Lighting.Wpf
{
    public class LightingDemoViewModel : ViewModelBase
    {
        public LightingDemoViewModel() : base()
        {
            var root = Examples.Common.LightingExampleScene.Build();

            SceneRoot = root;
            CameraManipulator = TrackballManipulator.Create();
        }
    }
}