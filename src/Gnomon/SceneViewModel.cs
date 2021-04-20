

using System.Numerics;
using Examples.Common.Wpf;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.InputAdapter;

namespace Gnomon
{
    public class SceneViewModel : ViewModelBase
    {
        
        private IGroup _gnomonRoot;
        public IGroup GnomonRoot
        {
            get => _gnomonRoot;
            set
            {
                _gnomonRoot = value;
                OnPropertyChanged("GnomonRoot");
            }
        }

        private Matrix4x4 _mainViewMatrix;
        public Matrix4x4 MainViewMatrix
        {
            get => _mainViewMatrix;
            set 
            {
                _mainViewMatrix = value;
                OnPropertyChanged("MainViewMatrix");
            }
        }

        private RgbaFloat _gnomonClearColor;

        public RgbaFloat GnomonClearColor
        {
            get => _gnomonClearColor;
            set
            {
                _gnomonClearColor = value;
                OnPropertyChanged("GnomonClearColor");
            }
        }

        internal SceneViewModel()
        {
            SceneRoot = Examples.Common.PathExampleScene.Build();
            CameraManipulator = TrackballManipulator.Create();
            GnomonClearColor = RgbaFloat.Clear;
            FsaaCount = TextureSampleCount.Count16;
            
            GnomonRoot = Group.Create();
            var gnomon = BaseGnomon.Build();
            GnomonRoot.AddChild(gnomon);
            
            EventHandler = new ViewMatrixEventHandler(this);
        }
        
        public void ChangeCamera(IUiActionAdapter uiActionAdapter, ICamera camera)
        {
            var up = new Vector3(0.0f, 0.0f, 1.0f);
            var t = SceneRoot.GetBound();
            var angle = 90.0f * 0.01745329252f; // 90 degrees in radians
            var eye = new Vector3(0, t.Radius+10.0f, 0);
            var rotation = Matrix4x4.CreateRotationZ(angle);
            eye = Vector3.Transform(eye, rotation);
                            
            if (!(CameraManipulator is StandardManipulator cameraManipulator)) return;
            cameraManipulator.SetTransformation(eye, t.Center, up);

            MainViewMatrix = cameraManipulator.InverseMatrix;
        }
    }
}