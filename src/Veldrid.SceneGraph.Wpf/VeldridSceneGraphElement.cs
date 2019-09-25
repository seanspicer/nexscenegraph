using System;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Input;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Viewer;
using Veldrid.SceneGraph.Wpf.Element;
using Veldrid.Utilities;

namespace Veldrid.SceneGraph.Wpf
{
    public class VeldridSceneGraphElement : DXElement
    {
        private ISubject<IGroup> _sceneDataSubject;
        private ISubject<ICameraManipulator> _cameraManipulatorSubject;
        private ISubject<IInputEventHandler> _eventHandlerSubject;

        private VeldridSceneGraphRenderer _vsgRenderer;
        
        public VeldridSceneGraphElement()
        {
            _sceneDataSubject = new ReplaySubject<IGroup>();
            _cameraManipulatorSubject = new ReplaySubject<ICameraManipulator>();
            _eventHandlerSubject = new ReplaySubject<IInputEventHandler>();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            
            _vsgRenderer = new VeldridSceneGraphRenderer();
            _sceneDataSubject.Subscribe((sceneData) =>
            {
                _vsgRenderer.SceneData = sceneData;
            });
            _cameraManipulatorSubject.Subscribe((cameraManipulator) =>
            {
                _vsgRenderer.CameraManipulator = cameraManipulator;
            });
            _eventHandlerSubject.Subscribe((eventHandler) =>
            {
                _vsgRenderer.EventHandler = eventHandler;
            });

            Renderer = _vsgRenderer;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
        }
    }
}