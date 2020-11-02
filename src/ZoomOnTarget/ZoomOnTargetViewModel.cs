using Examples.Common.Wpf;
using Veldrid;
using Veldrid.SceneGraph.InputAdapter;

namespace ZoomOnTarget
{
    public class ZoomOnTargetViewModel : ViewModelBase
    {
        public ZoomOnTargetViewModel()
        {
            SceneRoot = Examples.Common.PathExampleScene.Build();
            CameraManipulator = TrackballManipulator.Create();
            FsaaCount = TextureSampleCount.Count16;
            ClearColor = RgbaFloat.Blue;
            EventHandler = new PickEventHandler();

        }
    }
}