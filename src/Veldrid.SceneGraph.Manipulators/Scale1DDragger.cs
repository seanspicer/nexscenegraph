
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
        enum ScaleMode
        {
            ScaleWithOriginAsPivot,
            ScaleWithOppositeHandleAsPivot
        }
        
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
        
        public INode LeftHandleNode     { get; set; }
        public INode RightHandleNode  { get; set; }
        
        public IScale1DDragger.ScaleMode ScaleMode { get; set; }
        
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
                    Box.Create(LineProjector.LineStart, 0.05f*lineLength),
                    hints,
                    new [] {new Vector3(0.0f, 1.0f, 0.0f)}));

                geode.PipelineState = pipelineState;
                geode.PipelineState.RasterizerStateDescription 
                    = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
                AddChild(geode);
                LeftHandleNode = geode;

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
                RightHandleNode = geode;
            }
        }



    }
}