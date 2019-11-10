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
    public class VeldridSceneGraphControl : DXElement
    {
        private ISubject<IGroup> _sceneDataSubject;
        private ISubject<ICameraManipulator> _cameraManipulatorSubject;
        private ISubject<IInputEventHandler> _eventHandlerSubject;
        private ISubject<RgbaFloat> _clearColorSubject;
        private ISubject<TextureSampleCount> _fsaaCountSubject;

        private VeldridSceneGraphRenderer _vsgRenderer;
        
        private WpfInputStateSnapshot _inputState;
        private ModifierKeys _modifierKeys = ModifierKeys.None;

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
        
        private bool ShouldHandleKeyEvents { get; set; }
        
        public VeldridSceneGraphControl()
        {
            _sceneDataSubject = new ReplaySubject<IGroup>();
            _cameraManipulatorSubject = new ReplaySubject<ICameraManipulator>();
            _eventHandlerSubject = new ReplaySubject<IInputEventHandler>();
            _clearColorSubject = new ReplaySubject<RgbaFloat>();
            _fsaaCountSubject = new ReplaySubject<TextureSampleCount>();
            _inputState = new WpfInputStateSnapshot();

            ShouldHandleKeyEvents = false;
            
            Loaded += OnLoaded;
        }
        

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Must be set when loaded - seems like this doesn't propagate if set on construction.
            Focusable = true;
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
            _clearColorSubject.Subscribe((clearColor) =>
            {
                _vsgRenderer.ClearColor = clearColor;
            });
            _fsaaCountSubject.Subscribe((fsaaCout) =>
            {
                _vsgRenderer.FsaaCount = fsaaCout;
            });

            Renderer = _vsgRenderer;
            _vsgRenderer.FrameInfo.Subscribe((frameInfo) => { this.FrameInfo = $"FPS: {frameInfo.ToString("#.0")}"; });
            _vsgRenderer.DpiScale = GetDpiScale();
        }

        public ICamera GetCamera()
        {
            return _vsgRenderer.View.Camera;
        }
        
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            Focus();
            ShouldHandleKeyEvents = true;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            ShouldHandleKeyEvents = false;
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
        
        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this);
            _inputState.MousePosition = new Vector2((float)pos.X, (float)pos.Y);
            
            var mouseEvent = new MouseEvent(MouseButton.Right, true);
            _inputState.MouseDown[(int) MouseButton.Right] = true;
            _inputState.MouseEventList.Add(mouseEvent);
            ProcessEvents();
        }
        
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this);
            _inputState.MousePosition = new Vector2((float)pos.X, (float)pos.Y);

            var mouseEvent = new MouseEvent(MouseButton.Right, false);
            _inputState.MouseDown[(int) MouseButton.Right] = false;
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
        
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!ShouldHandleKeyEvents) return;
            
            var key = e.Key;
            if (key == System.Windows.Input.Key.System)
            {
                key = e.SystemKey;
            }

            switch (key)
            {
                case System.Windows.Input.Key.LeftShift:
                case System.Windows.Input.Key.RightShift:
                    _modifierKeys |= ModifierKeys.Shift;
                    break;
                case System.Windows.Input.Key.LeftCtrl:
                case System.Windows.Input.Key.RightCtrl:
                    _modifierKeys |= ModifierKeys.Control;
                    break;
                case System.Windows.Input.Key.LeftAlt:
                case System.Windows.Input.Key.RightAlt:
                    _modifierKeys |= ModifierKeys.Alt;
                    break;
                default:
                {
                    var keyEvent = new KeyEvent(MapKey(e), e.IsDown, _modifierKeys);
                    _inputState.KeyEventList.Add(keyEvent);
                    ProcessEvents();
                    break;
                }
            }
        }
        
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (!ShouldHandleKeyEvents) return;
            
            base.OnKeyUp(e);

            var key = e.Key;
            if (key == System.Windows.Input.Key.System)
            {
                key = e.SystemKey;
            }
            
            switch (key)
            {
                case System.Windows.Input.Key.LeftShift:
                case System.Windows.Input.Key.RightShift:
                    _modifierKeys &= ~ModifierKeys.Shift;
                    break;
                case System.Windows.Input.Key.LeftCtrl:
                case System.Windows.Input.Key.RightCtrl:
                    _modifierKeys &= ~ModifierKeys.Control;
                    break;
                case System.Windows.Input.Key.LeftAlt:
                case System.Windows.Input.Key.RightAlt:
                    _modifierKeys &= ~ModifierKeys.Alt;
                    break;
                default:
                {
                    var keyEvent = new KeyEvent(MapKey(e), e.IsDown, _modifierKeys);
                    _inputState.KeyEventList.Add(keyEvent);
                    ProcessEvents();
                    break;
                }
            }
        }

        private Key MapKey(KeyEventArgs e)
        {
            // Convert A - Z
            if ((int) e.Key >= 44 && (int) e.Key <= 69)
            {
                return (Key) ((int) e.Key + 39);
            }
            // Convert 0 - 9
            else if ((int) e.Key >= 34 && (int) e.Key <= 43)
            {
                return (Key) ((int) e.Key + 75);
            }
            // Convert F1 - F12
            else if ((int) e.Key >= 90 && (int) e.Key <= 101)
            {
                return (Key) ((int) e.Key - 80);
            }
            
            return Key.Unknown;
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

            if (false == IsReallyLoopRendering)
            {
                Render(); // Event processing needs to trigger render for remote desktop to work...this is a bit like render-on-demand
            }
            
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
            DependencyProperty.Register("SceneRoot", typeof(IGroup), typeof(VeldridSceneGraphControl), 
                new PropertyMetadata(Group.Create(), new PropertyChangedCallback(OnSetSceneRootChanged)));  
        public IGroup SceneRoot 
        {
            get => (IGroup) GetValue(SceneRootProperty);
            set => SetValue(SceneRootProperty, value);
        }
        
        private static void OnSetSceneRootChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {  
            var vsgControl = d as VeldridSceneGraphControl;  
            vsgControl?.SetSceneRoot(e);  
        }  
        private void SetSceneRoot(DependencyPropertyChangedEventArgs e) {  
            _sceneDataSubject.OnNext((IGroup) e.NewValue); 
        }
        
        #endregion
        
        #region CameraManipulatorProperty
        
        public static readonly DependencyProperty CameraManipulatorProperty = 
            DependencyProperty.Register("CameraManipulator", typeof(ICameraManipulator), typeof(VeldridSceneGraphControl), 
                new PropertyMetadata(TrackballManipulator.Create(), new PropertyChangedCallback(OnCameraManipulatorChanged)));  

        public ICameraManipulator CameraManipulator 
        {
            get => (ICameraManipulator) GetValue(CameraManipulatorProperty);
            set => SetValue(CameraManipulatorProperty, value);
        }
        
        private static void OnCameraManipulatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {  
            var vsgControl = d as VeldridSceneGraphControl;  
            vsgControl?.SetCameraManipulator(e);  
        } 
        
        private void SetCameraManipulator(DependencyPropertyChangedEventArgs e) {  
            _cameraManipulatorSubject.OnNext((ICameraManipulator) e.NewValue); 
        }
        
        #endregion
        
        #region EventHandlerProperty
        public static readonly DependencyProperty EventHandlerProperty = 
            DependencyProperty.Register("EventHandler", typeof(IInputEventHandler), typeof(VeldridSceneGraphControl), 
                new PropertyMetadata(null, new PropertyChangedCallback(OnEventHandlerChanged)));  

        public IInputEventHandler EventHandler 
        {
            get => (IInputEventHandler) GetValue(EventHandlerProperty);
            set => SetValue(EventHandlerProperty, value);
        }
        
        private static void OnEventHandlerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {  
            var vsgControl = d as VeldridSceneGraphControl;  
            vsgControl?.SetEventHandler(e);  
        } 
        
        private void SetEventHandler(DependencyPropertyChangedEventArgs e) {  
            _eventHandlerSubject.OnNext((IInputEventHandler) e.NewValue); 
        }
        #endregion
        
        #region ClearColorProperty
        public static readonly DependencyProperty ClearColorProperty = 
            DependencyProperty.Register("ClearColor", typeof(RgbaFloat), typeof(VeldridSceneGraphControl), 
                new PropertyMetadata(RgbaFloat.Grey, new PropertyChangedCallback(OnClearColorChanged)));  

        public RgbaFloat ClearColor 
        {
            get => (RgbaFloat) GetValue(ClearColorProperty);
            set => SetValue(ClearColorProperty, value);
        }
        
        private static void OnClearColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {  
            var vsgControl = d as VeldridSceneGraphControl;  
            vsgControl?.SetClearColor(e);  
        } 
        
        private void SetClearColor(DependencyPropertyChangedEventArgs e) {  
            _clearColorSubject.OnNext((RgbaFloat) e.NewValue); 
        }
        #endregion
        
        #region FSAAProperty
        public static readonly DependencyProperty FsaaCountProperty = 
            DependencyProperty.Register("FsaaCount", typeof(TextureSampleCount), typeof(VeldridSceneGraphControl), 
                new PropertyMetadata(TextureSampleCount.Count1, new PropertyChangedCallback(OnFsaaCountChanged)));  

        public TextureSampleCount FsaaCount 
        {
            get => (TextureSampleCount) GetValue(ClearColorProperty);
            set => SetValue(FsaaCountProperty, value);
        }
        
        private static void OnFsaaCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {  
            var vsgControl = d as VeldridSceneGraphControl;  
            vsgControl?.SetFsaaCount(e);  
        } 
        
        private void SetFsaaCount(DependencyPropertyChangedEventArgs e) {  
            _fsaaCountSubject.OnNext((TextureSampleCount) e.NewValue); 
        }
        #endregion
        
    }
}