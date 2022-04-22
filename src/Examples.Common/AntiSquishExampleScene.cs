using System.Numerics;
using SixLabors.Fonts;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.PipelineStates;
using Veldrid.SceneGraph.Text;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;

namespace Examples.Common
{
    public class AntiSquishExampleScene
    {
        public static IGroup Build()
        {
            var root = Group.Create();

            var pathCoords = new[]
            {
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 0.0f, -15.0f),
                new Vector3(5.0f, 0.0f, -20.0f),
                new Vector3(20.0f, 0.0f, -20.0f)
            };

            // Change the sceneScale here.
            var sceneScale = Matrix4x4.CreateScale(1, 1, 4);

            var path = Path.Create(pathCoords);

            //////////////////////////////////////////////////////////
            // MAIN PATH - GREEN
            //////////////////////////////////////////////////////////

            var hints = TessellationHints.Create();
            hints.SetDetailRatio(4f);
            hints.SetRadius(.1f);
            var pathDrawable = ShapeDrawable<Position3Texture2Color3Normal3>.Create(path, hints);
            var pathGeode = Geode.Create();
            pathGeode.AddDrawable(pathDrawable);

            // This is the main path
            var XForm = MatrixTransform.Create(sceneScale);
            XForm.PipelineState = GetPipelineState(0.0f, 1.0f, 0.0f);
            XForm.PipelineState.RasterizerStateDescription
                = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true,
                    false);
            XForm.AddChild(pathGeode);

            // Move the text to the bottom of the well
            var noSquishText = CreateText("No AntiSqiush");
            var wellBottomXlate
                = MatrixTransform.Create(Matrix4x4.CreateTranslation(20.0f, 0.0f, -20.0f));
            wellBottomXlate.AddChild(noSquishText);
            XForm.AddChild(wellBottomXlate);

            //////////////////////////////////////////////////////////
            // ANTISQUISH PATH - RED
            //////////////////////////////////////////////////////////

            var scaledPath = Path.Create(pathCoords, sceneScale);
            var scaledPathDrawable = ShapeDrawable<Position3Texture2Color3Normal3>.Create(scaledPath, hints);
            var scaledPathGeode = Geode.Create();
            scaledPathGeode.AddDrawable(scaledPathDrawable);

            // This moves the other well sideways
            var xlatMatrix = Matrix4x4.CreateTranslation(0, 10, 0);
            var Xlate = MatrixTransform.Create(xlatMatrix);

            Xlate.PipelineState = GetPipelineState(1.0f, 0.0f, 0.0f);
            Xlate.PipelineState.RasterizerStateDescription
                = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true,
                    false);

            Xlate.AddChild(scaledPathGeode);

            var antiSquishText = CreateText("AntiSquish");
            var wellBottomXlateAntiSquish
                = MatrixTransform.Create(Matrix4x4.CreateTranslation(20.0f, 10.0f, -20.0f));


            var unsquish = AntiSquish.Create();
            unsquish.ReferenceFrame = Transform.ReferenceFrameType.Absolute;
            unsquish.AddChild(antiSquishText);

            wellBottomXlateAntiSquish.AddChild(unsquish);
            XForm.AddChild(wellBottomXlateAntiSquish);

            root.AddChild(XForm);
            root.AddChild(Xlate);
            return root;
        }

        private static IMatrixTransform CreateText(string text)
        {
            var textNode = TextNode.Create(text, SystemFonts.CreateFont("Arial", 10), padding: 5, fontResolution: 3,
                horizontalAlignment: HorizontalAlignment.Left);
            textNode.AutoRotateToScreen = false;
            textNode.CharacterSizeMode = CharacterSizeModes.ObjectCoords;

            var geod = Geode.Create();
            geod.AddDrawable(textNode);

            var rotateText = MatrixTransform.Create(Matrix4x4.CreateRotationX(1.5708f)); //90 degree
            rotateText.AddChild(geod);

            return rotateText;
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
                    0)));

            return redMaterial.CreatePipelineState();
        }
    }
}