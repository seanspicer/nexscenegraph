using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reactive;
using System.Xml.Schema;
using SharpDX.Direct3D11;
using Veldrid.SceneGraph.Math.IsoSurface;
using Veldrid.SceneGraph.RenderGraph;
using Veldrid.SceneGraph.Shaders;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.NodeKits.DirectVolumeRendering
{
    public interface ISampledVolumeNode : IGeode
    {
        
    }

    internal class Edge : IComparable<Edge>
    {
        public IVertex3D V1;
        public IVertex3D V2;

        public Edge(IVertex3D v1, IVertex3D v2)
        {
            V1 = v1;
            V2 = v2;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Edge);
        }

        public override int GetHashCode()
        {
            return 1;
        }

        public bool Equals(Edge other)
        {
            // If parameter is null, return false.
            if (Object.ReferenceEquals(other, null))
            {
                return false;
            }
            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }
            if (this.GetType() != other.GetType())
            {
                return false;
            }
            
            var tol = 1e-6;

            if (System.Math.Abs(V1.X - other.V1.X) < tol &&
                System.Math.Abs(V1.Y - other.V1.Y) < tol &&
                System.Math.Abs(V1.Z - other.V1.Z) < tol &&
                System.Math.Abs(V2.X - other.V2.X) < tol &&
                System.Math.Abs(V2.Y - other.V2.Y) < tol &&
                System.Math.Abs(V2.Z - other.V2.Z) < tol)
            {
                return true;
            }
            
            if (System.Math.Abs(V1.X - other.V2.X) < tol &&
                System.Math.Abs(V1.Y - other.V2.Y) < tol &&
                System.Math.Abs(V1.Z - other.V2.Z) < tol &&
                System.Math.Abs(V2.X - other.V1.X) < tol &&
                System.Math.Abs(V2.Y - other.V1.Y) < tol &&
                System.Math.Abs(V2.Z - other.V1.Z) < tol)
            {
                return true;
            }

            return false;
        }
        
        public int CompareTo(Edge other)
        {
            return this.Equals(other) ? 0 : 1;
        }
        
        public static bool operator ==(Edge lhs, Edge rhs)
        {
            // Check for null on left side.
            if (Object.ReferenceEquals(lhs, null))
            {
                if (Object.ReferenceEquals(rhs, null))
                {
                    // null == null = true.
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Edge lhs, Edge rhs)
        {
            return !(lhs == rhs);
        }
    }
    
    class SamplingVolume : IVoxelVolume
    {
        public double[,,] Values { get; }
        public double[,,] XValues { get; }
        public double[,,] YValues { get; }
        public double[,,] ZValues { get; }

        public double XMin => XValues[0, 0, 0];
        public double XMax => XValues[1, 1, 1];
        
        public double YMin => YValues[0, 0, 0];
        public double YMax => YValues[1, 1, 1];
        
        public double ZMin => ZValues[0, 0, 0];
        public double ZMax => ZValues[1, 1, 1];

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

        public Vector3 TexGen(Vector3 point)
        {
            return new Vector3(
                (float)((point.X - XMin) / (XMax - XMin)),
                (float)((point.Y - YMin) / (YMax - YMin)),
                (float)((point.Z - ZMin) / (ZMax - ZMin))
                );
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
                        var pos = new Vector3(
                            (float) XValues[x, y, z],
                            (float) YValues[x, y, z],
                            (float) ZValues[x, y, z]);
                        
                        var dist = System.Math.Abs((pos - eyePoint).Length());

                        if (dist < MinDist) MinDist = dist;
                        if (dist > MaxDist) MaxDist = dist;
                        
                        Values[x, y, z] = dist;
                        
                    }
                }
            }
        }
    }
    
    class SamplingCullCallback : DrawableCullCallback, IDrawableCullCallback
    {
        private SampledVolumeNode _svn;
        private IIsoSurfaceGenerator _isoSurfaceGenerator;
        private SamplingVolume _samplingVolume;
        private bool _samplingPlanesDirty = true;
        private bool _outlinesDirty = true;
        private List<IIsoSurface> _isoSurfaces;

        private Vector3 _eyeLocal { get; set; }
        
        public SamplingCullCallback(SampledVolumeNode svn)
        {
            _svn = svn;
            _samplingVolume = new SamplingVolume(_svn.VoxelVolume);
            _isoSurfaceGenerator = new MarchingCubesIsoSurfaceGenerator();
            _isoSurfaces = new List<IIsoSurface>();
        }
        
        public override bool Cull(INodeVisitor nodeVisitorv, IDrawable drawable)
        {
            if (nodeVisitorv is ICullVisitor cv)
            {
                var eyeLocal = cv.GetEyeLocal();
                if (System.Math.Abs(eyeLocal.X - _eyeLocal.X) < 1e-5 &&
                    System.Math.Abs(eyeLocal.Y - _eyeLocal.Y) < 1e-5 &&
                    System.Math.Abs(eyeLocal.Z - _eyeLocal.Z) < 1e-5)
                {
                }
                else
                {
                    _samplingPlanesDirty = true;
                    _outlinesDirty = true;
                    _eyeLocal = cv.GetEyeLocal();
                    _samplingVolume.UpdateDistances(_eyeLocal);
                    BuildIsoSurfaces();
                }
            
                if (drawable is Geometry<Position3TexCoord3Color4> samplePlaneGeometry)
                {
                    if (_samplingPlanesDirty)
                    {
                        UpdateSamplingPlanes(samplePlaneGeometry);
                        _samplingPlanesDirty = false;
                    }
                
                    return false;
                }

                if (drawable is Geometry<Position3Color3> outlineGeometry)
                {
                    if (_outlinesDirty)
                    {
                        UpdateOutlines(outlineGeometry);
                        _outlinesDirty = false;
                    }
                
                    return false;
                }
                return true;
            }

            return true;

        }

        private void BuildIsoSurfaces()
        {
            var sampleRate = 100;
            var sampleStep = (_samplingVolume.MaxDist - _samplingVolume.MinDist) / sampleRate;
            
            _isoSurfaces.Clear();
            for (var i = _samplingVolume.MinDist; i <= _samplingVolume.MaxDist; i += sampleStep)
            {
                var isoSurface = _isoSurfaceGenerator.CreateIsoSurface(_samplingVolume, i);
                _isoSurfaces.Add(isoSurface);
            }
        }

        private void UpdateSamplingPlanes(Geometry<Position3TexCoord3Color4> geometry)
        {

            geometry.PrimitiveSets.Clear();
            List<Position3TexCoord3Color4> sliceVertices = new List<Position3TexCoord3Color4>();
            List<uint> indices = new List<uint>();
            var idx = 0u;
            
            var surfNo = 0f;
            
            foreach (var isoSurface in _isoSurfaces)
            {
                var startidx = idx;
                var idxcount = 0u;

                // foreach (var vtx in isoSurface.IsoSurfaceVertices)
                // {
                //     var pos = new Vector3((float) vtx.X, (float) vtx.Y, (float) vtx.Z);
                //     var texCoord = _samplingVolume.TexGen(pos);
                //     var color = new Vector4(0.0f, 0.0f, surfNo/5f, 1.0f);
                //     
                //     sliceVertices.Add(new Position3TexCoord3Color4(pos, texCoord, color));
                //     indices.Add(idx++);
                //     ++idxcount;
                // }
                
                var nTris = isoSurface.IsoSurfaceVertices.Count / 3;
                
                var currentVertex = isoSurface.IsoSurfaceVertices.First;
                for (var i = 0; i < nTris; ++i)
                {
                    var v1 = currentVertex.Value;
                    currentVertex = currentVertex.Next ??
                                    throw new Exception("Invalid number of vertices in isosurface");
                    var v2 = currentVertex.Value;
                    currentVertex = currentVertex.Next ??
                                    throw new Exception("Invalid number of vertices in isosurface");
                    var v3 = currentVertex.Value;

                    if (i < nTris - 1)
                    {
                        currentVertex = currentVertex.Next ??
                                        throw new Exception("Invalid number of vertices in isosurface");
                    }
                    
                    var color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
                    
                    var p1 = new Vector3((float) v1.X, (float) v1.Y, (float) v1.Z);
                    var p2 = new Vector3((float) v2.X, (float) v2.Y, (float) v2.Z);
                    var p3 = new Vector3((float) v3.X, (float) v3.Y, (float) v3.Z);
                    
                    var texCoord1 = _samplingVolume.TexGen(p1);
                    indices.Add(idx++);
                    ++idxcount;
                    
                    var texCoord2 = _samplingVolume.TexGen(p2);
                    indices.Add(idx++);
                    ++idxcount;
                    
                    var texCoord3 = _samplingVolume.TexGen(p3);
                    indices.Add(idx++);
                    ++idxcount;
                    
                    var c1 = p2 - p1;
                    var c2 = p3 - p2;
                    var cx = Vector3.Cross(c1, c2);
                    if (Vector3.Dot(cx, _eyeLocal) > 0)
                    {
                        sliceVertices.Add(new Position3TexCoord3Color4(p1, texCoord1, color));
                        sliceVertices.Add(new Position3TexCoord3Color4(p2, texCoord2, color));
                        sliceVertices.Add(new Position3TexCoord3Color4(p3, texCoord3, color));
                    }
                    else
                    {
                        sliceVertices.Add(new Position3TexCoord3Color4(p1, texCoord1, color));
                        sliceVertices.Add(new Position3TexCoord3Color4(p3, texCoord3, color));
                        sliceVertices.Add(new Position3TexCoord3Color4(p2, texCoord2, color));
                    }
                }

                if (0 != isoSurface.IsoSurfaceVertices.Count)
                {
                    var pSet = DrawElements<Position3TexCoord3Color4>.Create(
                        geometry,
                        PrimitiveTopology.TriangleList,
                        idxcount,
                        1,
                        startidx,
                        0,
                        0);

                    geometry.PrimitiveSets.Add(pSet);
                    
                }

                surfNo = surfNo + 1;
            }

            if (0 == sliceVertices.Count) return;
            
            geometry.VertexData = sliceVertices.ToArray();
            geometry.IndexData = indices.ToArray();
        }
        
        private void UpdateOutlines(Geometry<Position3Color3> geometry)
        {

            geometry.PrimitiveSets.Clear();
            List<Position3Color3> sliceVertices = new List<Position3Color3>();
            List<uint> indices = new List<uint>();
            var idx = 0u;
            
            foreach (var isoSurface in _isoSurfaces)
            {
                var startidx = idx;
                var idxcount = 0u;

                var nTris = isoSurface.IsoSurfaceVertices.Count / 3;

                var edgeSet = new HashSet<Edge>();
                
                var currentVertex = isoSurface.IsoSurfaceVertices.First;
                for (var i = 0; i < nTris; ++i)
                {
                    var v1 = currentVertex.Value;
                    currentVertex = currentVertex.Next ?? throw new Exception("Invalid number of vertices in isosurface");
                    var v2 = currentVertex.Value;
                    currentVertex = currentVertex.Next ?? throw new Exception("Invalid number of vertices in isosurface");
                    var v3 = currentVertex.Value;

                    if (i < nTris - 1)
                    {
                        currentVertex = currentVertex.Next ?? throw new Exception("Invalid number of vertices in isosurface");
                    }

                    var e1 = new Edge(v1, v2);
                    if (false == edgeSet.Add(e1))
                    {
                        edgeSet.Remove(e1);
                    }
                    
                    var e2 = new Edge(v2, v3);
                    if (false == edgeSet.Add(e2))
                    {
                        edgeSet.Remove(e2);
                    }
                    
                    var e3 = new Edge(v1, v3);
                    if (false == edgeSet.Add(e3))
                    {
                        edgeSet.Remove(e3);
                    }
                    
                    
                }
                
                foreach (var edge in edgeSet)
                {
                    var pos = new Vector3((float) edge.V1.X, (float) edge.V1.Y, (float) edge.V1.Z);
                    var color = new Vector3(0.0f, 0.0f, 1.0f);
                    sliceVertices.Add(new Position3Color3(pos, color));
                    indices.Add(idx++);
                    ++idxcount;
                    
                    pos = new Vector3((float) edge.V2.X, (float) edge.V2.Y, (float) edge.V2.Z);
                    color = new Vector3(0.0f, 1.0f, 0.0f);
                    sliceVertices.Add(new Position3Color3(pos, color));
                    
                    indices.Add(idx++);
                    ++idxcount;
                }

                if (0 != isoSurface.IsoSurfaceVertices.Count)
                {
                    var pSet = DrawElements<Position3Color3>.Create(
                        geometry,
                        PrimitiveTopology.LineList,
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

        private IShaderSet _customShaderSet;

        public static ISampledVolumeNode Create(IVoxelVolume voxelVolume, IShaderSet shaderSet = null)
        {
            return new SampledVolumeNode(voxelVolume, shaderSet);
        }

        protected SampledVolumeNode(IVoxelVolume voxelVolume, IShaderSet shaderSet=null)
        {
            _voxelVolume = voxelVolume;
            _customShaderSet = shaderSet;
            
            CreateCube(voxelVolume);
            CreateSlices(voxelVolume);
        }

        public override void Accept(INodeVisitor visitor)
        {
            visitor.Apply(this);
        }

        private void CreateSlices(IVoxelVolume voxelVolume)
        {
            var samplingCullCallback = new SamplingCullCallback(this);
            
            var geometry = Geometry<Position3TexCoord3Color4>.Create();
            
            geometry.VertexLayouts = new List<VertexLayoutDescription>()
            {
                Position3TexCoord3Color4.VertexLayoutDescription
            };
            
            geometry.PipelineState.ShaderSet = _customShaderSet ?? Position3TexCoord3Color4Shader.Instance.ShaderSet;
            geometry.PipelineState.BlendStateDescription = BlendStateDescription.SingleAlphaBlend;
            geometry.PipelineState.RasterizerStateDescription = new RasterizerStateDescription
            {
                CullMode = FaceCullMode.Back,
                FillMode = PolygonFillMode.Solid,
                DepthClipEnabled = false,
                FrontFace = FrontFace.CounterClockwise
            };
            geometry.SetCullCallback(samplingCullCallback);
            AddDrawable(geometry);
            
            var sliceOutlineGeometry = Geometry<Position3Color3>.Create();

            sliceOutlineGeometry.VertexLayouts = new List<VertexLayoutDescription>()
            {
                Position3Color3.VertexLayoutDescription
            };
            
            sliceOutlineGeometry.PipelineState.ShaderSet = Position3Color3Shader.Instance.ShaderSet;
            sliceOutlineGeometry.PipelineState.RasterizerStateDescription = RasterizerStateDescription.CullNone;
            sliceOutlineGeometry.SetCullCallback(samplingCullCallback);
            
            //AddDrawable(sliceOutlineGeometry);
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