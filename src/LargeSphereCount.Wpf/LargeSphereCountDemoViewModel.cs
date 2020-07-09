using Examples.Common.Wpf;
using Veldrid;
using Veldrid.SceneGraph.InputAdapter;

namespace LargeSphereCount.Wpf
{
    public class LargeSphereCountDemoViewModel : ViewModelBase
    {
        public LargeSphereCountDemoViewModel() : base()
        {
            SceneRoot = Examples.Common.LargeSphereCountScene.Build();
            CameraManipulator = TrackballManipulator.Create();
            FsaaCount = TextureSampleCount.Count8; // 8x FSAA
        }
    }
}