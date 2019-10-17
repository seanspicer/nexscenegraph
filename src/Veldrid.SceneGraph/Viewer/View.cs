//
// Copyright 2018 Sean Spicer 
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
using System.Numerics;
using System.Reactive.Subjects;
using Veldrid.SceneGraph.InputAdapter;

namespace Veldrid.SceneGraph.Viewer
{
    public class View : Veldrid.SceneGraph.View, IView
    {
        public IGroup SceneData { get; set; }

        private Renderer Renderer { get; set; }
        
        public Framebuffer Framebuffer 
        { 
            get => Renderer.Framebuffer;
            set => Renderer.Framebuffer = value;
        }
        
        public IObservable<IInputStateSnapshot> InputEvents { get; set; }
        public IObservable<IResizedEvent> ResizeEvent { get; set; }

        private ICameraManipulator _cameraManipulator = null;
        public ICameraManipulator CameraManipulator
        {
            get => _cameraManipulator;
            set
            {
                if (null != _cameraManipulator)
                {
                    throw new Exception("Setting camera manipulator twice.  Don't do that.");
                }
                _cameraManipulator = value;
                _cameraManipulator.SetCamera(Camera);

                InputEvents.Subscribe(_cameraManipulator.HandleInput);
            }
        }

        public static IView Create(IObservable<IResizedEvent> resizeEvents)
        {
            return new View(resizeEvents);
        }
        
        protected View(IObservable<IResizedEvent> resizeEvents)
        {
            Renderer = new Renderer(Camera);
            Camera.Renderer = Renderer;
            resizeEvents.Subscribe(Camera.HandleResizeEvent);
        }

        public void AddInputEventHandler(IInputEventHandler handler)
        {
            InputEvents.Subscribe(handler.HandleInput);
        }
    }
}