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

using Examples.Common;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Viewer;

namespace PathExample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Bootstrapper.Configure();

            //var logger = Veldrid.SceneGraph.Logging.LogManager.CreateLogger<Program>();

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