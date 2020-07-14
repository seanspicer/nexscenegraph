using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.PipelineStates;
using Veldrid.SceneGraph.Shaders;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;

namespace Examples.Common
{
    public class LargeSphereCountScene
    {
        public struct InstanceData
        {
            public static uint Size { get; } = (uint)Unsafe.SizeOf<InstanceData>();

            public Vector3 Position;
            public Vector3 Scale;
            public float Visibility;
            
            public InstanceData(Vector3 position, Vector3 scale)
            {
                Position = position;
                Scale = scale;
                Visibility = 1.0f;
            }
        }
        
        
        public static IGroup Build()
        {
            var random = new Random();
            
            var root = Group.Create();
            
            var sphereHints = TessellationHints.Create();
            sphereHints.SetDetailRatio(0.3f);
            var sphereGeode = Geode.Create();
            var sphereShape = Sphere.Create(Vector3.Zero, 0.01f);

            var INSTANCE_COUNT = 50000u;
            var instanceData = new InstanceData[INSTANCE_COUNT];
            for (var i = 0; i < INSTANCE_COUNT; ++i)
            {
                var xPos = -50f+(float) random.NextDouble() * 100;
                var yPos = -50f+(float) random.NextDouble() * 100;
                var zPos = -50f + (float) random.NextDouble() * 100;
                
                var scale = (float)(20f * random.NextDouble());
                
                instanceData[i] = new InstanceData(new Vector3(xPos, yPos, zPos), scale*Vector3.One );
                //instanceData[i] = new InstanceData(Vector3.Zero, Vector3.One );
            }

            var sphereInstanceData = VertexBuffer<InstanceData>.Create();
            sphereInstanceData.VertexData = instanceData;
            
            var vertexLayoutPerInstance = new VertexLayoutDescription(
                new VertexElementDescription("InstancePosition", VertexElementSemantic.TextureCoordinate,
                    VertexElementFormat.Float3),
                new VertexElementDescription("InstanceScale", VertexElementSemantic.TextureCoordinate,
                    VertexElementFormat.Float3),
                new VertexElementDescription("InstanceVisible", VertexElementSemantic.TextureCoordinate,
                    VertexElementFormat.Float1)
            );
            vertexLayoutPerInstance.InstanceStepRate = 1;

            var sphereDrawable =
                ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    sphereShape,
                    sphereHints,
                    new Vector3[] {new Vector3(1.0f, 0.0f, 0.0f)},
                    INSTANCE_COUNT);

            sphereDrawable.VertexLayouts.Add(vertexLayoutPerInstance);
            sphereDrawable.InstanceVertexBuffer = sphereInstanceData;
            sphereDrawable.SetFixedBoundingBox(BoundingBox.Create(-50,-50,-10,50, 50, 10));
            
            
            var sphereMaterial = InstancedSphereMaterial.Create(
            PhongMaterialParameters.Create(
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, 0.0f),
                5f),
            PhongHeadlight.Create(PhongLightParameters.Create(
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(1.0f, 1.0f, 1.0f),
                new Vector3(1.0f, 1.0f, 1.0f),
                1f,
                0)),
            true);

            var pipelineState = sphereMaterial.CreatePipelineState();
            //pipelineState.AddVertexBuffer(sphereInstanceData);

            sphereDrawable.PipelineState = pipelineState;
            root.AddChild(sphereDrawable);
            
            // for (var i = 0; i < 10; ++i)
            // {
            //     var xPos = (float) random.NextDouble() * 100;
            //     var yPos = (float) random.NextDouble() * 100;
            //
            //     var xform = MatrixTransform.Create(Matrix4x4.CreateTranslation(xPos, yPos, 0.0f));
            //     xform.AddChild(sphereDrawable);
            //     root.AddChild(xform);
            // }

            return root;
        }
        
    }

    public class InstancedSphereMaterial : PhongMaterial
    {
        public new static IPhongMaterial Create(IPhongMaterialParameters p, PhongLight light0, bool overrideColor = true)
        {
            return new InstancedSphereMaterial(p, light0, overrideColor);
        }
        
        private InstancedSphereMaterial(IPhongMaterialParameters p, PhongLight light0, bool overrideColor) 
            : base(p, light0, overrideColor)
        {
        }
        
        public override IPipelineState CreatePipelineState()
        {
            var pso = PipelineState.Create();
            var vtxShader =
                new ShaderDescription(ShaderStages.Vertex, ReadEmbeddedAssetBytes(@"Examples.Common.Assets.Shaders.LargeSphereCount-vertex.glsl"), "main");
            
            var frgShader =
                new ShaderDescription(ShaderStages.Fragment, ReadEmbeddedAssetBytes(@"Examples.Common.Assets.Shaders.LargeSphereCount-fragment.glsl"), "main");

            pso.ShaderSet = ShaderSet.Create("PhongShader", vtxShader, frgShader);

            pso.AddUniform(CreateLightSourceUniform());
            pso.AddUniform(CreateMaterialUniform());
            
            return pso;
        }
    }
}