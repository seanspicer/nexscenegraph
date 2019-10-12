using System;
using Examples.Common;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Viewer;

namespace PathExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper.Configure();

            var logger = Veldrid.SceneGraph.Logging.LogManager.CreateLogger<Program>();
            
            var viewer = SimpleViewer.Create("Path Shape Example");
            viewer.SetCameraManipulator(TrackballManipulator.Create());

            // Build the path scene
            var root = PathExampleScene.Build();
            
            viewer.SetSceneData(root);
            viewer.ViewAll();            
            viewer.Run();
        }
    }
}