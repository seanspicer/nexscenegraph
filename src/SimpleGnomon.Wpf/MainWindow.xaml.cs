using System;
using System.ComponentModel;
using System.Numerics;

namespace SimpleGnomon.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Matrix4x4 _gnomonStepBack = Matrix4x4.CreateTranslation(0, 0, -10.0f);
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new GnomonSceneViewModel();
        }

        void window_Activated(object sender, EventArgs e)
        {
            var vm = DataContext as GnomonSceneViewModel;
            vm.PropertyChanged += new PropertyChangedEventHandler(VmPropertyHandler);
            var camera = VsgElementGnomon.GetCamera();
            _gnomonStepBack = Matrix4x4.CreateTranslation(0, 0, -10.0f);
            camera.SetViewMatrix(_gnomonStepBack);
        }

        private void VmPropertyHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MainViewMatrix")
            {
                var vm = DataContext as GnomonSceneViewModel;
                Matrix4x4 matrix;
                var ok = Matrix4x4.Decompose(vm.MainViewMatrix, out var scale, out var rotation, out var translation);
                if (ok)
                {
                    matrix = Matrix4x4.CreateFromQuaternion(rotation);
                }
                else
                {
                    matrix = Matrix4x4.CreateFromQuaternion(new Quaternion());
                }
                matrix *= _gnomonStepBack;
                var camera = VsgElementGnomon.GetCamera();
                camera.SetViewMatrix(matrix);
            }
        }
    }
}