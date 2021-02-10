

using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Processing;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.PipelineStates;
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
        
        public new static ITabPlaneDragger Create()
        {
            return new TabPlaneDragger(Matrix4x4.Identity);
        }
        
        protected TabPlaneDragger(Matrix4x4 matrix) : base(matrix)
        {
            CornerScaleDragger = Scale2DDragger.Create(IScale2DDragger.ScaleMode.ScaleWithOppositeHandleAsPivot, false);
            CornerScaleDragger.NameString = "Corner Scale Dragger";
            AddChild(CornerScaleDragger);
            DraggerList.Add(CornerScaleDragger);

            HorizontalEdgeScaleDragger =
                Scale1DDragger.Create(IScale1DDragger.ScaleMode.ScaleWithOppositeHandleAsPivot, false);
            HorizontalEdgeScaleDragger.NameString = "Horizontal Edge Scale Dragger";
            AddChild(HorizontalEdgeScaleDragger);
            DraggerList.Add(HorizontalEdgeScaleDragger);
            
            VerticalEdgeScaleDragger = 
                Scale1DDragger.Create(IScale1DDragger.ScaleMode.ScaleWithOppositeHandleAsPivot, false);
            VerticalEdgeScaleDragger.NameString = "Vertical Edge Scale Dragger";
            AddChild(VerticalEdgeScaleDragger);
            DraggerList.Add(VerticalEdgeScaleDragger);

            TranslateDragger = TranslatePlaneDragger.Create();
            TranslateDragger.NameString = "Translate Dragger";
            TranslateDragger.SetColor(System.Drawing.Color.Gray);
            AddChild(TranslateDragger);
            DraggerList.Add(TranslateDragger);

            foreach (var dragger in DraggerList)
            {
                dragger.ParentDragger = this;
            }
            
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
            var vertexArray = new Position3Texture2Color3Normal3[4];
            vertexArray[0] =
                new Position3Texture2Color3Normal3(
                    Vector3.Multiply(handleScaleFactor, new Vector3(cornerScaleDragger.TopLeftHandlePosition.X, 0.0f, cornerScaleDragger.TopLeftHandlePosition.Y)),
                    Vector2.Zero,
                    Vector3.UnitY,
                    Vector3.UnitY);
            vertexArray[1] =
                new Position3Texture2Color3Normal3(
                    Vector3.Multiply(handleScaleFactor, new Vector3(cornerScaleDragger.BottomLeftHandlePosition.X, 0.0f, cornerScaleDragger.BottomLeftHandlePosition.Y)),
                    Vector2.Zero,
                    Vector3.UnitY,
                    Vector3.UnitY);
            vertexArray[2] =
                new Position3Texture2Color3Normal3(
                    Vector3.Multiply(handleScaleFactor, new Vector3(cornerScaleDragger.BottomRightHandlePosition.X, 0.0f, cornerScaleDragger.BottomRightHandlePosition.Y)),
                    Vector2.Zero,
                    Vector3.UnitY,
                    Vector3.UnitY);
            vertexArray[3] =
                new Position3Texture2Color3Normal3(
                    Vector3.Multiply(handleScaleFactor, new Vector3(cornerScaleDragger.TopRightHandlePosition.X, 0.0f, cornerScaleDragger.TopRightHandlePosition.Y)),
                    Vector2.Zero,
                    Vector3.UnitY,
                    Vector3.UnitY);
            
            var geometry = Geometry<Position3Texture2Color3Normal3>.Create();
            
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
                Position3Texture2Color3Normal3.VertexLayoutDescription
            };

            var pSet = DrawElements<Position3Texture2Color3Normal3>.Create(
                geometry,
                PrimitiveTopology.TriangleList,
                6,
                1,
                0,
                0,
                0);
            
            geometry.PrimitiveSets.Add(pSet);
            
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

            return antiSquish;
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
                cornerScaleDragger.TopLeftHandleNode = handleScene;
            }
            
            // Create bottom left box
            {
                var handleScene =
                    CreateHandleScene(
                        new Vector3(cornerScaleDragger.BottomLeftHandlePosition.X, 0.0f, 
                            cornerScaleDragger.BottomLeftHandlePosition.Y), handleNode, handleScaleFactor);

                cornerScaleDragger.AddChild(handleScene);
                cornerScaleDragger.BottomLeftHandleNode = handleScene;
            }
            
            // Create bottom right box
            {
                var handleScene =
                    CreateHandleScene(
                        new Vector3(cornerScaleDragger.BottomRightHandlePosition.X, 0.0f, 
                            cornerScaleDragger.BottomRightHandlePosition.Y), handleNode, handleScaleFactor);

                cornerScaleDragger.AddChild(handleScene);
                cornerScaleDragger.BottomRightHandleNode = handleScene;
            }
            
            // Create top right box
            {
                var handleScene =
                    CreateHandleScene(
                        new Vector3(cornerScaleDragger.TopRightHandlePosition.X, 0.0f, 
                            cornerScaleDragger.TopRightHandlePosition.Y), handleNode, handleScaleFactor);

                cornerScaleDragger.AddChild(handleScene);
                cornerScaleDragger.TopRightHandleNode = handleScene;
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
                var vertexArray = new Position3Color4[4];
                vertexArray[0] =
                    new Position3Color4(new Vector3(cornerScaleDragger.TopLeftHandlePosition.X, 0.0f, cornerScaleDragger.TopLeftHandlePosition.Y),
                        new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
                vertexArray[1] =
                    new Position3Color4(new Vector3(cornerScaleDragger.BottomLeftHandlePosition.X, 0.0f, cornerScaleDragger.BottomLeftHandlePosition.Y),
                        Vector4.Zero);
                vertexArray[2] =
                    new Position3Color4(new Vector3(cornerScaleDragger.BottomRightHandlePosition.X, 0.0f, cornerScaleDragger.BottomRightHandlePosition.Y),
                        Vector4.Zero);
                vertexArray[3] =
                    new Position3Color4(new Vector3(cornerScaleDragger.TopRightHandlePosition.X, 0.0f, cornerScaleDragger.TopRightHandlePosition.Y),
                        Vector4.Zero);
                
                var geometry = Geometry<Position3Color4>.Create();
            
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
                    Position3Color4.VertexLayoutDescription
                };

                var pSet = DrawElements<Position3Color4>.Create(
                    geometry,
                    PrimitiveTopology.TriangleList,
                    6,
                    1,
                    0,
                    0,
                    0);
            
                geometry.PrimitiveSets.Add(pSet);

                geometry.PipelineState.ShaderSet = Vertex3Color4Shader.Instance.ShaderSet;
                geometry.PipelineState.RasterizerStateDescription = new RasterizerStateDescription(FaceCullMode.None,
                    PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
                geometry.PipelineState.BlendStateDescription = BlendStateDescription.SingleAlphaBlend;
                
                
                var geode = Geode.Create();
                geode.NameString = "Dragger Dragger Translate Plane";
                geode.AddDrawable(geometry);

                translate2DDragger.AddChild(geode);
            }
            
            // Create an outline
            {
                var vertexArray = new Position3Color4[4];
                vertexArray[0] =
                    new Position3Color4(new Vector3(cornerScaleDragger.TopLeftHandlePosition.X, 0.0f, cornerScaleDragger.TopLeftHandlePosition.Y),
                        Vector4.One);
                vertexArray[1] =
                    new Position3Color4(new Vector3(cornerScaleDragger.BottomLeftHandlePosition.X, 0.0f, cornerScaleDragger.BottomLeftHandlePosition.Y),
                        Vector4.One);
                vertexArray[2] =
                    new Position3Color4(new Vector3(cornerScaleDragger.BottomRightHandlePosition.X, 0.0f, cornerScaleDragger.BottomRightHandlePosition.Y),
                        Vector4.One);
                vertexArray[3] =
                    new Position3Color4(new Vector3(cornerScaleDragger.TopRightHandlePosition.X, 0.0f, cornerScaleDragger.TopRightHandlePosition.Y),
                        Vector4.One);
                
                var geometry = Geometry<Position3Color4>.Create();
            
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
                    Position3Color4.VertexLayoutDescription
                };

                var pSet = DrawElements<Position3Color4>.Create(
                    geometry,
                    PrimitiveTopology.LineStrip,
                    5,
                    1,
                    0,
                    0,
                    0);
            
                geometry.PrimitiveSets.Add(pSet);

                geometry.PipelineState.ShaderSet = Vertex3Color4Shader.Instance.ShaderSet;
                geometry.PipelineState.RasterizerStateDescription = new RasterizerStateDescription(FaceCullMode.None,
                    PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
                
                
                var geode = Geode.Create();
                geode.NameString = "Dragger Dragger Translate Plane outline";
                geode.AddDrawable(geometry);

                translate2DDragger.AddChild(geode);
            }
        }

        public override bool Handle(IPointerInfo pointerInfo, IUiEventAdapter eventAdapter,
            IUiActionAdapter actionAdapter)
        {
            if (!pointerInfo.Contains(this)) return false;
            
            // Since the translate plane and the handleNode lie on the same plane the hit could've been on either one. But we
            // need to handle the scaling draggers before the translation. Check if the node path has the scaling nodes else
            // check for the scaling nodes in next hit.
            if (CornerScaleDragger.Handle(pointerInfo, eventAdapter, actionAdapter))
            {
                return true;
            }

            if (HorizontalEdgeScaleDragger.Handle(pointerInfo, eventAdapter, actionAdapter))
            {
                return true;
            }

            if (VerticalEdgeScaleDragger.Handle(pointerInfo, eventAdapter, actionAdapter))
            {
                return true;
            }

             var nextPointer = Veldrid.SceneGraph.Manipulators.PointerInfo.Create(pointerInfo);
             nextPointer.Next();
            
             // Run through the other hits to clear them
             while (false == nextPointer.Completed())
             {
                 if (CornerScaleDragger.Handle(nextPointer, eventAdapter, actionAdapter))
                 {
                     return true;
                 }

                 if (HorizontalEdgeScaleDragger.Handle(nextPointer, eventAdapter, actionAdapter))
                 {
                     return true;
                 }

                 if (VerticalEdgeScaleDragger.Handle(nextPointer, eventAdapter, actionAdapter))
                 {
                     return true;
                 }
                 nextPointer.Next();
             }

             if (TranslateDragger.Handle(pointerInfo, eventAdapter, actionAdapter))
             {
                 return true;
             }

            return false;
        }
    }
}