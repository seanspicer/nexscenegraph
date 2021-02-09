

using System.Collections.Generic;
using System.Net.Security;
using System.Numerics;
using Veldrid.SceneGraph.AssetPrimitives;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Manipulators.Commands;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface ITranslate1DDragger : IDragger
    {
        public bool CheckForNodeInPath { get; set; }
    }
    
    public class Translate1DDragger : Base1DDragger, ITranslate1DDragger
    {
        protected Vector3 StartProjectedPoint { get; set; }

        public bool CheckForNodeInPath { get; set; } = true;
        
        public new static ITranslate1DDragger Create()
        {
            return new Translate1DDragger(Matrix4x4.Identity);
        }
        
        public new static ITranslate1DDragger Create(Vector3 s, Vector3 e)
        {
            return new Translate1DDragger(s, e, Matrix4x4.Identity);
        }
        
        protected Translate1DDragger(Matrix4x4 matrix) : base(matrix)
        {
        }
        
        protected Translate1DDragger(Vector3 s, Vector3 e, Matrix4x4 matrix) : base(s,e, Matrix4x4.Identity)
        {
            
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
            //hints.NormalsType = NormalsType.PerFace;
            hints.SetDetailRatio(1.0f);

            var pipelineState = CreateMaterial().CreatePipelineState();
            
            // Create a left Cone
            {
                var geode = Geode.Create();
                var cone = Cone.Create(LineProjector.LineStart, 0.025f * lineLength, 0.1f * lineLength);
                cone.Rotation = QuaternionExtensions.MakeRotate(lineDir, Vector3.UnitZ);
                
                geode.AddDrawable(ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    cone,
                    hints,
                    new [] {new Vector3(0.0f, 1.0f, 0.0f)}));

                geode.PipelineState = pipelineState;
                geode.PipelineState.RasterizerStateDescription 
                    = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
                AddChild(geode);
            }
            
            // Create a right Cone
            {
                var geode = Geode.Create();
                var cone = Cone.Create(LineProjector.LineEnd, 0.025f * lineLength, 0.1f * lineLength);
                cone.Rotation = QuaternionExtensions.MakeRotate(Vector3.UnitZ, lineDir);
                
                geode.AddDrawable(ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    cone,
                    hints,
                    new [] {new Vector3(0.0f, 1.0f, 0.0f)}));

                geode.PipelineState = pipelineState;
                geode.PipelineState.RasterizerStateDescription 
                    = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
                AddChild(geode);
            }
        }

        public override bool Handle(IPointerInfo pointerInfo, IUiEventAdapter eventAdapter,
            IUiActionAdapter actionAdapter)
        {
            if (CheckForNodeInPath)
            {
                // Check if the pointer is in the nodepath.
                if (!pointerInfo.Contains(this)) return false;
            }

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
                        
                        var cmd = TranslateInLineCommand.Create(LineProjector.LineStart, LineProjector.LineEnd);

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
                        var cmd = TranslateInLineCommand.Create(LineProjector.LineStart, LineProjector.LineEnd);
                        cmd.Stage = IMotionCommand.MotionStage.Move;
                        cmd.SetLocalToWorldAndWorldToLocal(LineProjector.LocalToWorld, LineProjector.WorldToLocal);
                        cmd.Translation = projectedPoint - StartProjectedPoint;

                        Dispatch(cmd);
                        
                        actionAdapter.RequestRedraw();
                        
                    }
                    return true;
                }
                case IUiEventAdapter.EventTypeValue.Release:
                {
                    // Create the motion command
                    var cmd = TranslateInLineCommand.Create(LineProjector.LineStart, LineProjector.LineEnd);
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

    }
}