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
using System.Dynamic;
using Veldrid;
using Veldrid.SceneGraph.InputAdapter;

namespace Veldrid.SceneGraph.Viewer
{
    public interface IEndFrameEvent
    {
        float FrameTime { get; }
    }
    
    public interface IResizedEvent
    {
        int Width { get; }
        int Height { get; }
    }
    
    public interface IViewer : IView
    {
        Platform PlatformType { get; }

        //IObservable<IEndFrameEvent> EndFrameEvents { get; }
        //IObservable<IResizedEvent> ResizeEvents { get; }

        uint Width { get; }
        uint Height { get; }
        void SetBackgroundColor(RgbaFloat color);
        void ViewAll();
        
        void Run();
        void Run(GraphicsBackend? preferredBackend);
        void SetCameraOrthographic();
        void SetCameraPerspective();
    }
}