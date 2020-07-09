using System;
using System.Numerics;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.PipelineStates;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;

namespace Examples.Common
{
    public class LargeSphereCountScene
    {
        public static IGroup Build()
        {
            var random = new Random();
            
            var root = Group.Create();
            
            var sphereHints = TessellationHints.Create();
            sphereHints.SetDetailRatio(0.2f);
            var sphereGeode = Geode.Create();
            
            var sphereShape = Sphere.Create(Vector3.Zero, 0.5f);
            
            var sphereDrawable =
                ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    sphereShape,
                    sphereHints,
                    new Vector3[] {new Vector3(1.0f, 0.0f, 0.0f)});

            //sphereGeode.AddDrawable(sphereDrawable);

            var sphereMaterial = PhongMaterial.Create(
            PhongMaterialParameters.Create(
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(1.0f, 1.0f, 1.0f),
                5f),
            PhongPositionalLight.Create( new Vector4(0, 10, 0, 1),PhongLightParameters.Create(
                new Vector3(0.1f, 0.1f, 0.1f),
                new Vector3(1.0f, 1.0f, 1.0f),
                new Vector3(1.0f, 1.0f, 1.0f),
                10000f,
                2)),
            true);

            sphereDrawable.PipelineState = sphereMaterial.CreatePipelineState();
            
            for (var i = 0; i < 5000; ++i)
            {
                var xPos = (float) random.NextDouble() * 100;
                var yPos = (float) random.NextDouble() * 100;

                var xform = MatrixTransform.Create(Matrix4x4.CreateTranslation(xPos, yPos, 0.0f));
                xform.AddChild(sphereDrawable);
                root.AddChild(xform);
            }

            return root;
        }
    }
}