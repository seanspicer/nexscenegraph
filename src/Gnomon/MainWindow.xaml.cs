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
    }
}