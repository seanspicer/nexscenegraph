

using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface ITranslatePlaneDragger : ICompositeDragger
    {
        public ITranslate1DDragger Translate1DDragger { get; }
        public ITranslate2DDragger Translate2DDragger { get; }
        void SetColor(Color color);
    }
    
    public class TranslatePlaneDragger : CompositeDragger, ITranslatePlaneDragger
    {
        public ITranslate1DDragger Translate1DDragger { get; protected set; }
        public ITranslate2DDragger Translate2DDragger { get; protected set; }

        protected bool UsingTranslate1DDragger { get; set; } = false;

        public new static ITranslatePlaneDragger Create()
        {
            return new TranslatePlaneDragger(Matrix4x4.Identity);
        }
        
        protected TranslatePlaneDragger(Matrix4x4 matrix) : base(matrix)
        {
            Translate1DDragger = Veldrid.SceneGraph.Manipulators.Translate1DDragger.Create(Vector3.Zero, Vector3.UnitY);
            Translate1DDragger.CheckForNodeInPath = false;
            AddChild(Translate1DDragger);
            DraggerList.Add(Translate1DDragger);

            Translate2DDragger = Veldrid.SceneGraph.Manipulators.Translate2DDragger.Create();
            Translate2DDragger.CheckForNodeInPath = false;
            AddChild(Translate2DDragger);
            DraggerList.Add(Translate2DDragger);
            
            foreach (var dragger in DraggerList)
            {
                dragger.ParentDragger = this;
            }
        }

        public override void SetupDefaultGeometry()
        {
            // Create a polygon
            {
                var vertexArray = new Position3Color4[4];
                vertexArray[0] =
                    new Position3Color4(new Vector3(-0.5f, 0.0f, 0.5f),
                        new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
                vertexArray[1] =
                    new Position3Color4(new Vector3(-0.5f, 0.0f, -0.5f),
                        Vector4.Zero);
                vertexArray[2] =
                    new Position3Color4(new Vector3(0.5f, 0.0f, -0.5f),
                        Vector4.Zero);
                vertexArray[3] =
                    new Position3Color4(new Vector3(0.5f, 0.0f, 0.5f),
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

                AddChild(geode);
            }

            // Create an outline
            {
                var vertexArray = new Position3Color4[4];
                vertexArray[0] =
                    new Position3Color4(new Vector3(-0.5f, 0.0f, 0.5f),
                        Vector4.One);
                vertexArray[1] =
                    new Position3Color4(new Vector3(-0.5f, 0.0f, -0.5f),
                        Vector4.One);
                vertexArray[2] =
                    new Position3Color4(new Vector3(0.5f, 0.0f, -0.5f),
                        Vector4.One);
                vertexArray[3] =
                    new Position3Color4(new Vector3(0.5f, 0.0f, 0.5f),
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

                AddChild(geode);
            }
        }

        public void SetColor(Color color)
        {
            
        }

        public override bool Handle(IPointerInfo pointerInfo, IUiEventAdapter eventAdapter,
            IUiActionAdapter actionAdapter)
        {
            if (!pointerInfo.Contains(this)) return false;

            bool handled = false;
            if (UsingTranslate1DDragger)
            {
                if (Translate1DDragger.Handle(pointerInfo, eventAdapter, actionAdapter))
                {
                    handled = true;
                }
            }
            else
            {
                if (Translate2DDragger.Handle(pointerInfo, eventAdapter, actionAdapter))
                {
                    handled = true;
                }
            }

            return handled;
        }
    }
}