//
// Copyright 2018-2021 Sean Spicer 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.SceneGraph.Math.IsoSurface;
using Veldrid.SceneGraph.RenderGraph;
using Veldrid.SceneGraph.Shaders;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.NodeKits.DirectVolumeRendering
{
    public interface ILevoyCabralTechnique : IVolumeTechnique
    {
    }

    public class LevoyCabralTechnique : VolumeTechnique, ILevoyCabralTechnique, ILocator.ILocatorCallback
    {
        protected IShaderSet ShaderSet { get; private set; }
        
        protected INode Node { get; set; }
        
        protected IVoxelVolume VoxelVolume { get; set; }
        protected IGeometry<Position3TexCoord3Color4> Geometry { get; set; }
        protected IGeometry<Position3Color3> OutlinesGeometry { get; set; }
        
        private IIsoSurfaceGenerator _isoSurfaceGenerator;
        private ILevoyCabralLocator _levoyCabralLocator;
        private bool _dirty = true;
        private bool _samplingPlanesDirty = true;
        private bool _outlinesDirty = true;
        private List<IIsoSurface> _isoSurfaces;

        private Vector3 _eyeLocal { get; set; }
        
        public static ILevoyCabralTechnique Create(IShaderSet shaderSet = null)
        {
            return new LevoyCabralTechnique(shaderSet);
        }

        protected LevoyCabralTechnique(IShaderSet shaderSet)
        {
            ShaderSet = shaderSet;
            _isoSurfaceGenerator = new MarchingCubesIsoSurfaceGenerator();
            _isoSurfaces = new List<IIsoSurface>();
        }
        
        public override void Init()
        {
            if (_volumeTile?.Layer == null)
            {
                return;
            }

            if (_volumeTile.Layer is IVoxelVolumeLayer voxelVolumeLayer && _volumeTile.Locator is ILevoyCabralLocator levoyCabralLocator)
            {
                VoxelVolume = voxelVolumeLayer.VoxelVolume;
                _levoyCabralLocator = levoyCabralLocator;
                _levoyCabralLocator.AddCallback(this);
                
                // Create the Geometry Placeholder
                Node = CreateSlices();
            }
            else
            {
                throw new NotImplementedException("The Levoy-Cabral Technique only works for voxel volumes");
            }
        }

        public override void Update(IUpdateVisitor nv)
        {
            // Nothing to do here
        }

        public override void Cull(ICullVisitor cv)
        {
            UpdateGeometry(cv);

            Node?.Accept(cv);
        }

        private void UpdateGeometry(ICullVisitor cv)
        {
            // Only work on voxel volumes (for now)
            if (null == VoxelVolume || null == Geometry) return;

            var eyeLocal = cv.GetEyeLocal();
            if ((System.Math.Abs(eyeLocal.X - _eyeLocal.X) < 1e-5 &&
                System.Math.Abs(eyeLocal.Y - _eyeLocal.Y) < 1e-5 &&
                System.Math.Abs(eyeLocal.Z - _eyeLocal.Z) < 1e-5) &&
                false == _dirty)
            {
            }
            else
            {
                _samplingPlanesDirty = true;
                _outlinesDirty = true;
                _eyeLocal = cv.GetEyeLocal();
                _levoyCabralLocator.UpdateDistances(_eyeLocal);
                BuildIsoSurfaces();
                _dirty = false;
            }
            
            if (_samplingPlanesDirty)
            {
                UpdateSamplingPlanes();
                _samplingPlanesDirty = false;
            }
            
            if (_outlinesDirty)
            {
                UpdateOutlines();
                _outlinesDirty = false;
            }
            
        }

        protected IGeode CreateSlices()
        {
            var geode = Geode.Create();
            
            Geometry = Geometry<Position3TexCoord3Color4>.Create();
            
            Geometry.VertexLayouts = new List<VertexLayoutDescription>()
            {
                Position3TexCoord3Color4.VertexLayoutDescription
            };
            
            Geometry.PipelineState.ShaderSet = ShaderSet ?? Position3TexCoord3Color4Shader.Instance.ShaderSet;
            Geometry.PipelineState.BlendStateDescription = BlendStateDescription.SingleAlphaBlend;
            Geometry.PipelineState.RasterizerStateDescription = new RasterizerStateDescription
            {
                CullMode = FaceCullMode.Back,
                FillMode = PolygonFillMode.Solid,
                DepthClipEnabled = false,
                FrontFace = FrontFace.CounterClockwise
            };
            
            geode.AddDrawable(Geometry);
            return geode;
        }
        
        private void BuildIsoSurfaces()
        {
            // TODO enable changing this.
            var sampleRate = 100;
            var sampleStep = (_levoyCabralLocator.MaxDist - _levoyCabralLocator.MinDist) / sampleRate;
            
            _isoSurfaces.Clear();
            for (var i = _levoyCabralLocator.MinDist; i <= _levoyCabralLocator.MaxDist; i += sampleStep)
            {
                var isoSurface = _isoSurfaceGenerator.CreateIsoSurface(_levoyCabralLocator, i);
                _isoSurfaces.Add(isoSurface);
            }
        }
        
        private void UpdateSamplingPlanes()
        {
            if (null == Geometry) return;
            
            Geometry.PrimitiveSets.Clear();
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
                    
                    var texCoord1 = _levoyCabralLocator.TexGen(p1);
                    indices.Add(idx++);
                    ++idxcount;
                    
                    var texCoord2 = _levoyCabralLocator.TexGen(p2);
                    indices.Add(idx++);
                    ++idxcount;
                    
                    var texCoord3 = _levoyCabralLocator.TexGen(p3);
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
                        Geometry,
                        PrimitiveTopology.TriangleList,
                        idxcount,
                        1,
                        startidx,
                        0,
                        0);

                    Geometry.PrimitiveSets.Add(pSet);
                    
                }

                surfNo = surfNo + 1;
            }

            if (0 == sliceVertices.Count) return;
            
            Geometry.VertexData = sliceVertices.ToArray();
            Geometry.IndexData = indices.ToArray();
        }
        
        private void UpdateOutlines()
        {
            if (null == OutlinesGeometry) return;
            
            OutlinesGeometry.PrimitiveSets.Clear();
            List<Position3Color3> sliceVertices = new List<Position3Color3>();
            List<uint> indices = new List<uint>();
            var idx = 0u;
            
            foreach (var isoSurface in _isoSurfaces)
            {
                var startidx = idx;
                var idxcount = 0u;

                var nTris = isoSurface.IsoSurfaceVertices.Count / 3;

                var edgeSet = new HashSet<LevoyCabralEdge>();
                
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

                    var e1 = new LevoyCabralEdge(v1, v2);
                    if (false == edgeSet.Add(e1))
                    {
                        edgeSet.Remove(e1);
                    }
                    
                    var e2 = new LevoyCabralEdge(v2, v3);
                    if (false == edgeSet.Add(e2))
                    {
                        edgeSet.Remove(e2);
                    }
                    
                    var e3 = new LevoyCabralEdge(v1, v3);
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
                        OutlinesGeometry,
                        PrimitiveTopology.LineList,
                        idxcount,
                        1,
                        startidx,
                        0,
                        0);

                    OutlinesGeometry.PrimitiveSets.Add(pSet);
                    
                }
            }

            if (0 == sliceVertices.Count) return;
            
            OutlinesGeometry.VertexData = sliceVertices.ToArray();
            OutlinesGeometry.IndexData = indices.ToArray();
        }

        public void LocatorModified(ILocator locator)
        {
            _dirty = true;
        }
    }
    
    
    
    class LevoyCabralSamplingVolume : IVoxelVolume
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
        
        internal LevoyCabralSamplingVolume(IVoxelVolume source)
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
    
    internal class LevoyCabralEdge : IComparable<LevoyCabralEdge>
    {
        public IVertex3D V1;
        public IVertex3D V2;

        public LevoyCabralEdge(IVertex3D v1, IVertex3D v2)
        {
            V1 = v1;
            V2 = v2;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as LevoyCabralEdge);
        }

        public override int GetHashCode()
        {
            return 1;
        }

        public bool Equals(LevoyCabralEdge other)
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
        
        public int CompareTo(LevoyCabralEdge other)
        {
            return this.Equals(other) ? 0 : 1;
        }
        
        public static bool operator ==(LevoyCabralEdge lhs, LevoyCabralEdge rhs)
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

        public static bool operator !=(LevoyCabralEdge lhs, LevoyCabralEdge rhs)
        {
            return !(lhs == rhs);
        }
    }
}