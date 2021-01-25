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

using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Veldrid.SceneGraph.PipelineStates;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IScale1DDragger : IDragger
    {
        Color Color { get; }
        Color PickColor { get; }
    }
    
    public class Scale1DDragger : Dragger, IScale1DDragger 
    {
        protected ILineProjector LineProjector { get; set; }

        public new static IScale1DDragger Create()
        {
            return new Scale1DDragger(Matrix4x4.Identity);
        }
        
        protected Scale1DDragger(Matrix4x4 matrix) : base(matrix)
        {
            LineProjector = Veldrid.SceneGraph.Manipulators.LineProjector.Create(
                LineSegment.Create(new Vector3(-0.5f, 0.0f, 0.0f), new Vector3(0.5f, 0.0f, 0.0f)));
            
            Color = Color.Green;
            PickColor = Color.Magenta;
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
                    Box.Create(LineProjector.LineStart, 0.05f*lineLength),
                    hints,
                    new [] {new Vector3(0.0f, 1.0f, 0.0f)}));

                geode.PipelineState = pipelineState;
                geode.PipelineState.RasterizerStateDescription 
                    = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
                AddChild(geode);
            }
            
            // Create a right box
            {
                var geode = Geode.Create();
                
                geode.AddDrawable(ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    Box.Create(LineProjector.LineEnd, 0.05f*lineLength),
                    hints,
                    new [] {new Vector3(0.0f, 1.0f, 0.0f)}));

                geode.PipelineState = pipelineState;
                geode.PipelineState.RasterizerStateDescription 
                    = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
                
                AddChild(geode);
            }
        }

        public Color Color { get; protected set; }
        public Color PickColor { get; protected set; }

    }
}