

using System.Collections.Generic;
using System.Numerics;
using SixLabors.ImageSharp.Processing;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface ITabPlaneDragger : ICompositeDragger
    {
        
    }
    
    public class TabPlaneDragger : CompositeDragger, ITabPlaneDragger
    {
        protected ITranslatePlaneDragger TranslateDragger { get; }
        protected IScale2DDragger CornerScaleDragger { get; }
        protected IScale1DDragger HorizontalEdgeScaleDragger { get; }
        protected IScale1DDragger VerticalEdgeScaleDragger { get; }

        protected float HandleScaleFactor { get; set; } = 1.0f;
        
        public static ITabPlaneDragger Create()
        {
            return new TabPlaneDragger(Matrix4x4.Identity);
        }
        
        protected TabPlaneDragger(Matrix4x4 matrix) : base(matrix)
        {
            CornerScaleDragger = Scale2DDragger.Create(IScale2DDragger.ScaleMode.ScaleWithOppositeHandleAsPivot);
            AddChild(CornerScaleDragger);

            HorizontalEdgeScaleDragger =
                Scale1DDragger.Create(IScale1DDragger.ScaleMode.ScaleWithOppositeHandleAsPivot);
            AddChild(HorizontalEdgeScaleDragger);

            VerticalEdgeScaleDragger = 
                Scale1DDragger.Create(IScale1DDragger.ScaleMode.ScaleWithOppositeHandleAsPivot);
            AddChild(VerticalEdgeScaleDragger);

            TranslateDragger = TranslatePlaneDragger.Create();
            TranslateDragger.SetColor(System.Drawing.Color.Gray);
            AddChild(TranslateDragger);
        }

        public override void SetupDefaultGeometry()
        {
            SetupDefaultGeometry(false);
            
        }

        public void SetupDefaultGeometry(bool twoSidedHandle)
        {
            var handleNode = CreateHandleNode(CornerScaleDragger, HandleScaleFactor, twoSidedHandle);
            
            CreateCornerScaleDraggerGeometry(CornerScaleDragger, handleNode, HandleScaleFactor);
            CreateEdgeScaleDraggerGeometry(HorizontalEdgeScaleDragger, VerticalEdgeScaleDragger, handleNode, HandleScaleFactor);
            CreateTranslateDraggerGeometry(CornerScaleDragger, TranslateDragger);
        }

        INode CreateHandleNode(IScale2DDragger cornerScaleDragger, float handleScaleFactor, bool twoSided)
        {
            var vertexArray = new Position3Color3[4];
            vertexArray[0] =
                new Position3Color3(Vector3.Multiply(handleScaleFactor, new Vector3(cornerScaleDragger.TopLeftHandlePosition.X, 0.0f, cornerScaleDragger.TopLeftHandlePosition.Y)),
                    Vector3.UnitY);
            vertexArray[1] =
                new Position3Color3(Vector3.Multiply(handleScaleFactor, new Vector3(cornerScaleDragger.BottomLeftHandlePosition.X, 0.0f, cornerScaleDragger.BottomLeftHandlePosition.Y)),
                    Vector3.UnitY);
            vertexArray[2] =
                new Position3Color3(Vector3.Multiply(handleScaleFactor, new Vector3(cornerScaleDragger.BottomRightHandlePosition.X, 0.0f, cornerScaleDragger.BottomRightHandlePosition.Y)),
                    Vector3.UnitY);
            vertexArray[3] =
                new Position3Color3(Vector3.Multiply(handleScaleFactor, new Vector3(cornerScaleDragger.TopRightHandlePosition.X, 0.0f, cornerScaleDragger.TopRightHandlePosition.Y)),
                    Vector3.UnitY);
            
            var geometry = Geometry<Position3Color3>.Create();
            
            var indexArray = new uint[6];
            indexArray[0] = 0;
            indexArray[1] = 2;
            indexArray[2] = 1;
            indexArray[3] = 0;
            indexArray[4] = 3;
            indexArray[5] = 2;
            
            geometry.IndexData = indexArray;
            geometry.VertexData = vertexArray;
            geometry.VertexLayouts = new List<VertexLayoutDescription>()
            {
                Position3Color3.VertexLayoutDescription
            };

            var pSet = DrawElements<Position3Color3>.Create(
                geometry,
                PrimitiveTopology.TriangleList,
                6,
                1,
                0,
                0,
                0);
            
            geometry.PrimitiveSets.Add(pSet);

            geometry.PipelineState.ShaderSet = Position3Color3Shader.Instance.ShaderSet;

            var geode = Geode.Create();
            geode.NameString = "Dragger Handle";
            geode.AddDrawable(geometry);

            return geode;
        }

        protected INode CreateHandleScene(Vector3 pos, INode handleNode, float handleScaleFactor)
        {
            
            var autoTransform = AutoTransform.Create();
            autoTransform.Position = pos;
            autoTransform.PivotPoint = pos * handleScaleFactor;
            autoTransform.AutoScaleToScreen = true;
            autoTransform.AddChild(handleNode);
            
            var antiSquish = AntiSquish.Create(pos); // Use Pos as pivot
            antiSquish.AddChild(autoTransform);

            return autoTransform;
        }
        
        protected void CreateCornerScaleDraggerGeometry(IScale2DDragger cornerScaleDragger, INode handleNode,
            float handleScaleFactor)
        {
            // Create top left box
            {
                var handleScene =
                    CreateHandleScene(
                        new Vector3(cornerScaleDragger.TopLeftHandlePosition.X, 0.0f,
                            cornerScaleDragger.TopLeftHandlePosition.Y), handleNode, handleScaleFactor);

                cornerScaleDragger.AddChild(handleScene);
                cornerScaleDragger.TopLeftHandleNode = cornerScaleDragger;
            }
            
            // Create bottom left box
            {
                var handleScene =
                    CreateHandleScene(
                        new Vector3(cornerScaleDragger.BottomLeftHandlePosition.X, 0.0f, 
                            cornerScaleDragger.BottomLeftHandlePosition.Y), handleNode, handleScaleFactor);

                cornerScaleDragger.AddChild(handleScene);
                cornerScaleDragger.BottomLeftHandleNode = cornerScaleDragger;
            }
            
            // Create bottom right box
            {
                var handleScene =
                    CreateHandleScene(
                        new Vector3(cornerScaleDragger.BottomRightHandlePosition.X, 0.0f, 
                            cornerScaleDragger.BottomRightHandlePosition.Y), handleNode, handleScaleFactor);

                cornerScaleDragger.AddChild(handleScene);
                cornerScaleDragger.BottomRightHandleNode = cornerScaleDragger;
            }
            
            // Create top right box
            {
                var handleScene =
                    CreateHandleScene(
                        new Vector3(cornerScaleDragger.TopRightHandlePosition.X, 0.0f, 
                            cornerScaleDragger.TopRightHandlePosition.Y), handleNode, handleScaleFactor);

                cornerScaleDragger.AddChild(handleScene);
                cornerScaleDragger.TopRightHandleNode = cornerScaleDragger;
            }
        }

        protected void CreateEdgeScaleDraggerGeometry(
            IScale1DDragger horzEdgeScaleDragger,
            IScale1DDragger vertEdgeScaleDragger, 
            INode handleNode,
            float handleScaleFactor)
        {
            // Create left box
            {
                var handleScene =
                    CreateHandleScene(
                        new Vector3(horzEdgeScaleDragger.LeftHandlePosition, 0.0f, 0.0f), handleNode,
                        handleScaleFactor);

                horzEdgeScaleDragger.AddChild(handleScene);
                horzEdgeScaleDragger.LeftHandleNode = handleScene;
            }
            
            // Create right box
            {
                var handleScene =
                    CreateHandleScene(
                        new Vector3(horzEdgeScaleDragger.RightHandlePosition, 0.0f, 0.0f), handleNode,
                        handleScaleFactor);

                horzEdgeScaleDragger.AddChild(handleScene);
                horzEdgeScaleDragger.RightHandleNode = handleScene;
            }
            
            // Create top box
            {
                var handleScene =
                    CreateHandleScene(
                        new Vector3(vertEdgeScaleDragger.LeftHandlePosition, 0.0f, 0.0f), handleNode,
                        handleScaleFactor);

                vertEdgeScaleDragger.AddChild(handleScene);
                vertEdgeScaleDragger.LeftHandleNode = handleScene;
            }
            
            // Create bottom box
            {
                var handleScene =
                    CreateHandleScene(
                        new Vector3(vertEdgeScaleDragger.RightHandlePosition, 0.0f, 0.0f), handleNode,
                        handleScaleFactor);

                vertEdgeScaleDragger.AddChild(handleScene);
                vertEdgeScaleDragger.RightHandleNode = handleScene;
            }

            var rotation = QuaternionExtensions.MakeRotate(Vector3.UnitX, Vector3.UnitZ);
            vertEdgeScaleDragger.Matrix = Matrix4x4.CreateFromQuaternion(rotation);
        }

        protected void CreateTranslateDraggerGeometry(IScale2DDragger cornerScaleDragger,
            ITranslatePlaneDragger translate2DDragger)
        {
            // Create a polygon
            {
                var vertexArray = new Position3Color3[4];
                vertexArray[0] =
                    new Position3Color3(new Vector3(cornerScaleDragger.TopLeftHandlePosition.X, 0.0f, cornerScaleDragger.TopLeftHandlePosition.Y),
                        Vector3.One);
                vertexArray[1] =
                    new Position3Color3(new Vector3(cornerScaleDragger.BottomLeftHandlePosition.X, 0.0f, cornerScaleDragger.BottomLeftHandlePosition.Y),
                        Vector3.One);
                vertexArray[2] =
                    new Position3Color3(new Vector3(cornerScaleDragger.BottomRightHandlePosition.X, 0.0f, cornerScaleDragger.BottomRightHandlePosition.Y),
                        Vector3.One);
                vertexArray[3] =
                    new Position3Color3(new Vector3(cornerScaleDragger.TopRightHandlePosition.X, 0.0f, cornerScaleDragger.TopRightHandlePosition.Y),
                        Vector3.One);
                
                var geometry = Geometry<Position3Color3>.Create();
            
                var indexArray = new uint[6];
                indexArray[0] = 0;
                indexArray[1] = 1;
                indexArray[2] = 2;
                indexArray[3] = 0;
                indexArray[4] = 2;
                indexArray[5] = 3;
            
                geometry.IndexData = indexArray;
                geometry.VertexData = vertexArray;
                geometry.VertexLayouts = new List<VertexLayoutDescription>()
                {
                    Position3Color3.VertexLayoutDescription
                };

                var pSet = DrawElements<Position3Color3>.Create(
                    geometry,
                    PrimitiveTopology.TriangleList,
                    6,
                    1,
                    0,
                    0,
                    0);
            
                geometry.PrimitiveSets.Add(pSet);

                geometry.PipelineState.ShaderSet = Position3Color3Shader.Instance.ShaderSet;
                geometry.PipelineState.RasterizerStateDescription = new RasterizerStateDescription(FaceCullMode.None,
                    PolygonFillMode.Wireframe, FrontFace.Clockwise, true, false);
                
                var geode = Geode.Create();
                geode.NameString = "Dragger Dragger Translate Plane";
                geode.AddDrawable(geometry);

                translate2DDragger.AddChild(geode);
            }
        }
    }
}