//
// This file is part of IMAGEFrac (R) and related technologies.
//
// Copyright (c) 2017-2020 Reveal Energy Services.  All Rights Reserved.
//
// LEGAL NOTICE:
// IMAGEFrac contains trade secrets and otherwise confidential information
// owned by Reveal Energy Services. Access to and use of this information is 
// strictly limited and controlled by the Company. This file may not be copied,
// distributed, or otherwise disclosed outside of the Company's facilities 
// except under appropriate precautions to maintain the confidentiality hereof, 
// and may not be used in any way not expressly authorized by the Company.
//

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.PipelineStates;
using Veldrid.SceneGraph.Shaders;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;

namespace Examples.Common
{
    public class UpdatingInstancesExampleScene
    {
        public struct InstanceData
        {
            public static uint Size { get; } = (uint)Unsafe.SizeOf<UpdatingInstancesExampleScene.InstanceData>();

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
        
        
        internal class UpdateCallback : INodeCallback
        {
            private IVertexBuffer<UpdatingInstancesExampleScene.InstanceData> _vb;

            private Random _random;

            private DateTime _lastTime;

            private int _idx;
            
            internal UpdateCallback(IVertexBuffer<UpdatingInstancesExampleScene.InstanceData> vb)
            {
                _vb = vb;
                _random = new Random();
                _lastTime = DateTime.Now;
                _idx = 0;
            }
            
            public bool Run(IObject obj, IObject data)
            {
                var curTime = DateTime.Now;

                if ((curTime - _lastTime).TotalMilliseconds > 6)
                {
                    if (_idx >= _vb.VertexData.Length)
                    {
                        _idx = 0;
                    }
                    
                    if (_vb.VertexData[_idx].Scale.X > 0.5)
                    {
                        _vb.VertexData[_idx].Scale = 0.3f * Vector3.One;
                    }
                    else if (_vb.VertexData[_idx].Visibility > 0.5)
                    {
                        _vb.VertexData[_idx].Visibility = 0.0f;
                    }
                    else
                    {
                        _vb.VertexData[_idx].Scale = 1.0f * Vector3.One;
                        _vb.VertexData[_idx].Visibility = 1.0f;
                    }

                    _idx++;

                    _vb.SetDirty();
                }
                return true;
            }
        }
        
        public static IGroup Build()
        {
            var random = new Random();
            
            var root = Group.Create();
            
            var shapeHints = TessellationHints.Create();
            shapeHints.SetDetailRatio(0.3f);
            var cubeGeode = Geode.Create();
            var cubeShape = Box.Create(Vector3.Zero, 0.2f*Vector3.One);

            var INSTANCE_BOUNDS = 5.0;
            var INSTANCE_COUNT = 20.0;
            var INSTANCE_SPACING = 2*INSTANCE_BOUNDS / INSTANCE_COUNT;
            
            var instanceData = new UpdatingInstancesExampleScene.InstanceData[(int)(INSTANCE_COUNT*INSTANCE_COUNT*INSTANCE_COUNT)];
            for (var i = 0; i < INSTANCE_COUNT; ++i)
            {
                for (var j = 0; j < INSTANCE_COUNT; ++j)
                {
                    for (var k = 0; k < INSTANCE_COUNT; ++k)
                    {
                        var xPos = (float) (-INSTANCE_BOUNDS + (float) i*INSTANCE_SPACING);
                        var yPos = (float) (-INSTANCE_BOUNDS + (float) j*INSTANCE_SPACING);
                        var zPos = (float) (-INSTANCE_BOUNDS + (float) k*INSTANCE_SPACING);
                
                        var scale = 1;
                
                        instanceData[i*(int)(INSTANCE_COUNT*INSTANCE_COUNT) + j*(int)(INSTANCE_COUNT) + k] = new UpdatingInstancesExampleScene.InstanceData(new Vector3(xPos, yPos, zPos), scale*Vector3.One );
                    }
                }
            }
            
            var cubeInstanceData = VertexBuffer<UpdatingInstancesExampleScene.InstanceData>.Create();
            cubeInstanceData.VertexData = instanceData;
            
            var vertexLayoutPerInstance = new VertexLayoutDescription(
                new VertexElementDescription("InstancePosition", VertexElementSemantic.TextureCoordinate,
                    VertexElementFormat.Float3),
                new VertexElementDescription("InstanceScale", VertexElementSemantic.TextureCoordinate,
                    VertexElementFormat.Float3),
                new VertexElementDescription("InstanceVisible", VertexElementSemantic.TextureCoordinate,
                    VertexElementFormat.Float1)
                );
            vertexLayoutPerInstance.InstanceStepRate = 1;

            var cubeDrawable =
                ShapeDrawable<Position3Texture2Color3Normal3>.Create(
                    cubeShape,
                    shapeHints,
                    new Vector3[] {new Vector3(1.0f, 0.0f, 0.0f)},
                    (uint)(INSTANCE_COUNT*INSTANCE_COUNT*INSTANCE_COUNT));

            cubeDrawable.VertexLayouts.Add(vertexLayoutPerInstance);
            cubeDrawable.InstanceVertexBuffer = cubeInstanceData;
            cubeDrawable.InitialBoundingBox = BoundingBox.Create(
                (float)-INSTANCE_BOUNDS,
                (float)-INSTANCE_BOUNDS,
                (float)-INSTANCE_BOUNDS,
                (float)INSTANCE_BOUNDS, 
                (float)INSTANCE_BOUNDS, 
                (float)INSTANCE_BOUNDS);
            cubeDrawable.SetUpdateCallback(new UpdateCallback(cubeInstanceData));
            cubeDrawable.CullingActive = false;
            
            var cubeMaterial = InstancedSphereMaterial.Create(
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

            var pipelineState = cubeMaterial.CreatePipelineState();
            //pipelineState.AddVertexBuffer(sphereInstanceData);

            cubeDrawable.PipelineState = pipelineState;
            root.AddChild(cubeDrawable);
            
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
}