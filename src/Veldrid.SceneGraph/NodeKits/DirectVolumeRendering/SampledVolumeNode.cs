using System.Collections.Generic;
using System.Numerics;
using Veldrid.SceneGraph.Math.IsoSurface;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.NodeKits.DirectVolumeRendering
{
    public interface ISampledVolumeNode : IGeode
    {
        
    }
    
    public class SampledVolumeNode : Geode, ISampledVolumeNode
    {
        public new static ISampledVolumeNode Create(IVoxelVolume voxelVolume)
        {
            return new SampledVolumeNode(voxelVolume);
        }

        protected SampledVolumeNode(IVoxelVolume voxelVolume)
        {
            CreateCube(voxelVolume);
            CreateSlices(voxelVolume);
        }

        public override void Accept(INodeVisitor visitor)
        {
            visitor.Apply(this);
        }

        private void CreateSlices(IVoxelVolume voxelVolume)
        {
            var isoSurfaceGenerator = new MarchingCubesIsoSurfaceGenerator();

            List<IIsoSurface> isoSurfaces = new List<IIsoSurface>();
            for (var i = 0; i < 40; ++i)
            {
                var isoSurface = isoSurfaceGenerator.CreateIsoSurface(voxelVolume, 0.25*i);
                isoSurfaces.Add(isoSurface);
            }
            

            List<Position3Color3> sliceVertices = new List<Position3Color3>();
            List<uint> indices = new List<uint>();
            var idx = 0u;
            foreach (var isoSurface in isoSurfaces)
            {
                foreach (var vtx in isoSurface.IsoSurfaceVertices)
                {
                    sliceVertices.Add(new Position3Color3(new Vector3((float)vtx.X, (float)vtx.Y, (float)vtx.Z), Vector3.UnitX));
                    indices.Add(idx++);
                }
            }


            if (0 == sliceVertices.Count) return;
            
            var geometry = Geometry<Position3Color3>.Create();
            
            geometry.VertexData = sliceVertices.ToArray();
            
            geometry.IndexData = indices.ToArray();

            geometry.VertexLayouts = new List<VertexLayoutDescription>()
            {
                Position3Color3.VertexLayoutDescription
            };

            var pSet = DrawElements<Position3Color3>.Create(
                geometry,
                PrimitiveTopology.TriangleList,
                (uint)indices.Count,
                1,
                0,
                0,
                0);

            geometry.PrimitiveSets.Add(pSet);

            geometry.PipelineState.ShaderSet = Position3Color3Shader.Instance.ShaderSet;
            geometry.PipelineState.RasterizerStateDescription = RasterizerStateDescription.CullNone;
            
            AddDrawable(geometry);
        }
        
        private void CreateCube(IVoxelVolume vv)
        {
            var xDim = vv.XValues.GetLength(0);
            var yDim = vv.XValues.GetLength(1);
            var zDim = vv.XValues.GetLength(2);
            
            Vector3[] cubeVertices =
            {
                new Vector3((float)vv.XValues[0,      0,      0     ], (float)vv.YValues[0,      0,      0     ], (float)vv.ZValues[0,      0,      0     ]),  
                new Vector3((float)vv.XValues[xDim-1, 0,      0     ], (float)vv.YValues[xDim-1, 0,      0     ], (float)vv.ZValues[xDim-1, 0,      0     ]), 
                new Vector3((float)vv.XValues[xDim-1, yDim-1, 0     ], (float)vv.YValues[xDim-1, yDim-1, 0     ], (float)vv.ZValues[xDim-1, yDim-1, 0     ]),
                new Vector3((float)vv.XValues[0     , yDim-1, 0     ], (float)vv.YValues[0     , yDim-1, 0     ], (float)vv.ZValues[0     , yDim-1, 0     ]),
                new Vector3((float)vv.XValues[0,      0,      zDim-1], (float)vv.YValues[0,      0,      zDim-1], (float)vv.ZValues[0,      0,      zDim-1]),  
                new Vector3((float)vv.XValues[xDim-1, 0,      zDim-1], (float)vv.YValues[xDim-1, 0,      zDim-1], (float)vv.ZValues[xDim-1, 0,      zDim-1]), 
                new Vector3((float)vv.XValues[xDim-1, yDim-1, zDim-1], (float)vv.YValues[xDim-1, yDim-1, zDim-1], (float)vv.ZValues[xDim-1, yDim-1, zDim-1]),
                new Vector3((float)vv.XValues[0     , yDim-1, zDim-1], (float)vv.YValues[0     , yDim-1, zDim-1], (float)vv.ZValues[0     , yDim-1, zDim-1])
            };

            var cubeTriangleVertices = new List<Position3Color3>();
            var cubeTriangleIndices = new List<uint>();

            uint[] indicies =
            {
                0, 1, 2, 3, 0,
                4, 5, 6, 7, 4,
                0, 1, 5, 4, 0,
                2, 6, 7, 3, 2,
                0, 4, 7, 3, 2,
                1, 5, 6, 2, 1
            };

            Vector3[] normals =
            {
                -Vector3.UnitZ,
                Vector3.UnitZ,
                -Vector3.UnitY,
                Vector3.UnitY,
                -Vector3.UnitX,
                Vector3.UnitX
            };

            foreach (var t in cubeVertices)
            {
                cubeTriangleVertices.Add(new Position3Color3(
                    t, Vector3.UnitX));
            }

            var vertexData = cubeTriangleVertices.ToArray();

            for (uint i = 0; i < 6; ++i)
            {
                var geometry = Geometry<Position3Color3>.Create();

                geometry.VertexData = vertexData;

                geometry.IndexData = indicies;

                geometry.VertexLayouts = new List<VertexLayoutDescription>()
                {
                    Position3Color3.VertexLayoutDescription
                };

                var pSet = DrawElements<Position3Color3>.Create(
                    geometry,
                    PrimitiveTopology.LineStrip,
                    5,
                    1,
                    5u * i,
                    0,
                    0);

                geometry.PrimitiveSets.Add(pSet);

                geometry.PipelineState.ShaderSet = Position3Color3Shader.Instance.ShaderSet;

                // Set a cull callback that will cull based on the plane normal.
                //geometry.SetCullCallback(new PlaneCullCalback(normals[i]));

                this.AddDrawable(geometry);

            }
        }
    }
}