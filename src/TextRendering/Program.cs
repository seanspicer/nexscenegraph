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
using System.ComponentModel.DataAnnotations;
using Examples.Common;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Text;
using Veldrid.SceneGraph.Viewer;

namespace TextRendering
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper.Configure();
            
            var viewer = SimpleViewer.Create("Text Rendering Demo");
            viewer.SetCameraManipulator(TrackballManipulator.Create());

            var root = Group.Create();

            var textNode = TextNode.Create("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor");
            
            var geode = Geode.Create();
            geode.AddDrawable(textNode);
            
            root.AddChild(geode);

            viewer.SetSceneData(root);

            viewer.ViewAll();            
            viewer.Run();
        }
    }
}