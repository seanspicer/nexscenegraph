using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reactive;
using System.Xml.Schema;
using Veldrid.SceneGraph.Math.IsoSurface;
using Veldrid.SceneGraph.RenderGraph;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.NodeKits.DirectVolumeRendering
{
    public interface ISampledVolumeNode : IGeode
    {
        
    }

    class SamplingVolume : IVoxelVolume
    {
        public double[,,] Values { get; }
        public double[,,] XValues { get; }
        public double[,,] YValues { get; }
        public double[,,] ZValues { get; }

        private IVoxelVolume _source;
        
        public double MinDist { get; private set; }
        public double MaxDist { get; private set; }
        
        internal SamplingVolume(IVoxelVolume source)
        {
            _source = source;

            var xdim = _source.XValues.GetLength(0)-1;
            var ydim = _source.XValues.GetLength(1)-1;
            var zdim = _source.XValues.GetLength(2)-1;
            
            Values = new double[2, 2, 2];
            XValues = new double[2, 2, 2];
            YValues = new double[2, 2, 2];
            ZValues = new double[2, 2, 2];
            
            XValues[0, 0, 0] = _source.XValues[0, 0, 0];
            YValues[0, 0, 0] = _source.YValues[0, 0, 0];
            ZValues[0, 0, 0] = _source.ZValues[0, 0, 0];
            
            XValues[1, 0, 0] = _source.XValues[xdim, 0, 0];
            YValues[1, 0, 0] = _source.YValues[xdim, 0, 0];
            ZValues[1, 0, 0] = _source.ZValues[xdim, 0, 0];
            
            XValues[0, 1, 0] = _source.XValues[0, ydim, 0];
            YValues[0, 1, 0] = _source.YValues[0, ydim, 0];
            ZValues[0, 1, 0] = _source.ZValues[0, ydim, 0];
            
            XValues[0, 0, 1] = _source.XValues[0, 0, zdim];
            YValues[0, 0, 1] = _source.YValues[0, 0, zdim];
            ZValues[0, 0, 1] = _source.ZValues[0, 0, zdim];
            
            XValues[1, 1, 0] = _source.XValues[xdim, ydim, 0];
            YValues[1, 1, 0] = _source.YValues[xdim, ydim, 0];
            ZValues[1, 1, 0] = _source.ZValues[xdim, ydim, 0];
            
            XValues[0, 1, 1] = _source.XValues[0, ydim, zdim];
            YValues[0, 1, 1] = _source.YValues[0, ydim, zdim];
            ZValues[0, 1, 1] = _source.ZValues[0, ydim, zdim];
            
            XValues[1, 0, 1] = _source.XValues[xdim, 0, zdim];
            YValues[1, 0, 1] = _source.YValues[xdim, 0, zdim];
            ZValues[1, 0, 1] = _source.ZValues[xdim, 0, zdim];
            
            XValues[1, 1, 1] = _source.XValues[xdim, ydim, zdim];
            YValues[1, 1, 1] = _source.YValues[xdim, ydim, zdim];
            ZValues[1, 1, 1] = _source.ZValues[xdim, ydim, zdim];
            
            UpdateDistances(Vector3.Zero);
            
        }

        public void UpdateDistances(Vector3 eyePoint)
        {
            MinDist = double.MaxValue;
            MaxDist = double.MinValue;
            
            for (var z = 0; z < 2; ++z)
            {
                for (var y = 0; y < 2; ++y)
                {
                    for (var x = 0; x < 2; ++x)
                    {
                        var dist = System.Math.Abs((new Vector3((float) _source.XValues[x, y, z],
                            (float) _source.YValues[x, y, z], (float) _source.ZValues[x, y, z]) - eyePoint).Length());

                        if (dist < MinDist) MinDist = dist;
                        if (dist > MaxDist) MaxDist = dist;
                        
                        Values[x, y, z] = dist;
                        
                    }
                }
            }
        }
    }
    
    class SamplingCullCallback : Callback, IDrawableCullCallback
    {
        private SampledVolumeNode _svn;
        private IIsoSurfaceGenerator _isoSurfaceGenerator;
        private SamplingVolume _samplingVolume;
        public SamplingCullCallback(SampledVolumeNode svn)
        {
            _svn = svn;
            _samplingVolume = new SamplingVolume(_svn.VoxelVolume);
            _isoSurfaceGenerator = new MarchingCubesIsoSurfaceGenerator();
        }
        
        public bool Cull(ICullVisitor cv, IDrawable drawable)
        {
            if (drawable is Geometry<Position3Color4> geometry)
            {
                _samplingVolume.UpdateDistances(-cv.GetEyeLocal());
                Sample(geometry);
                return false;
            }
            return true;
        }

        private void Sample(Geometry<Position3Color4> geometry)
        {
            var sampleRate = 10;
            var sampleStep = (_samplingVolume.MaxDist - _samplingVolume.MinDist) / sampleRate;
            geometry.PrimitiveSets.Clear();

            List<IIsoSurface> isoSurfaces = new List<IIsoSurface>();
            for (var i = _samplingVolume.MinDist; i <= _samplingVolume.MaxDist; i += sampleStep)
            {
                var isoSurface = _isoSurfaceGenerator.CreateIsoSurface(_samplingVolume, i);
                isoSurfaces.Add(isoSurface);
            }
            
            List<Position3Color4> sliceVertices = new List<Position3Color4>();
            List<uint> indices = new List<uint>();
            var idx = 0u;
            
            foreach (var isoSurface in isoSurfaces)
            {
                var startidx = idx;
                var idxcount = 0u;

                foreach (var vtx in isoSurface.IsoSurfaceVertices)
                {
                    sliceVertices.Add(new Position3Color4(new Vector3((float) vtx.X, (float) vtx.Y, (float) vtx.Z),
                        new Vector4(0.0f, 0.0f, 1.0f, 0.5f)));
                    indices.Add(idx++);
                    ++idxcount;
                }

                if (0 != isoSurface.IsoSurfaceVertices.Count)
                {
                    var pSet = DrawElements<Position3Color4>.Create(
                        geometry,
                        PrimitiveTopology.TriangleList,
                        idxcount,
                        1,
                        startidx,
                        0,
                        0);

                    geometry.PrimitiveSets.Add(pSet);
                }
            }

            if (0 == sliceVertices.Count) return;
            
            geometry.VertexData = sliceVertices.ToArray();
            geometry.IndexData = indices.ToArray();
        }
    }

    public class SampledVolumeNode : Geode, ISampledVolumeNode
    {
        private IVoxelVolume _voxelVolume;
        public IVoxelVolume VoxelVolume => _voxelVolume;

        public static ISampledVolumeNode Create(IVoxelVolume voxelVolume)
        {
            return new SampledVolumeNode(voxelVolume);
        }

        protected SampledVolumeNode(IVoxelVolume voxelVolume)
        {
            _voxelVolume = voxelVolume;
            
            CreateCube(voxelVolume);
            CreateSlices(voxelVolume);
        }

        public override void Accept(INodeVisitor visitor)
        {
            visitor.Apply(this);
        }

        private void CreateSlices(IVoxelVolume voxelVolume)
        {
            var geometry = Geometry<Position3Color4>.Create();

            geometry.VertexLayouts = new List<VertexLayoutDescription>()
            {
                Position3Color4.VertexLayoutDescription
            };
            
            geometry.PipelineState.ShaderSet = Vertex3Color4Shader.Instance.ShaderSet;
            geometry.PipelineState.BlendStateDescription = BlendStateDescription.SingleAlphaBlend;
            geometry.PipelineState.RasterizerStateDescription = RasterizerStateDescription.CullNone;
            geometry.SetCullCallback(new SamplingCullCallback(this));
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