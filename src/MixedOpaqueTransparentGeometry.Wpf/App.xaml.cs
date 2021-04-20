using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Veldrid.SceneGraph;

namespace MixedOpaqueTransparentGeometry.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            if (Environment.GetEnvironmentVariable("VELDRID_SCENE_GRAPH_ENABLE_RENDERDOC_CAPTURE") != null)
            {
                var renderDocCapturePath =
                    Environment.GetEnvironmentVariable("VELDRID_SCENE_GRAPH_RENDERDOC_CAPTURE_PATH");
                
                RenderDocManager.Initialize(renderDocCapturePath);
            }
        }
    }
}