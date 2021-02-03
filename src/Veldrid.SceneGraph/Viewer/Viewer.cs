
using System;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Viewer
{
    
    public class Viewer : ViewerBase, IViewer
    {
        private IView View { get; set; }

        public INode SceneData { get; }
        public ICameraManipulator CameraManipulator { get; }
        public ICamera Camera => View?.Camera;
        public IObservable<IEvent> InputEvents { get; set; }
        public void EventTraversal()
        {
            throw new NotImplementedException();
        }

        public SceneContext SceneContext { get; set; }
        
        public void AddInputEventHandler(IEventHandler handler)
        {
            throw new NotImplementedException();
        }

        public void SetSceneData(INode node)
        {
            throw new NotImplementedException();
        }

        public void SetCameraManipulator(ICameraManipulator manipulator, bool resetPosition = true)
        {
            throw new NotImplementedException();
        }

        public bool ComputeIntersections(IUiEventAdapter eventAdapter, ref SortedMultiSet<ILineSegmentIntersector.IIntersection> intersections, uint traversalMask)
        {
            throw new NotImplementedException();
        }

        public static IViewer Create(string title, TextureSampleCount fsaaCount=TextureSampleCount.Count1, GraphicsBackend? preferredBackend = null)
        {
            return new Viewer(title, fsaaCount, preferredBackend);
        }

        protected Viewer(string title, TextureSampleCount fsaaCount, GraphicsBackend? preferredBackend)
        {
            
        }
        
        protected override ContextList GetContexts(bool onlyValid)
        {
            var contexts = new ContextList();
            if (Camera != null && (null != Camera.GraphicsContext || !onlyValid))
            {
                contexts.Add(Camera.GraphicsContext);
            }

            return contexts;

        }

        public void RequestRedraw()
        {
            throw new NotImplementedException();
        }

        public void RequestContinuousUpdate(bool flag)
        {
            throw new NotImplementedException();
        }

        public void RequestWarpPointer(float x, float y)
        {
            throw new NotImplementedException();
        }

        public void SetCamera(ICamera camera)
        {
            throw new NotImplementedException();
        }

        public Platform PlatformType { get; }
        public uint Width { get; }
        public uint Height { get; }
        public void SetBackgroundColor(RgbaFloat color)
        {
            throw new NotImplementedException();
        }

        public void ViewAll()
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
            throw new NotImplementedException();
        }

        public void SetCameraOrthographic()
        {
            throw new NotImplementedException();
        }

        public void SetCameraPerspective()
        {
            throw new NotImplementedException();
        }
    }
}