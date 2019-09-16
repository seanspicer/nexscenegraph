using System;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Viewer;

namespace Veldrid.SceneGraph.Wpf
{
    public class View : Veldrid.SceneGraph.View, IView
    {
        public IGroup SceneData { get; set; }

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
            Camera.Renderer = new Renderer(Camera);
            resizeEvents.Subscribe(Camera.HandleResizeEvent);
        }

        public void AddInputEventHandler(IInputEventHandler handler)
        {
            InputEvents.Subscribe(handler.HandleInput);
        }
    }
}