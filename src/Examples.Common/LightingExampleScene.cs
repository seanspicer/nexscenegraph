using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.IO;
using Veldrid.SceneGraph.PipelineStates;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;
using Math = System.Math;

namespace Examples.Common
{
    internal class UpdateCallback : NodeCallback, INodeCallback
    {
        private DateTime _lastTime;
        private readonly IPhongMaterial _material;
        private readonly Random _random;

        internal UpdateCallback(IPhongMaterial material)
        {
            _random = new Random();
            _lastTime = DateTime.Now;
            _material = material;
        }

        public override bool Run(IObject obj, IObject data)
        {
            var curTime = DateTime.Now;

            if ((curTime - _lastTime).TotalMilliseconds > 1000)
            {
                var color = new Vector3((float) _random.NextDouble(), (float) _random.NextDouble(),
                    (float) _random.NextDouble());

                _material.SetMaterial(color, color, Vector3.One, 20);
                _lastTime = curTime;
            }


            return true;
        }
    }

    public class LightingExampleScene
    {
        public static IGroup Build()
        {
            var root = Group.Create();

            var model = CreateDragonModel();

            var geometryFactory = GeometryFactory.Create();

            var cube = geometryFactory.CreateCube(VertexType.Position3Texture2Color3Normal3,
                TopologyType.IndexedTriangleList);

            // Shape Drawables
            var cubeShape = Box.Create(new Vector3(0f, 0.0f, 0.0f), 0.5f * Vector3.One);
            var hints = TessellationHints.Create();
            hints.NormalsType = NormalsType.PerVertex;
            hints.ColorsType = ColorsType.ColorPerVertex;

            var freq = (float) (2 * Math.PI / 9);
            var cubeColors = MakeColorGradient(freq, freq, freq, 0, 2, 4, 8);

            var cubeDrawable =
                ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    cubeShape,
                    hints,
                    cubeColors.ToArray());

            var cube2 = Geode.Create();
            cube2.AddDrawable(cubeDrawable);

            var sphereShape = Sphere.Create(Vector3.Zero, 0.5f);
            var sphereHints = TessellationHints.Create();
            sphereHints.SetDetailRatio(1.6f);

            var sphereDrawable =
                ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    sphereShape,
                    sphereHints,
                    new[] {new Vector3(1.0f, 0.0f, 0.0f)});

            var sphere = Geode.Create();
            sphere.AddDrawable(sphereDrawable);


            var cubeXForm = MatrixTransform.Create(Matrix4x4.CreateScale(10f, 10f, 10f));
            cubeXForm.AddChild(sphere);

            var quat = QuaternionExtensions.MakeRotate(Vector3.UnitY, -Vector3.UnitY);
            var cubeXForm2 = MatrixTransform.Create(Matrix4x4.CreateScale(10f, 10f, 10f)
                .PostMultiply(Matrix4x4.CreateFromQuaternion(quat)));
            cubeXForm2.AddChild(cube2);

            var at = AutoTransform.Create();
            at.AutoScaleToScreen = true;
            at.AddChild(cubeXForm2);

            var leftTop = MatrixTransform.Create(Matrix4x4.CreateTranslation(-10f, 10f, 0f));
            var rightTop = MatrixTransform.Create(Matrix4x4.CreateTranslation(10f, 10f, 0f));

            var leftBottom = MatrixTransform.Create(Matrix4x4.CreateTranslation(-10f, -10f, 0f));
            var rightBottom = MatrixTransform.Create(Matrix4x4.CreateTranslation(10f, -10f, 0f));

            leftTop.AddChild(model);

            rightTop.AddChild(model);

            leftBottom.AddChild(cubeXForm);
            rightBottom.AddChild(cubeXForm2);

            var flatYellowMaterial = PhongMaterial.Create(
                PhongMaterialParameters.Create(
                    new Vector3(1.0f, 1.0f, 0.0f),
                    new Vector3(1.0f, 1.0f, 0.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    20),
                PhongPositionalLight.Create(new Vector4(0, 100, 0, 1), PhongLightParameters.Create(
                    new Vector3(0.2f, 0.2f, 0.2f),
                    new Vector3(0.2f, 0.2f, 0.2f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    300f,
                    1)));

            var shinyRedGoldMaterial = PhongMaterial.Create(
                PhongMaterialParameters.Create(
                    new Vector3(1.0f, 1.0f, 0.0f),
                    new Vector3(1.0f, 1.0f, 0.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    5),
                PhongHeadlight.Create(PhongLightParameters.Create(
                    new Vector3(0.1f, 0.1f, 0.1f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    30f,
                    1)),
                false);

            var cubeMaterial = PhongMaterial.Create(
                PhongMaterialParameters.Create(
                    new Vector3(1.0f, 1.0f, 1.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    50f),
                PhongHeadlight.Create(PhongLightParameters.Create(
                    new Vector3(0.5f, 0.5f, 0.5f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    1f,
                    0)),
                false);

            var sphereMaterial = PhongMaterial.Create(
                PhongMaterialParameters.Create(
                    new Vector3(0.0f, 0.0f, 1.0f),
                    new Vector3(0.0f, 0.0f, 1.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    5f),
                PhongPositionalLight.Create(new Vector4(0, 10, 0, 1), PhongLightParameters.Create(
                    new Vector3(0.1f, 0.1f, 0.1f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    10000f,
                    2)));

            leftTop.SetUpdateCallback(new UpdateCallback(flatYellowMaterial));
            leftTop.PipelineState = flatYellowMaterial.CreatePipelineState();
            rightTop.PipelineState = shinyRedGoldMaterial.CreatePipelineState();
            sphere.PipelineState = sphereMaterial.CreatePipelineState();
            cube2.PipelineState = cubeMaterial.CreatePipelineState();
//            rightTop.PipelineState = CreateHeadlightState(
//                new Vector3(1.0f, 1.0f, 0.0f), 
//                50,
//                Vector3.One,
//                5);

            var sceneGroup = Group.Create();
            sceneGroup.AddChild(leftTop);
            sceneGroup.AddChild(rightTop);
            sceneGroup.AddChild(leftBottom);
            sceneGroup.AddChild(rightBottom);

            root.AddChild(sceneGroup);

            return root;
        }

        public static Stream OpenEmbeddedAssetStream(string name)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
        }

        public static byte[] ReadEmbeddedAssetBytes(string name)
        {
            var asm = Assembly.GetExecutingAssembly();
            var names = asm.GetManifestResourceNames();

            using (var stream = OpenEmbeddedAssetStream(name))
            {
                var bytes = new byte[stream.Length];
                using (var ms = new MemoryStream(bytes))
                {
                    stream.CopyTo(ms);
                    return bytes;
                }
            }
        }

        private static IGeode CreateDragonModel()
        {
            IGeode result;

            using (var dragonModelStream = OpenEmbeddedAssetStream(@"Examples.Common.Assets.Models.chinesedragon.dae"))
            {
                var importer = new Import();
                result = importer.LoadColladaModel(dragonModelStream);
            }

            return result;
        }

        private static List<Vector3> MakeColorGradient(float frequency1, float frequency2, float frequency3,
            float phase1, float phase2, float phase3, uint len)
        {
            var center = 128;
            var width = 127;

            var result = new List<Vector3>();

            for (var i = 0; i < len; ++i)
            {
                var red = (float) Math.Sin(frequency1 * i + phase1) * width + center;
                var grn = (float) Math.Sin(frequency2 * i + phase2) * width + center;
                var blu = (float) Math.Sin(frequency3 * i + phase3) * width + center;

                result.Add(new Vector3(red / 255f, grn / 255f, blu / 255f));
            }

            return result;
        }
    }
}