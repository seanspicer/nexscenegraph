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
using System.Linq;
using System.Numerics;
using System.Reactive.Subjects;
using System.Xml.Serialization;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Viewer
{
    public interface IView : IUiActionAdapter, Veldrid.SceneGraph.IView
    {
        INode SceneData { get; }
        ICameraManipulator CameraManipulator { get; }
        ICamera Camera { get; }
        IObservable<IUiEventAdapter> InputEvents { get; set; }

        SceneContext SceneContext { get; set; }
        
        void AddInputEventHandler(IUiEventHandler handler);

        void SetSceneData(INode node);
        
        void SetCameraManipulator(ICameraManipulator manipulator, bool resetPosition=true);

        bool ComputeIntersections(
            IUiEventAdapter eventAdapter,
            ref SortedMultiSet<ILineSegmentIntersector.IIntersection> intersections, 
            uint traversalMask);
    }
    
    public class View : Veldrid.SceneGraph.View, IView
    {
        private Scene _scene;

        public INode SceneData => _scene.SceneData;

        private Renderer Renderer { get; set; }
        
        public SceneContext SceneContext
        {
            get => Renderer.SceneContext;
            set => Renderer.SceneContext = value;
        }
        
        public IObservable<IUiEventAdapter> InputEvents { get; set; }

        private ICameraManipulator _cameraManipulator = null;
        public ICameraManipulator CameraManipulator
        {
            get => _cameraManipulator;
        }

        public override void SetCamera(ICamera camera)
        {
            camera.SetRenderer(CreateRenderer(camera));
            base.SetCamera(camera);
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
                AddInputEventHandler(_cameraManipulator);
                
                // TODO - need to set a coordinate frame callback
                if (null != SceneData)
                {
                    _cameraManipulator.SetNode(SceneData);
                }

                if (resetPosition)
                {
                    _cameraManipulator.Home(this);
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
                CameraManipulator.Home(this);
            }

            if (null != Camera)
            {
                // TODO here is where we will tell the Renderer that all the graphics device
                // objects need to be updated
            }
        }

        public static IView Create(uint width, uint height, float distance)
        {
            return new View(width, height, distance);
        }
        
        protected View(uint width, uint height, float distance) : base(width, height, distance)
        {
            _scene = new Scene();
            
            Camera.SetRenderer(CreateRenderer(Camera));
        }

        private IGraphicsDeviceOperation CreateRenderer(ICamera camera)
        {
            SceneContext currentSceneContext = null;
            if (null != Renderer)
            {
                currentSceneContext = Renderer.SceneContext;
            }
            
            Renderer = new Renderer(camera);
            Renderer.SceneContext = currentSceneContext;
            return Renderer;
        }
        
        public void AddInputEventHandler(IUiEventHandler handler)
        {
            InputEvents.Subscribe(x =>
            {
                handler.Handle(x, this);
            });
        }

        public virtual void RequestRedraw()
        {
        }

        public void RequestContinuousUpdate(bool flag)
        {
        }

        public void RequestWarpPointer(float x, float y)
        {
        }

        public bool ComputeIntersections(
            IUiEventAdapter eventAdapter,
            ref SortedMultiSet<ILineSegmentIntersector.IIntersection> intersections,
            uint traversalMask)
        {
            if (eventAdapter.PointerDataList.Count > 0)
            {
                var pd = eventAdapter.PointerDataList.Last();
                if (pd.Object is ICamera camera)
                {
                    return ComputeIntersections(camera,
                        IIntersector.CoordinateFrameMode.Projection,
                        pd.GetXNormalized(),
                        pd.GetYNormalized(),
                        ref intersections, traversalMask);
                }
            }
            intersections = null;
            return false;
        }

        public bool ComputeIntersections(
            ICamera camera, 
            IIntersector.CoordinateFrameMode cf, 
            float x, 
            float y,
            ref SortedMultiSet<ILineSegmentIntersector.IIntersection> intersections,
            uint traversalMask)
        {
            if (null == camera) return false;

            var picker = LineSegmentIntersector.Create(cf, x, y);
            var intersectionVisitor = IntersectionVisitor.Create(picker);
            intersectionVisitor.TraversalMask = traversalMask;
            
            camera.Accept(intersectionVisitor);
            
            if(picker.Intersections.Any())
            {
                intersections = picker.Intersections;
                return true;
            }
            
            intersections.Clear();
            return false;
            
        }
    }
}