using Examples.Common;
using Veldrid;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Logging;
using Veldrid.SceneGraph.Viewer;

namespace DraggersExample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Bootstrapper.Configure();
            LogManager.SetLogger(Bootstrapper.LoggerFactory);

            var viewer = SimpleViewer.Create("Draggers Example", TextureSampleCount.Count8);
            viewer.SetCameraManipulator(TrackballManipulator.Create());

            var root = DraggersExampleScene.Build();

            viewer.SetSceneData(root);
            viewer.ViewAll();
            viewer.Run();
        }
    }
}