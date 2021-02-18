﻿using System;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using Veldrid.SceneGraph.InputAdapter;

namespace Veldrid.SceneGraph.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class VeldridSceneGraphHwndControl : UserControl
    {
        private Application _app;
        private VeldridSceneGraphComponent _veldridSceneGraphComponent;
        private Window _myWindow;

        private ISubject<IGroup> _sceneDataSubject;
        private ISubject<ICameraManipulator> _cameraManipulatorSubject;
        private ISubject<IUiEventHandler> _eventHandlerSubject;
        
        public VeldridSceneGraphHwndControl()
        {
            _sceneDataSubject = new ReplaySubject<IGroup>();
            _cameraManipulatorSubject = new ReplaySubject<ICameraManipulator>();
            _eventHandlerSubject = new ReplaySubject<IUiEventHandler>();
            InitializeComponent();
        }
        
        private void On_UIReady(object sender, EventArgs e)
        {
            _app = Application.Current;
            _myWindow = _app.MainWindow;
            _veldridSceneGraphComponent = new VeldridSceneGraphComponent();
            _sceneDataSubject.Subscribe((sceneData) => { _veldridSceneGraphComponent.SceneData = sceneData; });
            _cameraManipulatorSubject.Subscribe((cameraManipulator) =>
            {
                _veldridSceneGraphComponent.CameraManipulator = cameraManipulator;
            });
            _eventHandlerSubject.Subscribe((eventHandler) =>
            {
                _veldridSceneGraphComponent.EventHandler = eventHandler;
            });
            ControlHostElement.Child = _veldridSceneGraphComponent;
        }
        
        #region SceneRoot Property
        
        public static readonly DependencyProperty sceneRootProperty = 
            DependencyProperty.Register("SceneRoot", typeof(IGroup), typeof(VeldridSceneGraphHwndControl), 
                new PropertyMetadata(Group.Create(), new PropertyChangedCallback(OnSetSceneRootChanged)));  
        public IGroup SceneRoot 
        {
            get => (IGroup) GetValue(sceneRootProperty);
            set => SetValue(sceneRootProperty, value);
        }
        
        private static void OnSetSceneRootChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {  
            VeldridSceneGraphHwndControl vsgHwndControl = d as VeldridSceneGraphHwndControl;  
            vsgHwndControl.SetSceneRoot(e);  
        }  
        private void SetSceneRoot(DependencyPropertyChangedEventArgs e) {  
            _sceneDataSubject.OnNext((IGroup) e.NewValue); 
        }
        
        #endregion
        
        #region CameraManipulatorProperty
        
        public static readonly DependencyProperty cameraManipulatorProperty = 
            DependencyProperty.Register("CameraManipulator", typeof(ICameraManipulator), typeof(VeldridSceneGraphHwndControl), 
                new PropertyMetadata(TrackballManipulator.Create(), new PropertyChangedCallback(OnCameraManipulatorChanged)));  

        public ICameraManipulator CameraManipulator 
        {
            get => (ICameraManipulator) GetValue(cameraManipulatorProperty);
            set => SetValue(cameraManipulatorProperty, value);
        }
        
        private static void OnCameraManipulatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {  
            VeldridSceneGraphHwndControl vsgHwndControl = d as VeldridSceneGraphHwndControl;  
            vsgHwndControl.SetCameraManipulator(e);  
        } 
        
        private void SetCameraManipulator(DependencyPropertyChangedEventArgs e) {  
            _cameraManipulatorSubject.OnNext((ICameraManipulator) e.NewValue); 
        }
        
        #endregion
        
        #region EventHandlerProperty
        public static readonly DependencyProperty eventHandlerProperty = 
            DependencyProperty.Register("EventHandler", typeof(IUiEventHandler), typeof(VeldridSceneGraphHwndControl), 
                new PropertyMetadata(null, new PropertyChangedCallback(OnEventHandlerChanged)));  

        public IUiEventHandler EventHandler 
        {
            get => (IUiEventHandler) GetValue(eventHandlerProperty);
            set => SetValue(eventHandlerProperty, value);
        }
        
        private static void OnEventHandlerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {  
            VeldridSceneGraphHwndControl vsgHwndControl = d as VeldridSceneGraphHwndControl;  
            vsgHwndControl.SetEventHandler(e);  
        } 
        
        private void SetEventHandler(DependencyPropertyChangedEventArgs e) {  
            _eventHandlerSubject.OnNext((IUiEventHandler) e.NewValue); 
        }
        #endregion
    }
}