using System.Collections.Generic;
using System.Numerics;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Manipulators.Commands;
using Veldrid.SceneGraph.PipelineStates;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IScale2DDragger : IDragger
    {
        enum ScaleMode
        {
            ScaleWithOriginAsPivot,
            ScaleWithOppositeHandleAsPivot
        }

        Vector2 TopLeftHandlePosition { get; set; }
        Vector2 BottomLeftHandlePosition { get; set; }
        Vector2 TopRightHandlePosition { get; set; }
        Vector2 BottomRightHandlePosition { get; set; }

        INode TopLeftHandleNode { get; set; }
        INode BottomLeftHandleNode { get; set; }
        INode TopRightHandleNode { get; set; }
        INode BottomRightHandleNode { get; set; }
    }

    public class Scale2DDragger : Base2DDragger, IScale2DDragger
    {
        private readonly IPhongMaterial _bottomLeftHandleMaterial;

        private INode _bottomLeftHandleNode;
        private readonly IPhongMaterial _bottomRightHandleMaterial;

        private INode _bottomRightHandleNode;
        private IPhongMaterial _pickedHandleMaterial;

        private readonly IPhongMaterial _topLeftHandleMaterial;

        private INode _topLeftHandleNode;
        private readonly IPhongMaterial _topRightHandleMaterial;

        private INode _topRightHandleNode;

        protected Scale2DDragger(IScale2DDragger.ScaleMode scaleMode, Matrix4x4 matrix, bool usePhongShading) : base(
            matrix, usePhongShading)
        {
            ScaleMode = scaleMode;

            TopLeftHandlePosition = new Vector2(-0.5f, 0.5f);
            BottomLeftHandlePosition = new Vector2(-0.5f, -0.5f);
            BottomRightHandlePosition = new Vector2(0.5f, -0.5f);
            TopRightHandlePosition = new Vector2(0.5f, 0.5f);

            _topLeftHandleMaterial = CreateMaterial();
            _topRightHandleMaterial = CreateMaterial();
            _bottomLeftHandleMaterial = CreateMaterial();
            _bottomRightHandleMaterial = CreateMaterial();
        }

        protected Vector3 StartProjectedPoint { get; set; }
        protected Vector2 ScaleCenter { get; set; }

        protected Vector2 ReferencePoint { get; set; }
        protected Vector2 MinScale { get; set; } = new Vector2(0.001f, 0.001f);

        public IScale2DDragger.ScaleMode ScaleMode { get; set; }
        public Vector2 TopLeftHandlePosition { get; set; }
        public Vector2 BottomLeftHandlePosition { get; set; }
        public Vector2 TopRightHandlePosition { get; set; }
        public Vector2 BottomRightHandlePosition { get; set; }

        public INode TopLeftHandleNode
        {
            get => _topLeftHandleNode;
            set
            {
                _topLeftHandleNode = value;
                _topLeftHandleNode.PipelineState = _topLeftHandleMaterial.CreatePipelineState();
            }
        }

        public INode BottomLeftHandleNode
        {
            get => _bottomLeftHandleNode;
            set
            {
                _bottomLeftHandleNode = value;
                _bottomLeftHandleNode.PipelineState = _bottomLeftHandleMaterial.CreatePipelineState();
            }
        }

        public INode TopRightHandleNode
        {
            get => _topRightHandleNode;
            set
            {
                _topRightHandleNode = value;
                _topRightHandleNode.PipelineState = _topRightHandleMaterial.CreatePipelineState();
            }
        }

        public INode BottomRightHandleNode
        {
            get => _bottomRightHandleNode;
            set
            {
                _bottomRightHandleNode = value;
                _bottomRightHandleNode.PipelineState = _bottomRightHandleMaterial.CreatePipelineState();
            }
        }

        public override void SetupDefaultGeometry()
        {
            // Create a Line
            var lineGeode = Geode.Create();
            {
                var geometry = Geometry<Position3Color3>.Create();
                var vertexArray = new Position3Color3[4];
                vertexArray[0] =
                    new Position3Color3(new Vector3(TopLeftHandlePosition.X, 0.0f, TopLeftHandlePosition.Y),
                        Vector3.One);
                vertexArray[1] =
                    new Position3Color3(new Vector3(BottomLeftHandlePosition.X, 0.0f, BottomLeftHandlePosition.Y),
                        Vector3.One);
                vertexArray[2] =
                    new Position3Color3(new Vector3(BottomRightHandlePosition.X, 0.0f, BottomRightHandlePosition.Y),
                        Vector3.One);
                vertexArray[3] =
                    new Position3Color3(new Vector3(TopRightHandlePosition.X, 0.0f, TopRightHandlePosition.Y),
                        Vector3.One);

                var indexArray = new uint[5];
                indexArray[0] = 0;
                indexArray[1] = 1;
                indexArray[2] = 2;
                indexArray[3] = 3;
                indexArray[4] = 0;

                geometry.IndexData = indexArray;
                geometry.VertexData = vertexArray;
                geometry.VertexLayouts = new List<VertexLayoutDescription>
                {
                    Position3Color3.VertexLayoutDescription
                };

                var pSet = DrawElements<Position3Color3>.Create(
                    geometry,
                    PrimitiveTopology.LineStrip,
                    5,
                    1,
                    0,
                    0,
                    0);

                geometry.PrimitiveSets.Add(pSet);

                geometry.PipelineState.ShaderSet = Position3Color3Shader.Instance.ShaderSet;

                lineGeode.AddDrawable(geometry);
            }
            AddChild(lineGeode);

            var hints = TessellationHints.Create();
            hints.ColorsType = ColorsType.ColorOverall;
            hints.NormalsType = NormalsType.PerFace;

            // Create top left box
            {
                var geode = Geode.Create();
                var pos = new Vector3(TopLeftHandlePosition.X, 0.0f, TopLeftHandlePosition.Y);

                geode.AddDrawable(ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    Box.Create(pos, 0.5f),
                    hints,
                    new[] {new Vector3(0.0f, 1.0f, 0.0f)}));

                var autoTransform = AutoTransform.Create();
                autoTransform.Position = pos;
                autoTransform.PivotPoint = pos;
                autoTransform.AutoScaleToScreen = true;
                autoTransform.AddChild(geode);

                var antiSquish = AntiSquish.Create(pos);
                antiSquish.AddChild(autoTransform);

                AddChild(antiSquish);
                TopLeftHandleNode = antiSquish;
            }

            // Create bottom left box
            {
                var geode = Geode.Create();
                var pos = new Vector3(BottomLeftHandlePosition.X, 0.0f, BottomLeftHandlePosition.Y);

                geode.AddDrawable(ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    Box.Create(pos, 0.5f),
                    hints,
                    new[] {new Vector3(0.0f, 1.0f, 0.0f)}));

                var autoTransform = AutoTransform.Create();
                autoTransform.Position = pos;
                autoTransform.PivotPoint = pos;
                autoTransform.AutoScaleToScreen = true;
                autoTransform.AddChild(geode);

                var antiSquish = AntiSquish.Create(pos);
                antiSquish.AddChild(autoTransform);

                AddChild(antiSquish);
                BottomLeftHandleNode = antiSquish;
            }

            // Create bottom right box
            {
                var geode = Geode.Create();
                var pos = new Vector3(BottomRightHandlePosition.X, 0.0f, BottomRightHandlePosition.Y);

                geode.AddDrawable(ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    Box.Create(pos, 0.5f),
                    hints,
                    new[] {new Vector3(0.0f, 1.0f, 0.0f)}));

                var autoTransform = AutoTransform.Create();
                autoTransform.Position = pos;
                autoTransform.PivotPoint = pos;
                autoTransform.AutoScaleToScreen = true;
                autoTransform.AddChild(geode);

                var antiSquish = AntiSquish.Create(pos);
                antiSquish.AddChild(autoTransform);

                AddChild(antiSquish);
                BottomRightHandleNode = antiSquish;
            }

            // Create top right box;
            {
                var geode = Geode.Create();
                var pos = new Vector3(TopRightHandlePosition.X, 0.0f, TopRightHandlePosition.Y);

                geode.AddDrawable(ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    Box.Create(pos, 0.5f),
                    hints,
                    new[] {new Vector3(0.0f, 1.0f, 0.0f)}));

                var autoTransform = AutoTransform.Create();
                autoTransform.Position = pos;
                autoTransform.PivotPoint = pos;
                autoTransform.AutoScaleToScreen = true;
                autoTransform.AddChild(geode);

                var antiSquish = AntiSquish.Create(pos);
                antiSquish.AddChild(autoTransform);

                AddChild(antiSquish);
                TopRightHandleNode = antiSquish;
            }
        }

        public override bool Handle(IPointerInfo pointerInfo, IUiEventAdapter eventAdapter,
            IUiActionAdapter actionAdapter)
        {
            if (!pointerInfo.Contains(TopLeftHandleNode) &&
                !pointerInfo.Contains(BottomLeftHandleNode) &&
                !pointerInfo.Contains(TopRightHandleNode) &&
                !pointerInfo.Contains(BottomRightHandleNode)) return false;

            switch (eventAdapter.EventType)
            {
                // Pick Start
                case IUiEventAdapter.EventTypeValue.Push:
                {
                    // Get the local to world matrix for this node and set it for the projector
                    var nodePathToRoot = Util.ComputeNodePathToRoot(this);
                    var localToWorld = ComputeLocalToWorld(nodePathToRoot);
                    PlaneProjector.LocalToWorld = localToWorld;

                    if (PlaneProjector.Project(pointerInfo, out var startProjectedPoint))
                    {
                        ScaleCenter = Vector2.Zero;
                        StartProjectedPoint = startProjectedPoint;

                        if (pointerInfo.Contains(TopLeftHandleNode))
                        {
                            _pickedHandleMaterial = _topLeftHandleMaterial;
                            ReferencePoint = TopLeftHandlePosition;
                            if (ScaleMode == IScale2DDragger.ScaleMode.ScaleWithOppositeHandleAsPivot)
                                ScaleCenter = BottomRightHandlePosition;
                        }
                        else if (pointerInfo.Contains(BottomLeftHandleNode))
                        {
                            _pickedHandleMaterial = _bottomLeftHandleMaterial;
                            ReferencePoint = BottomLeftHandlePosition;
                            if (ScaleMode == IScale2DDragger.ScaleMode.ScaleWithOppositeHandleAsPivot)
                                ScaleCenter = TopRightHandlePosition;
                        }
                        else if (pointerInfo.Contains(BottomRightHandleNode))
                        {
                            _pickedHandleMaterial = _bottomRightHandleMaterial;
                            ReferencePoint = BottomRightHandlePosition;
                            if (ScaleMode == IScale2DDragger.ScaleMode.ScaleWithOppositeHandleAsPivot)
                                ScaleCenter = TopLeftHandlePosition;
                        }
                        else if (pointerInfo.Contains(TopRightHandleNode))
                        {
                            _pickedHandleMaterial = _topRightHandleMaterial;
                            ReferencePoint = TopRightHandlePosition;
                            if (ScaleMode == IScale2DDragger.ScaleMode.ScaleWithOppositeHandleAsPivot)
                                ScaleCenter = BottomLeftHandlePosition;
                        }

                        // Create the motion command
                        var cmd = Scale2DCommand.Create();
                        cmd.Stage = IMotionCommand.MotionStage.Start;
                        cmd.SetLocalToWorldAndWorldToLocal(PlaneProjector.LocalToWorld, PlaneProjector.WorldToLocal);
                        cmd.ReferencePoint = ReferencePoint;

                        Dispatch(cmd);

                        var pickColor = GetColorVector(PickColor);
                        _pickedHandleMaterial?.SetMaterial(pickColor, pickColor, Vector3.One, 1);

                        actionAdapter.RequestRedraw();
                    }

                    return true;
                }
                // Pick Move
                case IUiEventAdapter.EventTypeValue.Drag:
                {
                    if (PlaneProjector.Project(pointerInfo, out var projectedPoint))
                    {
                        // Calculate scale
                        var scale = ComputeScale(StartProjectedPoint, projectedPoint, ScaleCenter);
                        if (scale.X < MinScale.X) scale.X = MinScale.X;
                        if (scale.Y < MinScale.Y) scale.Y = MinScale.Y;


                        // Create the motion command
                        var cmd = Scale2DCommand.Create();
                        cmd.Stage = IMotionCommand.MotionStage.Move;
                        cmd.SetLocalToWorldAndWorldToLocal(PlaneProjector.LocalToWorld, PlaneProjector.WorldToLocal);
                        cmd.Scale = scale;
                        cmd.ScaleCenter = ScaleCenter;
                        cmd.ReferencePoint = ReferencePoint;
                        cmd.MinScale = MinScale;

                        Dispatch(cmd);

                        actionAdapter.RequestRedraw();
                    }

                    return true;
                }
                case IUiEventAdapter.EventTypeValue.Release:
                {
                    // Create the motion command
                    var cmd = Scale2DCommand.Create();
                    cmd.Stage = IMotionCommand.MotionStage.Finish;
                    cmd.ReferencePoint = ReferencePoint;
                    cmd.SetLocalToWorldAndWorldToLocal(PlaneProjector.LocalToWorld, PlaneProjector.WorldToLocal);

                    Dispatch(cmd);

                    var normalColor = GetColorVector(Color);
                    _pickedHandleMaterial?.SetMaterial(normalColor, normalColor, Vector3.One, 1);

                    actionAdapter.RequestRedraw();

                    return true;
                }
                default:
                    return false;
            }
        }

        public static IScale2DDragger Create(
            IScale2DDragger.ScaleMode scaleMode = IScale2DDragger.ScaleMode.ScaleWithOriginAsPivot,
            bool usePhongShading = true)
        {
            return new Scale2DDragger(scaleMode, Matrix4x4.Identity, usePhongShading);
        }

        private Vector2 ComputeScale(Vector3 startProjectedPoint, Vector3 projectedPoint, Vector2 scaleCenter)
        {
            var scale = Vector2.One;
            if (startProjectedPoint.X - scaleCenter.X != 0.0f)
                scale.X = (projectedPoint.X - scaleCenter.X) / (startProjectedPoint.X - scaleCenter.X);
            if (startProjectedPoint.Z - scaleCenter.Y != 0.0)
                scale.Y = (projectedPoint.Z - scaleCenter.Y) / (startProjectedPoint.Z - scaleCenter.Y);
            return scale;
        }
    }
}