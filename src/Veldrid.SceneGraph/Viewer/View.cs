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
    public interface IView
    {
        INode SceneData { get; }
        ICameraManipulator CameraManipulator { get; }
        ICamera Camera { get; set; }
        IObservable<IInputStateSnapshot> InputEvents { get; set; }
        
        [Obsolete("this is should not be used anywhere")]
        Framebuffer Framebuffer { get; set; }
        
        SceneContext SceneContext { get; set; }
        
        void AddInputEventHandler(IInputEventHandler handler);

        void SetSceneData(INode node);
        
        void SetCameraManipulator(ICameraManipulator manipulator, bool resetPosition=true);
    }
    
    public class View : Veldrid.SceneGraph.View, IUiActionAdapter, IView
    {
        private Scene _scene;

        public INode SceneData => _scene.SceneData;

        private Renderer Renderer { get; set; }
        
        public  Framebuffer Framebuffer 
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
        
        public void SetSceneData(INode node)
        {
            if (node == _scene.SceneData) return;

            var scene = Scene.GetScene(node);

            if (null != scene)
            {
                System.Diagnostics.Trace.WriteLine($"View.SetSceneData() Sharing scene {scene.GetHashCode()}"); 
                _scene = scene;
            }
            else
            {
                _scene = new Scene();
                _scene.SetSceneData(node);
            }
            
            
            // TODO Assign scene to cameras
            AssignSceneDataToCameras();
        }

        private void AssignSceneDataToCameras()
        {
            var sceneData = _scene.SceneData;

            if (null != CameraManipulator)
            {
                CameraManipulator.SetNode(sceneData);
                CameraManipulator.Home(InputStateSnapshot.CreateEmpty(), this);
            }

            if (null != Camera)
            {
                // TODO here is where we will tell the Renderer that all the graphics device
                // objects need to be updated
            }
        }

        protected View()
        {
            _scene = new Scene();
            
            Camera.SetRenderer(CreateRenderer(Camera));
        }

        private IGraphicsDeviceOperation CreateRenderer(ICamera camera)
        {
            Renderer = new Renderer(camera);
            return Renderer;
        }
        
        public void AddInputEventHandler(IInputEventHandler handler)
        {
            InputEvents.Subscribe(x =>
            {
                handler.HandleInput(x, this);
            });
        }

        public void RequestRedraw()
        {
            //throw new NotImplementedException();
        }

        public void RequestContinuousUpdate(bool flag)
        {
            //throw new NotImplementedException();
        }
        
    }
}