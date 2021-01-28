

using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
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
        
        INode TopLeftHandleNode     { get; set; }
        INode BottomLeftHandleNode  { get; set; }
        INode TopRightHandleNode    { get; set; }
        INode BottomRightHandleNode { get; set; }
    }
    
    public class Scale2DDragger : Base2DDragger, IScale2DDragger
    {
        public Vector2 TopLeftHandlePosition { get; set; }
        public Vector2 BottomLeftHandlePosition { get; set; }
        public Vector2 TopRightHandlePosition { get; set; }
        public Vector2 BottomRightHandlePosition { get; set; }

        public INode TopLeftHandleNode     { get; set; }
        public INode BottomLeftHandleNode  { get; set; }
        public INode TopRightHandleNode    { get; set; }
        public INode BottomRightHandleNode { get; set; }
        
        public IScale2DDragger.ScaleMode ScaleMode { get; set; }
        
        public static IScale2DDragger Create(IScale2DDragger.ScaleMode scaleMode = IScale2DDragger.ScaleMode.ScaleWithOriginAsPivot)
        {
            return new Scale2DDragger(scaleMode, Matrix4x4.Identity);
        }
        
        protected Scale2DDragger(IScale2DDragger.ScaleMode scaleMode, Matrix4x4 matrix) : base(matrix)
        {
            ScaleMode = scaleMode;
            
            TopLeftHandlePosition     = new Vector2(-0.5f,  0.5f);
            BottomLeftHandlePosition  = new Vector2(-0.5f, -0.5f);
            BottomRightHandlePosition = new Vector2( 0.5f, -0.5f);
            TopRightHandlePosition    = new Vector2( 0.5f,  0.5f);
            
            Color = Color.Green;
            PickColor = Color.Magenta;
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
                geometry.VertexLayouts = new List<VertexLayoutDescription>()
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
            var pipelineState = NormalMaterial.CreatePipelineState();
            
            // Create top left box
            {
                var geode = Geode.Create();
                
                geode.AddDrawable(ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    Box.Create(new Vector3(TopLeftHandlePosition.X, 0.0f, TopLeftHandlePosition.Y), 0.05f),
                    hints,
                    new [] {new Vector3(0.0f, 1.0f, 0.0f)}));

                geode.PipelineState = pipelineState;
                geode.PipelineState.RasterizerStateDescription 
                    = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
                AddChild(geode);
                TopLeftHandleNode = geode;
            }
            
            // Create bottom left box
            {
                var geode = Geode.Create();
                
                geode.AddDrawable(ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    Box.Create(new Vector3(BottomLeftHandlePosition.X, 0.0f, BottomLeftHandlePosition.Y), 0.05f),
                    hints,
                    new [] {new Vector3(0.0f, 1.0f, 0.0f)}));

                geode.PipelineState = pipelineState;
                geode.PipelineState.RasterizerStateDescription 
                    = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
                AddChild(geode);
                BottomLeftHandleNode = geode;
            }
            
            // Create bottom right box
            {
                var geode = Geode.Create();
                
                geode.AddDrawable(ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    Box.Create(new Vector3(BottomRightHandlePosition.X, 0.0f, BottomRightHandlePosition.Y), 0.05f),
                    hints,
                    new [] {new Vector3(0.0f, 1.0f, 0.0f)}));

                geode.PipelineState = pipelineState;
                geode.PipelineState.RasterizerStateDescription 
                    = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
                AddChild(geode);
                BottomRightHandleNode = geode;
            }
            
            // Create top right box;
            {
                var geode = Geode.Create();
                
                geode.AddDrawable(ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    Box.Create(new Vector3(TopRightHandlePosition.X, 0.0f, TopRightHandlePosition.Y), 0.05f),
                    hints,
                    new [] {new Vector3(0.0f, 1.0f, 0.0f)}));

                geode.PipelineState = pipelineState;
                geode.PipelineState.RasterizerStateDescription 
                    = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
                AddChild(geode);
                TopRightHandleNode = geode;
            }
        }

    }
}