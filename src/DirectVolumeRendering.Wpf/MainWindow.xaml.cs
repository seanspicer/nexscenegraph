
using System.Windows;

namespace DirectVolumeRendering.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new DirectVolumeRenderingViewModel();
        }
    }
}