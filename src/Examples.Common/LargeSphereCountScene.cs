using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
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
            
            public InstanceData(Vector3 position, Vector3 scale)
            {
                Position = position;
                Scale = scale;
            }
        }
        
        
        public static IGroup Build()
        {
            var random = new Random();
            
            var root = Group.Create();
            
            var sphereHints = TessellationHints.Create();
            sphereHints.SetDetailRatio(0.2f);
            var sphereGeode = Geode.Create();
            var sphereShape = Sphere.Create(Vector3.Zero, 0.5f);

            var INSTANCE_COUNT = 5000;
            var instanceData = new InstanceData[INSTANCE_COUNT];
            for (var i = 0; i < INSTANCE_COUNT; ++i)
            {
                var xPos = (float) random.NextDouble() * 100;
                var yPos = (float) random.NextDouble() * 100;

                instanceData[i] = new InstanceData(new Vector3(xPos, yPos, 0.0f), Vector3.One );
            }

            var sphereUniform = Uniform<InstanceData>.Create("InstanceData",
                BufferUsage.UniformBuffer | BufferUsage.Dynamic,
                ShaderStages.Vertex | ShaderStages.Fragment);
            sphereUniform.UniformData = instanceData;
            
            var sphereDrawable =
                ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    sphereShape,
                    sphereHints,
                    new Vector3[] {new Vector3(1.0f, 0.0f, 0.0f)});

            //sphereGeode.AddDrawable(sphereDrawable);

            var sphereMaterial = InstancedSphereMaterial.Create(
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

            var pipelineState = sphereMaterial.CreatePipelineState();
            pipelineState.AddUniform(sphereUniform);

            sphereDrawable.PipelineState = pipelineState;
            
            for (var i = 0; i < 10; ++i)
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