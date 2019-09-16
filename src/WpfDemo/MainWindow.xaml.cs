using System;
using System.Windows;

namespace WpfDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Application _app;
        private IntPtr _hwndListBox;
        private int _itemCount;
        private VeldridComponent _veldridControl;
        private Window _myWindow;
        private int _selectedItem;
        
        public MainWindow()
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