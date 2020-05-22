using System;
using System.Numerics;

namespace Gnomon
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
            DataContext = new SceneViewModel();
        }
        private void window_Activated(object sender, EventArgs e)
        {
            var vm = DataContext as SceneViewModel;
            // vm.PropertyChanged += new PropertyChangedEventHandler(VmPropertyHandler);
            var camera = VsgElementGnomon.GetCamera();
            _gnomonStepBack = Matrix4x4.CreateTranslation(0, 0, -10.0f);
            camera.SetViewMatrix(_gnomonStepBack);
        }
    }
}