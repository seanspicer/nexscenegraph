

using System.Collections.Generic;
using System.Numerics;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface ITranslate2DDragger : IDragger
    {
        
    }
    
    public class Translate2DDragger : Base2DDragger, ITranslate2DDragger
    {
        public new static ITranslate2DDragger Create()
        {
            return new Translate2DDragger(Matrix4x4.Identity);
        }
        
        protected Translate2DDragger(Matrix4x4 matrix) : base(matrix)
        {
            
        }

        public override void SetupDefaultGeometry()
        {
            var lineStart = Vector3.Multiply(Vector3.UnitZ, -0.5f);
            var lineEnd = Vector3.Multiply(Vector3.UnitZ, 0.5f);
            
            // Create a Line
            var lineGeode = Geode.Create();
            {
                var geometry = Geometry<Position3Color3>.Create();
                var vertexArray = new Position3Color3[2];
                vertexArray[0] = new Position3Color3(lineStart, Vector3.One);
                vertexArray[1] = new Position3Color3(lineEnd, Vector3.One);

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
            
            //AddChild(lineGeode);
            var hints = TessellationHints.Create();
            hints.ColorsType = ColorsType.ColorOverall;

            var pipelineState = NormalMaterial.CreatePipelineState();
            
            var geode = Geode.Create();
            
            // Create a left Cone
            {
                var cone = Cone.Create(lineStart, 0.025f, 0.1f);
                cone.Rotation = QuaternionExtensions.MakeRotate(Vector3.Multiply(Vector3.UnitZ, -1.0f), Vector3.UnitZ);
                
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
                var cone = Cone.Create(lineEnd, 0.025f, 0.1f);
                //cone.Rotation = QuaternionExtensions.MakeRotate(Vector3.UnitZ, Vector3.Multiply(Vector3.UnitZ, -1.0f));
                geode.AddDrawable(ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    cone,
                    hints,
                    new [] {new Vector3(0.0f, 1.0f, 0.0f)}));

                geode.PipelineState = pipelineState;
                geode.PipelineState.RasterizerStateDescription 
                    = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
                AddChild(geode);
            }

            var planeNormal = Vector3.Normalize(PlaneProjector.Plane.Normal);
            var planeRotation = QuaternionExtensions.MakeRotate(new Vector3(0.0f, 1.0f, 0.0f), planeNormal);
            var xform = MatrixTransform.Create(Matrix4x4.CreateFromQuaternion(planeRotation));
            
            // Create an arrow in the XAxis
            {
                var rot = QuaternionExtensions.MakeRotate(new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f));
                var arrow = MatrixTransform.Create(Matrix4x4.CreateFromQuaternion(rot));
                arrow.AddChild(lineGeode);
                arrow.AddChild(geode);
                xform.AddChild(arrow);
            }
            
            // Create an arrow in the XAxis
            {
                var arrow = Group.Create();
                arrow.AddChild(lineGeode);
                arrow.AddChild(geode);
                xform.AddChild(arrow);
            }

            AddChild(xform);
        }
    }
}