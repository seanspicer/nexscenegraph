//
// This file is part of IMAGEFrac (R) and related technologies.
//
// Copyright (c) 2017-2020 Reveal Energy Services.  All Rights Reserved.
//
// LEGAL NOTICE:
// IMAGEFrac contains trade secrets and otherwise confidential information
// owned by Reveal Energy Services. Access to and use of this information is 
// strictly limited and controlled by the Company. This file may not be copied,
// distributed, or otherwise disclosed outside of the Company's facilities 
// except under appropriate precautions to maintain the confidentiality hereof, 
// and may not be used in any way not expressly authorized by the Company.
//

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
        
        internal SceneViewModel()
        {
            SceneRoot = Examples.Common.PathExampleScene.Build();
            CameraManipulator = TrackballManipulator.Create();
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