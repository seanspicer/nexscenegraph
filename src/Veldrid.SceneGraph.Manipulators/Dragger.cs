
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
    
    public abstract class Dragger : MatrixTransform, IDragger
    {
        public Color Color { get; protected set; }
        public Color PickColor { get; protected set; }

        private bool _handleEvents = false;
        public bool HandleEvents
        {
            get => _handleEvents;
            set
            {
                if (_handleEvents == value) return;

                _handleEvents = value;
                
                // update the number of children that require an event traversal to make sure this dragger receives events
                if (_handleEvents)
                {
                    SetNumChildrenRequiringEventTraversal(GetNumChildrenRequiringEventTraversal()+1);
                }
                else if (GetNumChildrenRequiringEventTraversal() >= 1)
                {
                    SetNumChildrenRequiringEventTraversal(GetNumChildrenRequiringEventTraversal()-1);
                }
            }
        }

        public bool DraggerActive { get; set; } = false;

        public uint ActivationModKeyMask { get; set; } = 0;
        public uint ActivationMouseButtonMask { get; set; } = 0;
        public int ActivationKeyEvent { get; set; } = 0;
        public bool ActivationPermittedByModKeyMask { get; set; } = false;
        public bool ActivationPermittedByMouseButtonMask { get; set; } = false;
        public bool ActivationPermittedByKeyEvent { get; set; } = false;

        public List<Constraint> Constraints { get; } = new List<Constraint>();

        public List<DraggerCallback> DraggerCallbacks { get; } = new List<DraggerCallback>();

        public IDragger ParentDragger { get; set; } = null;

        public uint IntersectionNodeMask { get; set; } = 0xffffffff;
        
        private IDraggerCallback _selfUpdater;
        
        protected Dragger(Matrix4x4 matrix) : base(matrix)
        {
            ParentDragger = this;
            _selfUpdater = DraggerTransformCallback.Create(this);
        }

        /**
         *  Return true if the axis of the Locator are inverted requiring the faces of any cubes used from
         *  rendering to be flipped to ensure the correct front/back face is used.
         */
        public bool Inverted()
        {
            var xAxis = new Vector3(Matrix.M11, Matrix.M21, Matrix.M31);
            var yAxis = new Vector3(Matrix.M12, Matrix.M22, Matrix.M32);
            var zAxis = new Vector3(Matrix.M13, Matrix.M23, Matrix.M33);

            var volume = Vector3.Dot(Vector3.Cross(xAxis, yAxis), zAxis);
            return volume < 0.0f;
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