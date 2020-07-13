using System;
using Examples.Common;
using Veldrid;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Viewer;

namespace UpdatingInstances
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper.Configure();
            Veldrid.SceneGraph.Logging.LogManager.SetLogger(Bootstrapper.LoggerFactory);

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