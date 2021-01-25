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
using System.Numerics;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface ITranslate1DDragger : IDragger
    {
        
    }
    
    public class Translate1DDragger : Base1DDragger, ITranslate1DDragger
    {
        public static ITranslate1DDragger Create()
        {
            return new Translate1DDragger(Matrix4x4.Identity);
        }
        
        protected Translate1DDragger(Matrix4x4 matrix) : base(matrix)
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
            hints.SetDetailRatio(1.0f);

            var pipelineState = NormalMaterial.CreatePipelineState();
            
            // Create a left Cone
            {
                var geode = Geode.Create();
                
                geode.AddDrawable(ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    Cone.Create(LineProjector.LineStart, 0.025f*lineLength, 0.1f*lineLength),
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
                
                geode.AddDrawable(ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    Cone.Create(LineProjector.LineEnd, 0.025f*lineLength, 0.1f*lineLength),
                    hints,
                    new [] {new Vector3(0.0f, 1.0f, 0.0f)}));

                geode.PipelineState = pipelineState;
                geode.PipelineState.RasterizerStateDescription 
                    = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
                AddChild(geode);
            }
        }
        
    }
}