using Examples.Common.Wpf;
using Veldrid.SceneGraph.InputAdapter;

namespace PathExample.Wpf
{
    public class PathExampleViewModel : ViewModelBase
    {
        internal PathExampleViewModel() : base()
        {
            SceneRoot = Examples.Common.PathExampleScene.Build();
            CameraManipulator = TrackballManipulator.Create();
        }
    }
}