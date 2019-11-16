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
using System.Numerics;
using System.Reactive.Subjects;
using Veldrid.SceneGraph.InputAdapter;

namespace Veldrid.SceneGraph.Viewer
{
    internal class UiActionAdapter : IUiActionAdapter
    {
        public void RequestRedraw()
        {
            throw new NotImplementedException();
        }

        public void RequestContinuousRedraw()
        {
            throw new NotImplementedException();
        }
    }
    
    public class View : Veldrid.SceneGraph.View, IUiActionAdapter, IView
    {
        public IGroup SceneData { get; set; }

        private Renderer Renderer { get; set; }
        
        
        public Framebuffer Framebuffer 
        { 
            get => Renderer.Framebuffer;
            set => Renderer.Framebuffer = value;
        }

        public SceneContext SceneContext
        {
            get => Renderer.SceneContext;
            set => Renderer.SceneContext = value;
        }
        
        public IObservable<IInputStateSnapshot> InputEvents { get; set; }
        public IObservable<IResizedEvent> ResizeEvent { get; set; }

        private ICameraManipulator _cameraManipulator = null;
        public ICameraManipulator CameraManipulator
        {
            get => _cameraManipulator;
        }

        public void SetCameraManipulator(ICameraManipulator manipulator, bool resetPosition)
        {
            // TODO this is probably temporary now.
            if (null != _cameraManipulator)
            {
                throw new Exception("Setting camera manipulator twice.  Don't do that.");
            }
            
            _cameraManipulator = manipulator;

            if (null != _cameraManipulator)
            {
                // TODO - need to set a coordinate frame callback
                if (null != SceneData)
                {
                    _cameraManipulator.SetNode(SceneData);
                }

                if (resetPosition)
                {
                    _cameraManipulator.Home(
                        InputStateSnapshot.CreateEmpty(
                            (int)DisplaySettings.Instance.ScreenWidth,
                            (int)DisplaySettings.Instance.ScreenHeight), 
                        this);
                }
            }
        }
        
        public void SetSceneData(IGroup root)
        {
            if (root == SceneData) return;
            
            SceneData = root;
            
            // TODO Assign scene to cameras
            AssignSceneDataToCameras();
        }

        private void AssignSceneDataToCameras()
        {
            throw new NotImplementedException();
        }

        protected View()
        {
            
        }
        
//        protected View(IObservable<IResizedEvent> resizeEvents)
//        {
//            Renderer = new Renderer(Camera);
//            Camera.Renderer = Renderer;
//            resizeEvents.Subscribe(Camera.HandleResizeEvent);
//        }

        public void AddInputEventHandler(IInputEventHandler handler)
        {
            InputEvents.Subscribe(x =>
            {
                handler.HandleInput(x, new UiActionAdapter());
            });
        }

        public void RequestRedraw()
        {
            throw new NotImplementedException();
        }

        public void RequestContinuousRedraw()
        {
            throw new NotImplementedException();
        }
    }
}