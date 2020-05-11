using System;
using Examples.Common;
using Veldrid;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Viewer;

namespace ExampleViewer
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO - add argument parsing
            
            Bootstrapper.Configure();
            Veldrid.SceneGraph.Logging.LogManager.SetLogger(Bootstrapper.LoggerFactory);

            var viewer = SimpleViewer.Create("ExampleViewer", TextureSampleCount.Count8);

            viewer.SetCameraManipulator(TrackballManipulator.Create());

            var root = AntiSquishExampleScene.Build();

            viewer.SetSceneData(root);
            viewer.ViewAll();            
            viewer.Run();
        }
    }
}