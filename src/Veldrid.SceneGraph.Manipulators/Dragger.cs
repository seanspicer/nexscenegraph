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

using System;
using System.Numerics;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.PipelineStates;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IDragger : IMatrixTransform
    {
        void SetupDefaultGeometry();
        bool Handle(IInputStateSnapshot snapshot, IUiActionAdapter uiActionAdapter);
        bool Handle(IPointerInfo pointerInfo, IInputStateSnapshot snapshot, IUiActionAdapter uiActionAdapter);
    }
    
    public class Dragger : MatrixTransform, IDragger
    {
        protected bool HandleEvents { get; set; }
        protected bool DraggerActive { get; set; }
        
        protected uint ActivationModKeyMask { get; set; }
        protected uint ActivationMouseButtonMask { get; set; }
        protected int  ActivationKeyEvent { get; set; }
        protected bool ActivationPermittedByModKeyMask { get; set; }
        protected bool ActivationPermittedByMouseButtonMask { get; set; }
        protected bool ActivationPermittedByKeyEvent { get; set; }
        
        protected IDragger ParentDragger { get; set; }
        
        protected Dragger(Matrix4x4 matrix) : base(matrix)
        {
            
        }

        public virtual bool Handle(IPointerInfo pointerInfo, IInputStateSnapshot snapshot,
            IUiActionAdapter uiActionAdapter)
        {
            return false;
        }

        public virtual bool Handle(IInputStateSnapshot snapshot, IUiActionAdapter uiActionAdapter)
        {
            throw new NotImplementedException();
        }
        
        public virtual void SetupDefaultGeometry() {}
        
        protected IPhongMaterial NormalMaterial =>
            PhongMaterial.Create(
                PhongMaterialParameters.Create(
                    new Vector3(0.0f, 1.0f, 0.0f),
                    new Vector3(0.0f, 1.0f, 0.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    5f),
                PhongHeadlight.Create(PhongLightParameters.Create(
                    new Vector3(0.1f, 0.1f, 0.1f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    1f,
                    0)),
                true);
        
        protected IPhongMaterial PickMaterial =>
            PhongMaterial.Create(
                PhongMaterialParameters.Create(
                    new Vector3(1.0f, 1.0f, 0.0f),
                    new Vector3(1.0f, 1.0f, 0.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    5f),
                PhongHeadlight.Create(PhongLightParameters.Create(
                    new Vector3(0.1f, 0.1f, 0.1f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    1f,
                    0)),
                true);
    }
}