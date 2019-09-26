using System;
using System.Diagnostics;
using System.Numerics;
using System.Reactive.Subjects;
using System.Runtime.Serialization;
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

        private string _frameInfo = string.Empty;
        public string FrameInfo
        {
            get => _frameInfo;
            set
            {
                _frameInfo = value;
                OnPropertyChanged("FrameInfo");
            }
        }
        
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
            _vsgRenderer.FrameInfo.Subscribe((frameInfo) => { this.FrameInfo = $"FPS: {frameInfo.ToString("#.0")}"; });
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
            ProcessEvents();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            var pos = e.GetPosition(this);
            _inputState.MousePosition = new Vector2((float)pos.X, (float)pos.Y);
            
            var mouseEvent = new MouseEvent(MouseButton.Left, false);
            _inputState.MouseDown[(int) MouseButton.Left] = false;
            _inputState.MouseEventList.Add(mouseEvent);
            ProcessEvents();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var pos = e.GetPosition(this);
            _inputState.MousePosition = new Vector2((float)pos.X, (float)pos.Y);
            ProcessEvents();
            
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            _inputState.WheelDelta += e.Delta / 10;
            ProcessEvents();
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
        
        #region SceneRoot Property
        
        public static readonly DependencyProperty SceneRootProperty = 
            DependencyProperty.Register("SceneRoot", typeof(IGroup), typeof(VeldridSceneGraphElement), 
                new PropertyMetadata(Group.Create(), new PropertyChangedCallback(OnSetSceneRootChanged)));  
        public IGroup SceneRoot 
        {
            get => (IGroup) GetValue(SceneRootProperty);
            set => SetValue(SceneRootProperty, value);
        }
        
        private static void OnSetSceneRootChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {  
            var element = d as VeldridSceneGraphElement;  
            element?.SetSceneRoot(e);  
        }  
        private void SetSceneRoot(DependencyPropertyChangedEventArgs e) {  
            _sceneDataSubject.OnNext((IGroup) e.NewValue); 
        }
        
        #endregion
        
        #region CameraManipulatorProperty
        
        public static readonly DependencyProperty CameraManipulatorProperty = 
            DependencyProperty.Register("CameraManipulator", typeof(ICameraManipulator), typeof(VeldridSceneGraphElement), 
                new PropertyMetadata(TrackballManipulator.Create(), new PropertyChangedCallback(OnCameraManipulatorChanged)));  

        public ICameraManipulator CameraManipulator 
        {
            get => (ICameraManipulator) GetValue(CameraManipulatorProperty);
            set => SetValue(CameraManipulatorProperty, value);
        }
        
        private static void OnCameraManipulatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {  
            var element = d as VeldridSceneGraphElement;  
            element?.SetCameraManipulator(e);  
        } 
        
        private void SetCameraManipulator(DependencyPropertyChangedEventArgs e) {  
            _cameraManipulatorSubject.OnNext((ICameraManipulator) e.NewValue); 
        }
        
        #endregion
        
        #region EventHandlerProperty
        public static readonly DependencyProperty EventHandlerProperty = 
            DependencyProperty.Register("EventHandler", typeof(IInputEventHandler), typeof(VeldridSceneGraphElement), 
                new PropertyMetadata(null, new PropertyChangedCallback(OnEventHandlerChanged)));  

        public IInputEventHandler EventHandler 
        {
            get => (IInputEventHandler) GetValue(EventHandlerProperty);
            set => SetValue(EventHandlerProperty, value);
        }
        
        private static void OnEventHandlerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {  
            var element = d as VeldridSceneGraphElement;  
            element?.SetEventHandler(e);  
        } 
        
        private void SetEventHandler(DependencyPropertyChangedEventArgs e) {  
            _eventHandlerSubject.OnNext((IInputEventHandler) e.NewValue); 
        }
        #endregion
    }
}