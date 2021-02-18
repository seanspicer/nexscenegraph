using System;
using Examples.Common;
using Veldrid;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Viewer;

namespace DraggersExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper.Configure();
            Veldrid.SceneGraph.Logging.LogManager.SetLogger(Bootstrapper.LoggerFactory);

            var viewer = SimpleViewer.Create("Draggers Example", TextureSampleCount.Count8);
            viewer.SetCameraManipulator(TrackballManipulator.Create());

            var root = DraggersExampleScene.Build();

            viewer.SetSceneData(root);
            viewer.ViewAll();            
            viewer.Run();
        }
    }
}