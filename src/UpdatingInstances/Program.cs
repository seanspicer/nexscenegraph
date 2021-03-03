using Examples.Common;
using Veldrid;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Logging;
using Veldrid.SceneGraph.Viewer;

namespace UpdatingInstances
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Bootstrapper.Configure();
            LogManager.SetLogger(Bootstrapper.LoggerFactory);

            var viewer = SimpleViewer.Create("Updating Instances Example Scene", TextureSampleCount.Count8);
            //viewer.SetCameraOrthographic();
            viewer.SetCameraManipulator(TrackballManipulator.Create());

            var root = UpdatingInstancesExampleScene.Build();

            viewer.SetSceneData(root);
            viewer.ViewAll();
            viewer.Run();
        }
    }
}