using System;
using Examples.Common;
using Veldrid;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Viewer;

namespace DirectVolumeRendering
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper.Configure();
            Veldrid.SceneGraph.Logging.LogManager.SetLogger(Bootstrapper.LoggerFactory);

            var viewer = SimpleViewer.Create("Direct Volume Rendering", TextureSampleCount.Count8);
            viewer.SetBackgroundColor(RgbaFloat.Black);
            viewer.SetCameraManipulator(TrackballManipulator.Create());

            var root = SampledVolumeRenderingExampleScene.Build();

            viewer.SetSceneData(root);
            viewer.ViewAll();            
            viewer.Run();
        }
    }
}