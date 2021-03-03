//
// Copyright 2018-2021 Sean Spicer 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System.Collections.Generic;
using System.Numerics;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Manipulators.Commands;
using Veldrid.SceneGraph.PipelineStates;
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
        private readonly IPhongMaterial _leftHandleMaterial;

        private INode _leftHandleNode;
        private IPhongMaterial _pickedHandleMaterial;
        private readonly IPhongMaterial _rightHandleMaterial;

        private INode _rightHandleNode;

        protected Translate1DDragger(Matrix4x4 matrix) : base(matrix, true)
        {
            _leftHandleMaterial = CreateMaterial();
            _rightHandleMaterial = CreateMaterial();
        }

        protected Translate1DDragger(Vector3 s, Vector3 e, Matrix4x4 matrix) : base(s, e, Matrix4x4.Identity, true)
        {
            _leftHandleMaterial = CreateMaterial();
            _rightHandleMaterial = CreateMaterial();
        }

        protected Vector3 StartProjectedPoint { get; set; }

        public INode LeftHandleNode
        {
            get => _leftHandleNode;
            set
            {
                _leftHandleNode = value;
                _leftHandleNode.PipelineState = _leftHandleMaterial.CreatePipelineState();
            }
        }

        public INode RightHandleNode
        {
            get => _rightHandleNode;
            set
            {
                _rightHandleNode = value;
                _rightHandleNode.PipelineState = _rightHandleMaterial.CreatePipelineState();
            }
        }

        public bool CheckForNodeInPath { get; set; } = true;

        public override void SetupDefaultGeometry()
        {
            var lineDir = LineProjector.LineEnd - LineProjector.LineStart;
            var lineLength = lineDir.Length();

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
                geometry.VertexLayouts = new List<VertexLayoutDescription>
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

            // Create a left Cone
            {
                var geode = Geode.Create();
                var cone = Cone.Create(LineProjector.LineStart, 0.5f * lineLength, 2f * lineLength);
                cone.Rotation = QuaternionExtensions.MakeRotate(lineDir, Vector3.UnitZ);

                geode.AddDrawable(ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    cone,
                    hints,
                    new[] {new Vector3(0.0f, 1.0f, 0.0f)}));

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

            // Create a right Cone
            {
                var geode = Geode.Create();
                var cone = Cone.Create(LineProjector.LineEnd, 0.5f * lineLength, 2f * lineLength);
                cone.Rotation = QuaternionExtensions.MakeRotate(Vector3.UnitZ, lineDir);

                geode.AddDrawable(ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    cone,
                    hints,
                    new[] {new Vector3(0.0f, 1.0f, 0.0f)}));

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

        public override bool Handle(IPointerInfo pointerInfo, IUiEventAdapter eventAdapter,
            IUiActionAdapter actionAdapter)
        {
            if (CheckForNodeInPath)
                // Check if the pointer is in the nodepath.
                if (!pointerInfo.Contains(LeftHandleNode) && !pointerInfo.Contains(RightHandleNode))
                    return false;

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

                        if (pointerInfo.Contains(LeftHandleNode))
                            _pickedHandleMaterial = _leftHandleMaterial;
                        else if (pointerInfo.Contains(RightHandleNode)) _pickedHandleMaterial = _rightHandleMaterial;

                        var cmd = TranslateInLineCommand.Create(LineProjector.LineStart, LineProjector.LineEnd);

                        cmd.Stage = IMotionCommand.MotionStage.Start;
                        cmd.SetLocalToWorldAndWorldToLocal(LineProjector.LocalToWorld, LineProjector.WorldToLocal);

                        Dispatch(cmd);

                        _pickedHandleMaterial?.SetMaterial(PickColor, PickColor, Vector3.One, 1);

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

                    _pickedHandleMaterial?.SetMaterial(Color, Color, Vector3.One, 1);

                    actionAdapter.RequestRedraw();

                    return true;
                }
                default:
                    return false;
            }
        }

        public new static ITranslate1DDragger Create()
        {
            return new Translate1DDragger(Matrix4x4.Identity);
        }

        public static ITranslate1DDragger Create(Vector3 s, Vector3 e)
        {
            return new Translate1DDragger(s, e, Matrix4x4.Identity);
        }
    }
}