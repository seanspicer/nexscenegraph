//
// Copyright 2018-2021 Sean Spicer 
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

using System.Collections.Generic;
using System.Numerics;
using Examples.Common;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Viewer;

namespace ColoredCube
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Bootstrapper.Configure();

            var viewer = SimpleViewer.Create("Colored Cube Scene Graph");
            viewer.SetCameraOrthographic();

            viewer.SetCameraManipulator(TrackballManipulator.Create());

            var root = ColoredCubeExampleScene.Build();
            viewer.SetSceneData(root);

            viewer.ViewAll();

            viewer.Run();
        }
    }
}