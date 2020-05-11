using System.Numerics;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.PipelineStates;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;

namespace Examples.Common
{
    public class AntiSquishExampleScene
    {
        public static IGroup Build()
        {
            var root = Group.Create();

            var pathCoords = new Vector3[]
            {
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 0.0f, -15.0f),
                new Vector3(5.0f, 0.0f, -20.0f),
                new Vector3(20.0f, 0.0f, -20.0f),
            };
            
            var path = Path.Create(pathCoords);
            var scaledPath = Path.Create(pathCoords, Matrix4x4.CreateScale(1, 1, 2));
            
            
            var hints = TessellationHints.Create();
            hints.SetDetailRatio(4f);
            hints.SetRadius(.1f);
            var pathDrawable = ShapeDrawable<Position3Texture2Color3Normal3>.Create(path, hints);
            var pathGeode = Geode.Create();
            pathGeode.AddDrawable(pathDrawable);
            
            var scaledPathDrawable = ShapeDrawable<Position3Texture2Color3Normal3>.Create(scaledPath, hints);
            var scaledPathGeode = Geode.Create();
            scaledPathGeode.AddDrawable(scaledPathDrawable);

            var Xlate = MatrixTransform.Create(Matrix4x4.CreateTranslation(0, 10, 0));
            Xlate.PipelineState = GetPipelineState(1.0f, 0.0f, 0.0f);
            Xlate.PipelineState.RasterizerStateDescription 
                = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
            
            Xlate.AddChild(scaledPathGeode);
            
            
            var XForm = MatrixTransform.Create(Matrix4x4.CreateScale(1, 1, 2));

            //var antiSquish = AntiSquish.Create();
            XForm.PipelineState = GetPipelineState(0.0f, 1.0f, 0.0f);
            XForm.PipelineState.RasterizerStateDescription 
                = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
            XForm.AddChild(pathGeode);
            
            //antiSquish.AddChild(pathGeode);
            
            //XForm.AddChild(antiSquish);
            //XForm.AddChild(Xlate);

            root.AddChild(XForm);
            root.AddChild(Xlate);
            return root;
        }

        private static IPipelineState GetPipelineState(float r, float g, float b)
        {
            var redMaterial = PhongMaterial.Create(
                PhongMaterialParameters.Create(
                    new Vector3(r, g, b),
                    new Vector3(r, g, b),
                    new Vector3(r, g, b),
                    5f),
                PhongHeadlight.Create(PhongLightParameters.Create(
                    new Vector3(0.1f, 0.1f, 0.1f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    1f,
                    0)),
                true);

            return redMaterial.CreatePipelineState();
        }
    }
}