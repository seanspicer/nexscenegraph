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
using Veldrid;
using Veldrid.SceneGraph.InputAdapter;

namespace Gnomon
{
    public class ViewMatrixEventHandler : InputEventHandler
    {
        
        private readonly SceneViewModel _viewModel;
        
        public ViewMatrixEventHandler(SceneViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public override void HandleInput(IInputStateSnapshot snapshot, IUiActionAdapter uiActionAdapter)
        {
            base.HandleInput(snapshot, uiActionAdapter);
            _viewModel.MainViewMatrix = snapshot.ViewMatrix;
            
            foreach (var keyEvent in snapshot.KeyEvents)
            {
                if (keyEvent.Down)
                {
                    switch (keyEvent.Key)
                    {
                        case Key.X:
                            var up = new Vector3(0.0f, 0.0f, 1.0f);
                            var t = _viewModel.SceneRoot.GetBound();
                            var angle = 90.0f * 0.01745329252f; // 90 degrees in radians
                            var eye = new Vector3(0, t.Radius+10.0f, 0);
                            var rotation = Matrix4x4.CreateRotationZ(angle);
                            eye = Vector3.Transform(eye, rotation);
                            
                            if (!(_viewModel.CameraManipulator is StandardManipulator cameraManipulator)) return;
                            cameraManipulator.SetTransformation(eye, t.Center, up);
                            // MainViewMatrix = MainCameraViewMatrix;
                            break;
                    }
                }
            }
        }
    }
}