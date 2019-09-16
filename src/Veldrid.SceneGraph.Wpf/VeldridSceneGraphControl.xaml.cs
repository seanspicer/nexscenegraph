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
        private VeldridComponent _veldridControl;
        private Window _myWindow;
        
        public VeldridSceneGraphControl()
        {
            InitializeComponent();
        }

        private void On_UIReady(object sender, EventArgs e)
        {
            _app = Application.Current;
            _myWindow = _app.MainWindow;
            _veldridControl = new VeldridComponent();
            ControlHostElement.Child = _veldridControl;
        }
    }
}