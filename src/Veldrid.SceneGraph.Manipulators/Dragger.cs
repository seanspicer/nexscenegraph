
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Drawing;
using System.Globalization;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Manipulators.Commands;
using Veldrid.SceneGraph.PipelineStates;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IDragger : IMatrixTransform
    {
        void SetupDefaultGeometry();
        bool HandleEvents { get; set; }
        bool Handle(IInputStateSnapshot snapshot, IUiActionAdapter uiActionAdapter);
        bool Handle(IPointerInfo pointerInfo, IInputStateSnapshot snapshot, IUiActionAdapter uiActionAdapter);
        
        List<Constraint> Constraints { get; }
        List<DraggerCallback> DraggerCallbacks { get; }
        
        Color Color { get; }
        Color PickColor { get; }
        
        uint ActivationModKeyMask { get; set; }
        
        uint ActivationMouseButtonMask { get; set; }
        int  ActivationKeyEvent { get; set; }
        bool ActivationPermittedByModKeyMask { get; set; }
        bool ActivationPermittedByMouseButtonMask { get; set; }
        bool ActivationPermittedByKeyEvent { get; set; }
        IDragger ParentDragger { get; set; }
        bool DraggerActive { get; set; }
        uint IntersectionNodeMask { get; set; }

        bool Receive(IMotionCommand command);
        void Dispatch(IMotionCommand command);
    }
    
    public class Dragger : MatrixTransform, IDragger
    {
        public Color Color { get; protected set; }
        public Color PickColor { get; protected set; }
        
        public bool HandleEvents { get; set; }

        public bool DraggerActive { get; set; } = true;
        
        public uint ActivationModKeyMask { get; set; }
        public uint ActivationMouseButtonMask { get; set; }
        public int  ActivationKeyEvent { get; set; }
        public bool ActivationPermittedByModKeyMask { get; set; }
        public bool ActivationPermittedByMouseButtonMask { get; set; }
        public bool ActivationPermittedByKeyEvent { get; set; }

        public List<Constraint> Constraints { get; } = new List<Constraint>();

        public List<DraggerCallback> DraggerCallbacks { get; } = new List<DraggerCallback>();
        
        public IDragger ParentDragger { get; set; }
        
        public uint IntersectionNodeMask { get; set; }
        
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

        public bool Receive(IMotionCommand command)
        {
            return true;
        }

        public void Dispatch(IMotionCommand command)
        {
            throw new NotImplementedException();
        }

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