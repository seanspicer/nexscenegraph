using System;
using System.Diagnostics;
using System.Numerics;
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
        
        private WpfInputStateSnapshot _inputState;
        
        public VeldridSceneGraphElement()
        {
            _sceneDataSubject = new ReplaySubject<IGroup>();
            _cameraManipulatorSubject = new ReplaySubject<ICameraManipulator>();
            _eventHandlerSubject = new ReplaySubject<IInputEventHandler>();
            _inputState = new WpfInputStateSnapshot();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //_vsgRenderer.Initialize();
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
            _vsgRenderer.DpiScale = GetDpiScale();
        }
        

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            
            var pos = e.GetPosition(this);
            _inputState.MousePosition = new Vector2((float)pos.X, (float)pos.Y);
            
            var mouseEvent = new MouseEvent(MouseButton.Left, true);
            _inputState.MouseDown[(int) MouseButton.Left] = true;
            _inputState.MouseEventList.Add(mouseEvent);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            var pos = e.GetPosition(this);
            _inputState.MousePosition = new Vector2((float)pos.X, (float)pos.Y);
            
            var mouseEvent = new MouseEvent(MouseButton.Left, false);
            _inputState.MouseDown[(int) MouseButton.Left] = false;
            _inputState.MouseEventList.Add(mouseEvent);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            _inputState.MousePosition = new Vector2((float)pos.X, (float)pos.Y);
            base.OnMouseMove(e);
        }
        
        private void ProcessEvents()
        {
            double dpiScale = GetDpiScale();
            int width =  (ActualWidth < 0 ? 0 : (int)Math.Ceiling(ActualWidth * dpiScale));
            int height = (ActualHeight < 0 ? 0 : (int)Math.Ceiling(ActualHeight * dpiScale));

            var inputStateSnap = InputStateSnapshot.Create(_inputState, width, height);
            _vsgRenderer.HandleInput(inputStateSnap);
            _inputState.MouseEventList.Clear();
            _inputState.KeyEventList.Clear();
            _inputState.WheelDelta = 0;
        }
        
        private double GetDpiScale()
        {
            PresentationSource source = PresentationSource.FromVisual(this);

            if (source != null)
                if (source.CompositionTarget != null)
                    return source.CompositionTarget.TransformToDevice.M11;

            return 1.0d;
        }
    }
}