//
// Copyright 2018-2019 Sean Spicer 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Examples.Common;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using SixLabors.ImageSharp;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Viewer;
using Veldrid.SceneGraph.IO;
using Veldrid.SceneGraph.PipelineStates;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;

namespace Lighting
{
    
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper.Configure();
            Veldrid.SceneGraph.Logging.LogManager.SetLogger(Bootstrapper.LoggerFactory);

            var viewer = SimpleViewer.Create("Phong Shaded Dragon Scene Graph", TextureSampleCount.Count8);
            //viewer.SetCameraOrthographic();
            viewer.SetCameraManipulator(TrackballManipulator.Create());

            var root = LightingExampleScene.Build();

            viewer.SetSceneData(root);
            viewer.ViewAll();            
            viewer.Run();
        }
    }
}