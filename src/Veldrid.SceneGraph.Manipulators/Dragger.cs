
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Manipulators.Commands;
using Veldrid.SceneGraph.PipelineStates;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IDragger : IMatrixTransform
    {
        void SetupDefaultGeometry();
        bool HandleEvents { get; set; }
        bool Handle(IUiEventAdapter eventAdapter, IUiActionAdapter uiActionAdapter);
        bool Handle(IPointerInfo pointerInfo, IUiEventAdapter eventAdapter, IUiActionAdapter uiActionAdapter);
        
        HashSet<IConstraint> Constraints { get; }
        HashSet<IDraggerCallback> DraggerCallbacks { get; }
        
        Color Color { get; }
        Color PickColor { get; }
        
        IUiEventAdapter.ModKeyMaskType ActivationModKeyMask { get; set; }
        IUiEventAdapter.MouseButtonMaskType ActivationMouseButtonMask { get; set; }
        IUiEventAdapter.KeySymbol  ActivationKeyEvent { get; set; }
        bool ActivationPermittedByModKeyMask { get; set; }
        bool ActivationPermittedByMouseButtonMask { get; set; }
        bool ActivationPermittedByKeyEvent { get; set; }
        IDragger ParentDragger { get; set; }
        bool DraggerActive { get; set; }
        uint IntersectionNodeMask { get; set; }

        bool Receive(IMotionCommand command);
        void Dispatch(IMotionCommand command);

        void AddTransformUpdating(IMatrixTransform transform,
            IDraggerTransformCallback.HandleCommandMask handleCommandMask =
                IDraggerTransformCallback.HandleCommandMask.HandleAll);

        void RemoveTransformUpdating(IMatrixTransform transform);

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

        protected IPointerInfo PointerInfo { get; set; } = Veldrid.SceneGraph.Manipulators.PointerInfo.Create();
        
        public IUiEventAdapter.ModKeyMaskType ActivationModKeyMask { get; set; } = 0;
        public IUiEventAdapter.MouseButtonMaskType ActivationMouseButtonMask { get; set; } = 0;
        public IUiEventAdapter.KeySymbol ActivationKeyEvent { get; set; } = 0;
        public bool ActivationPermittedByModKeyMask { get; set; } = false;
        public bool ActivationPermittedByMouseButtonMask { get; set; } = false;
        public bool ActivationPermittedByKeyEvent { get; set; } = false;

        public HashSet<IConstraint> Constraints { get; } = new HashSet<IConstraint>();

        public HashSet<IDraggerCallback> DraggerCallbacks { get; } = new HashSet<IDraggerCallback>();

        private IDragger _parentDragger = null;

        public virtual IDragger ParentDragger
        {
            get => _parentDragger;
            set => _parentDragger = value;
        }

        public virtual uint IntersectionNodeMask { get; set; } = 0xffffffff;
        
        private IDraggerCallback _selfUpdater;
        
        protected Dragger(Matrix4x4 matrix) : base(matrix)
        {
            _parentDragger = this;
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

        public void AddTransformUpdating(IMatrixTransform transform,
            IDraggerTransformCallback.HandleCommandMask handleCommandMask =
                IDraggerTransformCallback.HandleCommandMask.HandleAll)
        {
            DraggerCallbacks.Add(DraggerTransformCallback.Create(transform, handleCommandMask));
        }

        public void RemoveTransformUpdating(IMatrixTransform transform)
        {
            DraggerCallbacks.RemoveWhere(x =>
            {
                if (!(x is IDraggerTransformCallback dtc)) return false;
                
                if (dtc.Transform == transform)
                {
                    return true;
                }

                return false;
            });
        }

        // Allow this node (and its children) to handle events in traversal
        public override void Traverse(INodeVisitor nodeVisitor)
        {
            if (HandleEvents && nodeVisitor.Type == NodeVisitor.VisitorType.EventVisitor)
            {
                if (nodeVisitor is IEventVisitor eventVisitor) 
                {
                    foreach (var evt in eventVisitor.Events)
                    {
                        if (evt is IUiEventAdapter eventAdapter)
                        {
                            if (Handle(eventAdapter, eventVisitor.ActionAdapter))
                            {
                                eventAdapter.Handled = true;
                            }
                        }
                    }
                }

                return;
            }
            
            base.Traverse(nodeVisitor);
        }
        
        public virtual bool Handle(IUiEventAdapter eventAdapter,
            IUiActionAdapter actionAdapter)
        {
            if (eventAdapter.Handled) return false;

            if (actionAdapter is Veldrid.SceneGraph.Viewer.IView view)
            {
                var handled = false;
                var activationPermitted = true;

                if (ActivationModKeyMask != 0 || ActivationMouseButtonMask != 0 || ActivationKeyEvent != 0)
                {
                    ActivationPermittedByModKeyMask = (ActivationModKeyMask != 0) && 
                                                      ((eventAdapter.ModKeyMask & 
                                                        ActivationModKeyMask) != 0);
                    
                    ActivationPermittedByMouseButtonMask = (ActivationMouseButtonMask != 0) &&
                                                           ((eventAdapter.MouseButtonMask &
                                                             ActivationMouseButtonMask) != 0);

                }

                if (ActivationKeyEvent != 0)
                {
                    switch (eventAdapter.EventType)
                    {
                        case IUiEventAdapter.EventTypeValue.KeyDown:
                        {
                            if (eventAdapter.Key == ActivationKeyEvent)
                            {
                                ActivationPermittedByKeyEvent = true;
                            }

                            break;
                        }
                        case IUiEventAdapter.EventTypeValue.KeyUp:
                        {
                            if (eventAdapter.Key == ActivationKeyEvent)
                            {
                                ActivationPermittedByKeyEvent = false;
                            }

                            break;
                        }
                        default:
                        {
                            break;
                        }
                    }

                    activationPermitted = ActivationPermittedByModKeyMask || 
                                          ActivationPermittedByMouseButtonMask ||
                                          ActivationPermittedByKeyEvent;
                }

                if (activationPermitted || DraggerActive)
                {
                    switch (eventAdapter.EventType)
                    {
                        case IUiEventAdapter.EventTypeValue.Push:
                        {
                            var intersections = new SortedMultiSet<ILineSegmentIntersector.IIntersection>();
                            
                            PointerInfo.Reset();
                            if (view.ComputeIntersections(eventAdapter, ref intersections, IntersectionNodeMask))
                            {
                                foreach (var intersection in intersections)
                                {
                                    PointerInfo.HitList.Add(new Tuple<NodePath, Vector3>(intersection.NodePath,
                                        intersection.LocalIntersectionPoint));
                                }

                                foreach (var node in PointerInfo.HitList.First().Item1)
                                {
                                    if (node is IDragger dragger)
                                    {
                                        if (dragger == this)
                                        {
                                            var rootCamera = view.Camera;
                                            var nodePath = PointerInfo.HitList.First().Item1;
                                            foreach (var rnode in nodePath.Reverse())
                                            {
                                                if (rnode is Camera camera)
                                                {
                                                    if (camera.ReferenceFrame !=
                                                        ReferenceFrameType.Relative || camera.NumParents == 0)
                                                    {
                                                        rootCamera = camera;
                                                        break;
                                                    }
                                                }
                                            }
                                            
                                            PointerInfo.SetCamera(rootCamera);
                                            PointerInfo.SetMousePosition(eventAdapter.X, eventAdapter.Y);

                                            if (dragger.Handle(PointerInfo, eventAdapter, actionAdapter))
                                            {
                                                dragger.DraggerActive = true;
                                                handled = true;
                                            }
                                        }
                                    }
                                }
                            }

                            break;
                        }
                        case IUiEventAdapter.EventTypeValue.Drag:
                        case IUiEventAdapter.EventTypeValue.Release:
                        {
                            if (DraggerActive)
                            {
                                PointerInfo.SetMousePosition(eventAdapter.X, eventAdapter.Y);

                                if (Handle(PointerInfo, eventAdapter, actionAdapter))
                                {
                                    handled = true;
                                }
                            }
                            break;
                        }
                        default:
                            break;
                    }

                    if (DraggerActive && eventAdapter.EventType == IUiEventAdapter.EventTypeValue.Release)
                    {
                        DraggerActive = false;
                        PointerInfo.Reset();
                    }
                }

                return handled;
            }

            return false;
        }

        public virtual bool Handle(IPointerInfo pointerInfo, IUiEventAdapter eventAdapter,
            IUiActionAdapter actionAdapter)
        {
            return false;
        }
        
        public virtual void SetupDefaultGeometry() {}

        public bool Receive(IMotionCommand command)
        {
            return null != _selfUpdater && _selfUpdater.Receive(command);
        }

        public void Dispatch(IMotionCommand command)
        {
            // Apply any constraints
            foreach (var constraint in Constraints)
            {
                command.Accept(constraint);
            }
            
            // Apply any constraints of the parent dragger
            if (this != ParentDragger)
            {
                foreach (var constraint in ParentDragger.Constraints)
                {
                    command.Accept(constraint);
                }
            }
            
            // Move self
            ParentDragger.Receive(command);
            
            // Pass along movement to any callbacks
            foreach (var callback in DraggerCallbacks)
            {
                command.Accept(callback);
            }
        }
        

        protected IPhongMaterial NormalMaterial =>
            PhongMaterial.Create(
                PhongMaterialParameters.Create(
                    new Vector3(0.0f, 1.0f, 0.0f),
                    new Vector3(0.0f, 1.0f, 0.0f),
                    new Vector3(0.0f, 0.0f, 0.0f),
                    1f),
                PhongHeadlight.Create(PhongLightParameters.Create(
                    new Vector3(0.2f, 0.2f, 0.2f),
                    new Vector3(0.2f, 0.2f, 0.2f),
                    new Vector3(0.0f, 0.0f, 0.0f),
                    5f,
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