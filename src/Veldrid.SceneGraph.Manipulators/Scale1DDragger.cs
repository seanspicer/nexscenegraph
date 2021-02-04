
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http.Headers;
using System.Numerics;
using SharpDX.Direct3D11;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Manipulators.Commands;
using Veldrid.SceneGraph.PipelineStates;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IScale1DDragger : IDragger
    {
        enum ScaleMode
        {
            ScaleWithOriginAsPivot,
            ScaleWithOppositeHandleAsPivot
        }
        
        float MinScale { get; set; }
        
        float LeftHandlePosition { get; set; }
        float RightHandlePosition { get; set; }
        
        INode LeftHandleNode     { get; set; }
        INode RightHandleNode  { get; set; }
        
    }
    
    public class Scale1DDragger : Base1DDragger, IScale1DDragger 
    {
        public float LeftHandlePosition
        {
            get => LineProjector.LineStart.X;
            set => LineProjector.LineStart = new Vector3(value, 0.0f, 0.0f);
        }

        public float RightHandlePosition
        {
            get => LineProjector.LineEnd.X;
            set => LineProjector.LineEnd = new Vector3(value, 0.0f, 0.0f);
        }

        public float MinScale { get; set; } = 0.001f;
        
        public INode LeftHandleNode     { get; set; }
        public INode RightHandleNode  { get; set; }

        public IScale1DDragger.ScaleMode ScaleMode { get; set; }
        
        protected double ScaleCenter { get; set; }
        
        protected Vector3 StartProjectedPoint { get; set; }
        
        public static IScale1DDragger Create(IScale1DDragger.ScaleMode scaleMode = IScale1DDragger.ScaleMode.ScaleWithOriginAsPivot)
        {
            return new Scale1DDragger(scaleMode, Matrix4x4.Identity);
        }
        
        protected Scale1DDragger(IScale1DDragger.ScaleMode scaleMode, Matrix4x4 matrix) : base(matrix)
        {
            ScaleMode = scaleMode;
        }

        public override void SetupDefaultGeometry()
        {
            var lineDir = LineProjector.LineEnd - LineProjector.LineStart;
            float lineLength = lineDir.Length();
            
            // Create a Line
            var lineGeode = Geode.Create();
            {
                var geometry = Geometry<Position3Color3>.Create();
                var vertexArray = new Position3Color3[2];
                vertexArray[0] = new Position3Color3(LineProjector.LineStart, Vector3.One);
                vertexArray[1] = new Position3Color3(LineProjector.LineEnd, Vector3.One);

                var indexArray = new uint[2];
                indexArray[0] = 0;
                indexArray[1] = 1;

                geometry.IndexData = indexArray;
                geometry.VertexData = vertexArray;
                geometry.VertexLayouts = new List<VertexLayoutDescription>()
                {
                    Position3Color3.VertexLayoutDescription
                };
                
                var pSet = DrawElements<Position3Color3>.Create(
                    geometry,
                    PrimitiveTopology.LineStrip,
                    2,
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

            var pipelineState = NormalMaterial.CreatePipelineState();
            
            // Create a left box
            {
                var geode = Geode.Create();
                
                geode.AddDrawable(ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    Box.Create(LineProjector.LineStart, 0.5f*lineLength),
                    hints,
                    new [] {new Vector3(0.0f, 1.0f, 0.0f)}));

                geode.PipelineState = pipelineState;
                geode.PipelineState.RasterizerStateDescription 
                    = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
                
                var autoTransform = AutoTransform.Create();
                autoTransform.Position = LineProjector.LineStart;
                autoTransform.PivotPoint = LineProjector.LineStart;
                autoTransform.AutoScaleToScreen = true;
                autoTransform.AddChild(geode);
                
                var antiSquish = AntiSquish.Create(LineProjector.LineStart);
                antiSquish.AddChild(autoTransform);
                
                AddChild(antiSquish);
                LeftHandleNode = antiSquish;

            }
            
            // Create a right box
            {
                var geode = Geode.Create();
                
                geode.AddDrawable(ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    Box.Create(LineProjector.LineEnd, 0.5f*lineLength),
                    hints,
                    new [] {new Vector3(0.0f, 1.0f, 0.0f)}));

                geode.PipelineState = pipelineState;
                geode.PipelineState.RasterizerStateDescription 
                    = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);

                var autoTransform = AutoTransform.Create();
                autoTransform.Position = LineProjector.LineEnd;
                autoTransform.PivotPoint = LineProjector.LineEnd;
                autoTransform.AutoScaleToScreen = true;
                autoTransform.AddChild(geode);
                
                var antiSquish = AntiSquish.Create(LineProjector.LineEnd);
                antiSquish.AddChild(autoTransform);
                
                AddChild(antiSquish);
                RightHandleNode = antiSquish;
            }
        }

        public override bool Handle(IPointerInfo pointerInfo, IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter)
        {
            // Check if the pointer is in the nodepath.
            if (!pointerInfo.Contains(RightHandleNode) && !pointerInfo.Contains(LeftHandleNode)) return false;

            switch (eventAdapter.EventType)
            {
                // Pick Start
                case IUiEventAdapter.EventTypeValue.Push:
                {
                    // Get the local to world matrix for this node and set it for the projector
                    var nodePathToRoot = Util.ComputeNodePathToRoot(this);
                    var localToWorld = ComputeLocalToWorld(nodePathToRoot);
                    LineProjector.LocalToWorld = localToWorld;

                    if (LineProjector.Project(pointerInfo, out var startProjectedPoint))
                    {
                        StartProjectedPoint = startProjectedPoint;
                        ScaleCenter = 0.0;
                        if (ScaleMode == IScale1DDragger.ScaleMode.ScaleWithOppositeHandleAsPivot)
                        {
                            if (pointerInfo.Contains(LeftHandleNode))
                            {
                                ScaleCenter = LineProjector.LineEnd.X;
                            }
                            else if (pointerInfo.Contains(RightHandleNode))
                            {
                                ScaleCenter = LineProjector.LineStart.X;
                            }
                        }
                        
                        // Create the motion command
                        var cmd = Scale1DCommand.Create();
                        cmd.Stage = IMotionCommand.MotionStage.Start;
                        cmd.SetLocalToWorldAndWorldToLocal(LineProjector.LocalToWorld, LineProjector.WorldToLocal);
                        
                        Dispatch(cmd);
                        
                        // TODO -- Set material color
                        
                        actionAdapter.RequestRedraw();
                    }

                    return true;
                }
                // Pick Move
                case IUiEventAdapter.EventTypeValue.Drag:
                {
                    if (LineProjector.Project(pointerInfo, out var projectedPoint))
                    {
                        // Create the motion command
                        var cmd = Scale1DCommand.Create();
                        
                        // Calculate scale
                        var scale = ComputeScale(StartProjectedPoint, projectedPoint, ScaleCenter);
                        if (scale < MinScale) scale = MinScale;
                        
                        // Step the reference point to the line start or end depending on which is closer
                        var referencePoint = StartProjectedPoint.X;
                        if (System.Math.Abs(LineProjector.LineStart.X - referencePoint) <
                            System.Math.Abs(LineProjector.LineEnd.X) - referencePoint)
                        {
                            referencePoint = LineProjector.LineStart.X;
                        }
                        else
                        {
                            referencePoint = LineProjector.LineEnd.X;
                        }

                        cmd.Stage = IMotionCommand.MotionStage.Move;
                        cmd.SetLocalToWorldAndWorldToLocal(LineProjector.LocalToWorld, LineProjector.WorldToLocal);
                        cmd.Scale = scale;
                        cmd.ScaleCenter = ScaleCenter;
                        cmd.ReferencePoint = referencePoint;
                        cmd.MinScale = MinScale;
                        
                        Dispatch(cmd);
                        
                        actionAdapter.RequestRedraw();
                        
                    }
                    return true;
                }
                case IUiEventAdapter.EventTypeValue.Release:
                {
                    // Create the motion command
                    var cmd = Scale1DCommand.Create();
                    cmd.Stage = IMotionCommand.MotionStage.Finish;
                    cmd.SetLocalToWorldAndWorldToLocal(LineProjector.LocalToWorld, LineProjector.WorldToLocal);
                    
                    Dispatch(cmd);
                    
                    // TODO: Reset Color
                    
                    actionAdapter.RequestRedraw();

                    return true;
                }
                default:
                    return false;
            }
        }
        
        internal static double ComputeScale(Vector3 startProjectedPoint, Vector3 projectedPoint, double scaleCenter)
        {
            var denom = startProjectedPoint.X - scaleCenter;
            var scale = (0 != denom) ? (projectedPoint.X - scaleCenter)/denom : 1.0;
            return scale;
        }
    }
}