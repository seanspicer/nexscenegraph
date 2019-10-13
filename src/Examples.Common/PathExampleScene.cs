using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.PipelineStates;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;

namespace Examples.Common
{
    public class PathExampleScene
    {
        public static IGroup Build()
        {
            var root = Group.Create();

            var path = Path.Create(new Vector3[]
            {
                new Vector3(0.0f,  1.0f, 0.0f),
                new Vector3(0.0f,  0.0f, 0.0f), 
                new Vector3(1.0f,  -1.0f, 0.0f),
                //new Vector3(2.0f, -2.0f, 2.0f), 
            });

            var hints = TessellationHints.Create();
            hints.SetDetailRatio(4.5f);
            hints.SetRadius(0.1f);
            hints.CreatePathAsLine = false;
            var pathDrawable = ShapeDrawable<Position3Texture2Color3Normal3>.Create(path, hints);
            
            var redMaterial = PhongMaterial.Create(
                PhongMaterialParameters.Create(
                    new Vector3(1.0f, 0.0f, 0.0f),
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(0.0f, 0.0f, 0.0f),
                    5f),
                PhongPositionalLight.Create( new Vector4(0, 10, 0, 1),PhongLightParameters.Create(
                    new Vector3(1.0f, 1.0f, 1.0f),
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(0.0f, 0.0f, 0.0f),
                    1f,
                    0)),
                true);

            var pathGeode = Geode.Create();
            pathGeode.AddDrawable(pathDrawable);
            pathGeode.PipelineState = redMaterial.CreatePipelineState();

            root.AddChild(pathGeode);
            return root;
        }
    }
}