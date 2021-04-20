using Examples.Common.Wpf;
using Veldrid;
using Veldrid.SceneGraph.InputAdapter;

namespace DirectVolumeRendering.Wpf
{
    public class DirectVolumeRenderingViewModel : ViewModelBase
    {
        public DirectVolumeRenderingViewModel() : base()
        {
            ClearColor = RgbaFloat.Black;
            SceneRoot = Examples.Common.SampledVolumeRenderingExampleScene.Build();
            CameraManipulator = TrackballManipulator.Create();
            
            FsaaCount = TextureSampleCount.Count8; // 8x FSAA
        }
    }
}