using System;
using System.Windows;
using System.Windows.Controls;

namespace Veldrid.SceneGraph.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class VeldridSceneGraphControl : UserControl
    {
        private Application _app;
        private VeldridSceneGraphComponent _veldridSceneGraphControl;
        private Window _myWindow;
        
        public VeldridSceneGraphControl()
        {
            InitializeComponent();
        }

        private void On_UIReady(object sender, EventArgs e)
        {
            _app = Application.Current;
            _myWindow = _app.MainWindow;
            _veldridSceneGraphControl = new VeldridSceneGraphComponent();
            ControlHostElement.Child = _veldridSceneGraphControl;
        }
    }
}