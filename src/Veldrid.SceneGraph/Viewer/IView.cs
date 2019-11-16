﻿//
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
using Veldrid.SceneGraph.InputAdapter;

namespace Veldrid.SceneGraph.Viewer
{
    public interface IView
    {
        
        IGroup SceneData { get; set; }
        ICameraManipulator CameraManipulator { get; }
        ICamera Camera { get; set; }
        IObservable<IInputStateSnapshot> InputEvents { get; set; }
        
        [Obsolete("this is should not be used anywhere")]
        Framebuffer Framebuffer { get; set; }
        
        SceneContext SceneContext { get; set; }
        
        void AddInputEventHandler(IInputEventHandler handler);

        void SetSceneData(IGroup root);
        
        void SetCameraManipulator(ICameraManipulator manipulator, bool resetPosition=true);
    }
}