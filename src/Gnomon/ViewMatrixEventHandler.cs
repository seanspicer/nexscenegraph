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
            _viewModel.MainViewMatrix = snapshot.ViewMatrix;
        }
    }
}