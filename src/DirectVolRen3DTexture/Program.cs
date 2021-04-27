using System;
using Examples.Common;
using Veldrid;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Logging;
using Veldrid.SceneGraph.Viewer;

namespace DirectVolRen3DTexture
{
    class Program
    {
        private static void Main(string[] args)
        {
            Bootstrapper.Configure();
            LogManager.SetLogger(Bootstrapper.LoggerFactory);

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