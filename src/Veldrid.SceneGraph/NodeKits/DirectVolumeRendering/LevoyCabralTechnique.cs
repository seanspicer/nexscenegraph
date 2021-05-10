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
using System.Linq;
using System.Numerics;
using Veldrid.SceneGraph.AssetPrimitives;
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
        public delegate ITexture3D TextureTranslator(IVoxelVolume voxelVolume);
        
        private bool _dirty = true;

        private readonly IIsoSurfaceGenerator _isoSurfaceGenerator;
        private readonly List<IIsoSurface> _isoSurfaces;
        private ILevoyCabralLocator _levoyCabralLocator;
        private bool _outlinesDirty = true;
        private bool _samplingPlanesDirty = true;

        protected LevoyCabralTechnique(IShaderSet shaderSet, TextureTranslator textureTranslator)
        {
            ShaderSet = shaderSet;
            Texture3DTranslator = textureTranslator ?? Default3DTranslator;
            _isoSurfaceGenerator = new MarchingCubesIsoSurfaceGenerator();
            _isoSurfaces = new List<IIsoSurface>();
        }

        protected IShaderSet ShaderSet { get; }

        protected INode Node { get; set; }

        protected IVoxelVolume VoxelVolume { get; set; }
        protected ITexture3D TextureData { get; set; }
        protected TextureTranslator Texture3DTranslator { get; set; }
        protected IGeometry<Position3TexCoord3Color4> Geometry { get; set; }
        protected IGeometry<Position3Color3> OutlinesGeometry { get; set; }

        private Vector3 _eyeLocal { get; set; }

        public override void Init()
        {
            if (_volumeTile?.Layer == null) return;

            if (_volumeTile.Layer is IVoxelVolumeLayer voxelVolumeLayer &&
                _volumeTile.Locator is ILevoyCabralLocator levoyCabralLocator)
            {
                VoxelVolume = voxelVolumeLayer.VoxelVolume;
                _levoyCabralLocator = levoyCabralLocator;
                _levoyCabralLocator.AddCallback(this);
                
                TextureData = Texture3DTranslator(VoxelVolume);
                
                // Create the Geometry Placeholder
                Node = CreateSlices();
            }
            else
            {
                throw new NotImplementedException("The Levoy-Cabral Technique only works for voxel volumes");
            }
        }

        public void LocatorModified(ILocator locator)
        {
            _dirty = true;
        }

        public static ILevoyCabralTechnique Create(IShaderSet shaderSet = null,
            TextureTranslator textureTranslator = null)
        {
            return new LevoyCabralTechnique(shaderSet, textureTranslator);
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
            if (System.Math.Abs(eyeLocal.X - _eyeLocal.X) < 1e-5 &&
                System.Math.Abs(eyeLocal.Y - _eyeLocal.Y) < 1e-5 &&
                System.Math.Abs(eyeLocal.Z - _eyeLocal.Z) < 1e-5 &&
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

            Geometry.VertexLayouts = new List<VertexLayoutDescription>
            {
                Position3TexCoord3Color4.VertexLayoutDescription
            };

            Geometry.PipelineState.ShaderSet = ShaderSet ?? Position3TexCoord3Color4Shader.Instance.ShaderSet;
            Geometry.PipelineState.BlendStateDescription = BlendStateDescription.SingleAlphaBlend;
            Geometry.PipelineState.RasterizerStateDescription = new RasterizerStateDescription
            {
                CullMode = FaceCullMode.None,
                FillMode = PolygonFillMode.Solid,
                DepthClipEnabled = false,
                FrontFace = FrontFace.CounterClockwise
            };

            if (null != TextureData)
            {
                Geometry.PipelineState.AddTexture(TextureData);
            }

            OutlinesGeometry = Geometry<Position3Color3>.Create();

            OutlinesGeometry.VertexLayouts = new List<VertexLayoutDescription>
            {
                Position3Color3.VertexLayoutDescription
            };

            OutlinesGeometry.PipelineState.ShaderSet = Position3Color3Shader.Instance.ShaderSet;
            OutlinesGeometry.PipelineState.RasterizerStateDescription = RasterizerStateDescription.CullNone;

            geode.AddDrawable(Geometry);
            //geode.AddDrawable(OutlinesGeometry);
            return geode;
        }

        private void BuildIsoSurfaces()
        {
            // TODO enable changing this.
            var sampleRate = 200;
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
            var sliceVertices = new List<Position3TexCoord3Color4>();
            var indices = new List<uint>();
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
                        currentVertex = currentVertex.Next ??
                                        throw new Exception("Invalid number of vertices in isosurface");

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
            var sliceVertices = new List<Position3Color3>();
            var indices = new List<uint>();
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
                    currentVertex = currentVertex.Next ??
                                    throw new Exception("Invalid number of vertices in isosurface");
                    var v2 = currentVertex.Value;
                    currentVertex = currentVertex.Next ??
                                    throw new Exception("Invalid number of vertices in isosurface");
                    var v3 = currentVertex.Value;

                    if (i < nTris - 1)
                        currentVertex = currentVertex.Next ??
                                        throw new Exception("Invalid number of vertices in isosurface");

                    var e1 = new LevoyCabralEdge(v1, v2);
                    if (false == edgeSet.Add(e1)) edgeSet.Remove(e1);

                    var e2 = new LevoyCabralEdge(v2, v3);
                    if (false == edgeSet.Add(e2)) edgeSet.Remove(e2);

                    var e3 = new LevoyCabralEdge(v1, v3);
                    if (false == edgeSet.Add(e3)) edgeSet.Remove(e3);
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
        
        private static ITexture3D Default3DTranslator(IVoxelVolume voxelVolume)
        {
            var xdim = voxelVolume.Values.GetLength(0);
            var ydim = voxelVolume.Values.GetLength(1);
            var zdim = voxelVolume.Values.GetLength(2);
            
            var allTexData = RgbaData(voxelVolume, xdim, ydim, zdim);
            
            var texData = new ProcessedTexture(
                PixelFormat.R8_G8_B8_A8_UNorm, TextureType.Texture3D,
                (uint) xdim, (uint) ydim, (uint) zdim,
                (uint) 1, 1,
                allTexData);
            
            return Texture3D.Create(texData, 1,
                "SurfaceTexture", "SurfaceSampler");
        }
        
        private static byte[] RgbaData(IVoxelVolume voxelVolume, int xdim, int ydim, int zdim)
        {
            var rgbaData = new byte[xdim * ydim * zdim * 4];
            var maxValue = voxelVolume.Values.Cast<double>().Max();
            var minValue = voxelVolume.Values.Cast<double>().Min();
            var normalizer = 1/(maxValue - minValue);

            if (double.IsNaN(normalizer) || double.IsInfinity(normalizer) || !(normalizer > 0.0)) return rgbaData;
            
            for (var x = 0; x < xdim; ++x)
            for (var y = 0; y < ydim; ++y)
            for (var z = 0; z < zdim; ++z)
            {
                var index = (z + ydim * (y + xdim * x)) * 4;

                var colorLookup = (voxelVolume.Values[x, y, z] - minValue) * normalizer;
                System.Drawing.Color color = Rainbow(colorLookup);

                rgbaData[index + 0] = color.R; // R
                rgbaData[index + 1] = color.G; // G
                rgbaData[index + 2] = color.B; // B
                rgbaData[index + 3] = color.A; // A
            }
            return rgbaData;
        }

        private static System.Drawing.Color Rainbow(double normalizedValue)
        {
            // Convert into a value between 0 and 1023.
            int int_value = (int)(1023 * normalizedValue);

            switch (int_value)
            {
                // Map different color bands.
                case < 256:
                    // Red to yellow. (255, 0, 0) to (255, 255, 0).
                    return System.Drawing.Color.FromArgb(255, int_value, 0);
                case < 512:
                    // Yellow to green. (255, 255, 0) to (0, 255, 0).
                    int_value -= 256;
                    return System.Drawing.Color.FromArgb(255 - int_value, 255, 0);
                case < 768:
                    // Green to aqua. (0, 255, 0) to (0, 255, 255).
                    int_value -= 512;
                    return System.Drawing.Color.FromArgb(0, 255, int_value);
                default:
                    // Aqua to blue. (0, 255, 255) to (0, 0, 255).
                    int_value -= 768;
                    return System.Drawing.Color.FromArgb(0, 255 - int_value, 255);
            }
        }
        
        
        private static readonly Color[] _parulaColors =
        {
            Color.FromRgb( (0.2422f),  (0.1504f),  (0.6603f )),
            Color.FromRgb( (0.2444f),  (0.1534f),  (0.6728f )),
            Color.FromRgb( (0.2464f),  (0.1569f),  (0.6847f )),
            Color.FromRgb( (0.2484f),  (0.1607f),  (0.6961f )),
            Color.FromRgb( (0.2503f),  (0.1648f),  (0.7071f )),
            Color.FromRgb( (0.2522f),  (0.1689f),  (0.7179f )),
            Color.FromRgb( (0.254f),  (0.1732f),  (0.7286f )),
            Color.FromRgb( (0.2558f),  (0.1773f),  (0.7393f )),
            Color.FromRgb( (0.2576f),  (0.1814f),  (0.7501f )),
            Color.FromRgb( (0.2611f),  (0.1893f),  (0.7719f )),
            Color.FromRgb( (0.2628f),  (0.1932f),  (0.7828f )),
            Color.FromRgb( (0.2645f),  (0.1972f),  (0.7937f )),
            Color.FromRgb( (0.2594f),  (0.1854f),  (0.761f )),
            Color.FromRgb( (0.2661f),  (0.2011f),  (0.8043f )),
            Color.FromRgb( (0.2676f),  (0.2052f),  (0.8148f )),
            Color.FromRgb( (0.2691f),  (0.2094f),  (0.8249f )),
            Color.FromRgb( (0.2704f),  (0.2138f),  (0.8346f )),
            Color.FromRgb( (0.2717f),  (0.2184f),  (0.8439f )),
            Color.FromRgb( (0.2729f),  (0.2231f),  (0.8528f )),
            Color.FromRgb( (0.274f),  (0.228f),  (0.8612f )),
            Color.FromRgb( (0.2749f),  (0.233f),  (0.8692f )),
            Color.FromRgb( (0.2758f),  (0.2382f),  (0.8767f )),
            Color.FromRgb( (0.2766f),  (0.2435f),  (0.884f )),
            Color.FromRgb( (0.2774f),  (0.2489f),  (0.8908f )),
            Color.FromRgb( (0.2781f),  (0.2543f),  (0.8973f )),
            Color.FromRgb( (0.2788f),  (0.2598f),  (0.9035f )),
            Color.FromRgb( (0.2794f),  (0.2653f),  (0.9094f )),
            Color.FromRgb( (0.2798f),  (0.2708f),  (0.915f )),
            Color.FromRgb( (0.2802f),  (0.2764f),  (0.9204f )),
            Color.FromRgb( (0.2806f),  (0.2819f),  (0.9255f )),
            Color.FromRgb( (0.2809f),  (0.2875f),  (0.9305f )),
            Color.FromRgb( (0.2811f),  (0.293f),  (0.9352f )),
            Color.FromRgb( (0.2813f),  (0.2985f),  (0.9397f )),
            Color.FromRgb( (0.2814f),  (0.304f),  (0.9441f )),
            Color.FromRgb( (0.2814f),  (0.3095f),  (0.9483f )),
            Color.FromRgb( (0.2813f),  (0.315f),  (0.9524f )),
            Color.FromRgb( (0.2811f),  (0.3204f),  (0.9563f )),
            Color.FromRgb( (0.2809f),  (0.3259f),  (0.96f )),
            Color.FromRgb( (0.2807f),  (0.3313f),  (0.9636f )),
            Color.FromRgb( (0.2803f),  (0.3367f),  (0.967f )),
            Color.FromRgb( (0.2798f),  (0.3421f),  (0.9702f )),
            Color.FromRgb( (0.2791f),  (0.3475f),  (0.9733f )),
            Color.FromRgb( (0.2784f),  (0.3529f),  (0.9763f )),
            Color.FromRgb( (0.2776f),  (0.3583f),  (0.9791f )),
            Color.FromRgb( (0.2766f),  (0.3638f),  (0.9817f )),
            Color.FromRgb( (0.2754f),  (0.3693f),  (0.984f )),
            Color.FromRgb( (0.2741f),  (0.3748f),  (0.9862f )),
            Color.FromRgb( (0.2726f),  (0.3804f),  (0.9881f )),
            Color.FromRgb( (0.271f),  (0.386f),  (0.9898f )),
            Color.FromRgb( (0.2691f),  (0.3916f),  (0.9912f )),
            Color.FromRgb( (0.267f),  (0.3973f),  (0.9924f )),
            Color.FromRgb( (0.2647f),  (0.403f),  (0.9935f )),
            Color.FromRgb( (0.2621f),  (0.4088f),  (0.9946f )),
            Color.FromRgb( (0.2591f),  (0.4145f),  (0.9955f )),
            Color.FromRgb( (0.2556f),  (0.4203f),  (0.9965f )),
            Color.FromRgb( (0.2517f),  (0.4261f),  (0.9974f )),
            Color.FromRgb( (0.2473f),  (0.4319f),  (0.9983f )),
            Color.FromRgb( (0.2424f),  (0.4378f),  (0.9991f )),
            Color.FromRgb( (0.2369f),  (0.4437f),  (0.9996f )),
            Color.FromRgb( (0.2311f),  (0.4497f),  (0.9995f )),
            Color.FromRgb( (0.225f),  (0.4559f),  (0.9985f )),
            Color.FromRgb( (0.2189f),  (0.462f),  (0.9968f )),
            Color.FromRgb( (0.2128f),  (0.4682f),  (0.9948f )),
            Color.FromRgb( (0.2066f),  (0.4743f),  (0.9926f )),
            Color.FromRgb( (0.2006f),  (0.4803f),  (0.9906f )),
            Color.FromRgb( (0.195f),  (0.4861f),  (0.9887f )),
            Color.FromRgb( (0.1903f),  (0.4919f),  (0.9867f )),
            Color.FromRgb( (0.1869f),  (0.4975f),  (0.9844f )),
            Color.FromRgb( (0.1847f),  (0.503f),  (0.9819f )),
            Color.FromRgb( (0.1831f),  (0.5084f),  (0.9793f )),
            Color.FromRgb( (0.1818f),  (0.5138f),  (0.9766f )),
            Color.FromRgb( (0.1806f),  (0.5191f),  (0.9738f )),
            Color.FromRgb( (0.1795f),  (0.5244f),  (0.9709f )),
            Color.FromRgb( (0.1785f),  (0.5296f),  (0.9677f )),
            Color.FromRgb( (0.1778f),  (0.5349f),  (0.9641f )),
            Color.FromRgb( (0.1773f),  (0.5401f),  (0.9602f )),
            Color.FromRgb( (0.1768f),  (0.5452f),  (0.956f )),
            Color.FromRgb( (0.1764f),  (0.5504f),  (0.9516f )),
            Color.FromRgb( (0.1755f),  (0.5554f),  (0.9473f )),
            Color.FromRgb( (0.174f),  (0.5605f),  (0.9432f )),
            Color.FromRgb( (0.1716f),  (0.5655f),  (0.9393f )),
            Color.FromRgb( (0.1686f),  (0.5705f),  (0.9357f )),
            Color.FromRgb( (0.1649f),  (0.5755f),  (0.9323f )),
            Color.FromRgb( (0.161f),  (0.5805f),  (0.9289f )),
            Color.FromRgb( (0.1573f),  (0.5854f),  (0.9254f )),
            Color.FromRgb( (0.154f),  (0.5902f),  (0.9218f )),
            Color.FromRgb( (0.1513f),  (0.595f),  (0.9182f )),
            Color.FromRgb( (0.1492f),  (0.5997f),  (0.9147f )),
            Color.FromRgb( (0.1475f),  (0.6043f),  (0.9113f )),
            Color.FromRgb( (0.1461f),  (0.6089f),  (0.908f )),
            Color.FromRgb( (0.1446f),  (0.6135f),  (0.905f )),
            Color.FromRgb( (0.1429f),  (0.618f),  (0.9022f )),
            Color.FromRgb( (0.1408f),  (0.6226f),  (0.8998f )),
            Color.FromRgb( (0.1383f),  (0.6272f),  (0.8975f )),
            Color.FromRgb( (0.1354f),  (0.6317f),  (0.8953f )),
            Color.FromRgb( (0.1321f),  (0.6363f),  (0.8932f )),
            Color.FromRgb( (0.1288f),  (0.6408f),  (0.891f )),
            Color.FromRgb( (0.1253f),  (0.6453f),  (0.8887f )),
            Color.FromRgb( (0.1219f),  (0.6497f),  (0.8862f )),
            Color.FromRgb( (0.1185f),  (0.6541f),  (0.8834f )),
            Color.FromRgb( (0.1152f),  (0.6584f),  (0.8804f )),
            Color.FromRgb( (0.1119f),  (0.6627f),  (0.877f )),
            Color.FromRgb( (0.1085f),  (0.6669f),  (0.8734f )),
            Color.FromRgb( (0.1048f),  (0.671f),  (0.8695f )),
            Color.FromRgb( (0.1009f),  (0.675f),  (0.8653f )),
            Color.FromRgb( (0.0964f),  (0.6789f),  (0.8609f )),
            Color.FromRgb( (0.0914f),  (0.6828f),  (0.8562f )),
            Color.FromRgb( (0.0855f),  (0.6865f),  (0.8513f )),
            Color.FromRgb( (0.0789f),  (0.6902f),  (0.8462f )),
            Color.FromRgb( (0.0713f),  (0.6938f),  (0.8409f )),
            Color.FromRgb( (0.0628f),  (0.6972f),  (0.8355f )),
            Color.FromRgb( (0.0535f),  (0.7006f),  (0.8299f )),
            Color.FromRgb( (0.0433f),  (0.7039f),  (0.8242f )),
            Color.FromRgb( (0.0328f),  (0.7071f),  (0.8183f )),
            Color.FromRgb( (0.0234f),  (0.7103f),  (0.8124f )),
            Color.FromRgb( (0.0155f),  (0.7133f),  (0.8064f )),
            Color.FromRgb( (0.0091f),  (0.7163f),  (0.8003f )),
            Color.FromRgb( (0.0046f),  (0.7192f),  (0.7941f )),
            Color.FromRgb( (0.0019f),  (0.722f),  (0.7878f )),
            Color.FromRgb( (0.0009f),  (0.7248f),  (0.7815f )),
            Color.FromRgb( (0.0018f),  (0.7275f),  (0.7752f )),
            Color.FromRgb( (0.0046f),  (0.7301f),  (0.7688f )),
            Color.FromRgb( (0.0094f),  (0.7327f),  (0.7623f )),
            Color.FromRgb( (0.0162f),  (0.7352f),  (0.7558f )),
            Color.FromRgb( (0.0253f),  (0.7376f),  (0.7492f )),
            Color.FromRgb( (0.0369f),  (0.74f),  (0.7426f )),
            Color.FromRgb( (0.0504f),  (0.7423f),  (0.7359f )),
            Color.FromRgb( (0.0638f),  (0.7446f),  (0.7292f )),
            Color.FromRgb( (0.077f),  (0.7468f),  (0.7224f )),
            Color.FromRgb( (0.0899f),  (0.7489f),  (0.7156f )),
            Color.FromRgb( (0.1023f),  (0.751f),  (0.7088f )),
            Color.FromRgb( (0.1141f),  (0.7531f),  (0.7019f )),
            Color.FromRgb( (0.1252f),  (0.7552f),  (0.695f )),
            Color.FromRgb( (0.1354f),  (0.7572f),  (0.6881f )),
            Color.FromRgb( (0.1448f),  (0.7593f),  (0.6812f )),
            Color.FromRgb( (0.1532f),  (0.7614f),  (0.6741f )),
            Color.FromRgb( (0.1609f),  (0.7635f),  (0.6671f )),
            Color.FromRgb( (0.1678f),  (0.7656f),  (0.6599f )),
            Color.FromRgb( (0.1741f),  (0.7678f),  (0.6527f )),
            Color.FromRgb( (0.1799f),  (0.7699f),  (0.6454f )),
            Color.FromRgb( (0.1853f),  (0.7721f),  (0.6379f )),
            Color.FromRgb( (0.1905f),  (0.7743f),  (0.6303f )),
            Color.FromRgb( (0.1954f),  (0.7765f),  (0.6225f )),
            Color.FromRgb( (0.2003f),  (0.7787f),  (0.6146f )),
            Color.FromRgb( (0.2061f),  (0.7808f),  (0.6065f )),
            Color.FromRgb( (0.2118f),  (0.7828f),  (0.5983f )),
            Color.FromRgb( (0.2178f),  (0.7849f),  (0.5899f )),
            Color.FromRgb( (0.2244f),  (0.7869f),  (0.5813f )),
            Color.FromRgb( (0.2318f),  (0.7887f),  (0.5725f )),
            Color.FromRgb( (0.2401f),  (0.7905f),  (0.5636f )),
            Color.FromRgb( (0.2491f),  (0.7922f),  (0.5546f )),
            Color.FromRgb( (0.2589f),  (0.7937f),  (0.5454f )),
            Color.FromRgb( (0.2695f),  (0.7951f),  (0.536f )),
            Color.FromRgb( (0.2809f),  (0.7964f),  (0.5266f )),
            Color.FromRgb( (0.2929f),  (0.7975f),  (0.517f )),
            Color.FromRgb( (0.3052f),  (0.7985f),  (0.5074f )),
            Color.FromRgb( (0.3176f),  (0.7994f),  (0.4975f )),
            Color.FromRgb( (0.3301f),  (0.8002f),  (0.4876f )),
            Color.FromRgb( (0.3424f),  (0.8009f),  (0.4774f )),
            Color.FromRgb( (0.3548f),  (0.8016f),  (0.4669f )),
            Color.FromRgb( (0.3671f),  (0.8021f),  (0.4563f )),
            Color.FromRgb( (0.3795f),  (0.8026f),  (0.4454f )),
            Color.FromRgb( (0.3921f),  (0.8029f),  (0.4344f )),
            Color.FromRgb( (0.405f),  (0.8031f),  (0.4233f )),
            Color.FromRgb( (0.4184f),  (0.803f),  (0.4122f )),
            Color.FromRgb( (0.4322f),  (0.8028f),  (0.4013f )),
            Color.FromRgb( (0.4463f),  (0.8024f),  (0.3904f )),
            Color.FromRgb( (0.4608f),  (0.8018f),  (0.3797f )),
            Color.FromRgb( (0.4753f),  (0.8011f),  (0.3691f )),
            Color.FromRgb( (0.4899f),  (0.8002f),  (0.3586f )),
            Color.FromRgb( (0.5044f),  (0.7993f),  (0.348f )),
            Color.FromRgb( (0.5187f),  (0.7982f),  (0.3374f )),
            Color.FromRgb( (0.5329f),  (0.797f),  (0.3267f )),
            Color.FromRgb( (0.547f),  (0.7957f),  (0.3159f )),
            Color.FromRgb( (0.5609f),  (0.7943f),  (0.305f )),
            Color.FromRgb( (0.5748f),  (0.7929f),  (0.2941f )),
            Color.FromRgb( (0.5886f),  (0.7913f),  (0.2833f )),
            Color.FromRgb( (0.6024f),  (0.7896f),  (0.2726f )),
            Color.FromRgb( (0.6161f),  (0.7878f),  (0.2622f )),
            Color.FromRgb( (0.6297f),  (0.7859f),  (0.2521f )),
            Color.FromRgb( (0.6433f),  (0.7839f),  (0.2423f )),
            Color.FromRgb( (0.6567f),  (0.7818f),  (0.2329f )),
            Color.FromRgb( (0.6701f),  (0.7796f),  (0.2239f )),
            Color.FromRgb( (0.6833f),  (0.7773f),  (0.2155f )),
            Color.FromRgb( (0.6963f),  (0.775f),  (0.2075f )),
            Color.FromRgb( (0.7091f),  (0.7727f),  (0.1998f )),
            Color.FromRgb( (0.7218f),  (0.7703f),  (0.1924f )),
            Color.FromRgb( (0.7344f),  (0.7679f),  (0.1852f )),
            Color.FromRgb( (0.7468f),  (0.7654f),  (0.1782f )),
            Color.FromRgb( (0.759f),  (0.7629f),  (0.1717f )),
            Color.FromRgb( (0.771f),  (0.7604f),  (0.1658f )),
            Color.FromRgb( (0.7829f),  (0.7579f),  (0.1608f )),
            Color.FromRgb( (0.7945f),  (0.7554f),  (0.157f )),
            Color.FromRgb( (0.806f),  (0.7529f),  (0.1546f )),
            Color.FromRgb( (0.8172f),  (0.7505f),  (0.1535f )),
            Color.FromRgb( (0.8281f),  (0.7481f),  (0.1536f )),
            Color.FromRgb( (0.8389f),  (0.7457f),  (0.1546f )),
            Color.FromRgb( (0.8495f),  (0.7435f),  (0.1564f )),
            Color.FromRgb( (0.86f),  (0.7413f),  (0.1587f )),
            Color.FromRgb( (0.8703f),  (0.7392f),  (0.1615f )),
            Color.FromRgb( (0.8804f),  (0.7372f),  (0.165f )),
            Color.FromRgb( (0.8903f),  (0.7353f),  (0.1695f )),
            Color.FromRgb( (0.9f),  (0.7336f),  (0.1749f )),
            Color.FromRgb( (0.9093f),  (0.7321f),  (0.1815f )),
            Color.FromRgb( (0.9184f),  (0.7308f),  (0.189f )),
            Color.FromRgb( (0.9272f),  (0.7298f),  (0.1973f )),
            Color.FromRgb( (0.9357f),  (0.729f),  (0.2061f )),
            Color.FromRgb( (0.944f),  (0.7285f),  (0.2151f )),
            Color.FromRgb( (0.9523f),  (0.7284f),  (0.2237f )),
            Color.FromRgb( (0.9606f),  (0.7285f),  (0.2312f )),
            Color.FromRgb( (0.9689f),  (0.7292f),  (0.2373f )),
            Color.FromRgb( (0.977f),  (0.7304f),  (0.2418f )),
            Color.FromRgb( (0.9842f),  (0.733f),  (0.2446f )),
            Color.FromRgb( (0.99f),  (0.7365f),  (0.2429f )),
            Color.FromRgb( (0.9946f),  (0.7407f),  (0.2394f )),
            Color.FromRgb( (0.9966f),  (0.7458f),  (0.2351f )),
            Color.FromRgb( (0.9971f),  (0.7513f),  (0.2309f )),
            Color.FromRgb( (0.9972f),  (0.7569f),  (0.2267f )),
            Color.FromRgb( (0.9971f),  (0.7626f),  (0.2224f )),
            Color.FromRgb( (0.9969f),  (0.7683f),  (0.2181f )),
            Color.FromRgb( (0.9966f),  (0.774f),  (0.2138f )),
            Color.FromRgb( (0.9962f),  (0.7798f),  (0.2095f )),
            Color.FromRgb( (0.9957f),  (0.7856f),  (0.2053f )),
            Color.FromRgb( (0.9949f),  (0.7915f),  (0.2012f )),
            Color.FromRgb( (0.9938f),  (0.7974f),  (0.1974f )),
            Color.FromRgb( (0.9923f),  (0.8034f),  (0.1939f )),
            Color.FromRgb( (0.9906f),  (0.8095f),  (0.1906f )),
            Color.FromRgb( (0.9885f),  (0.8156f),  (0.1875f )),
            Color.FromRgb( (0.9861f),  (0.8218f),  (0.1846f )),
            Color.FromRgb( (0.9835f),  (0.828f),  (0.1817f )),
            Color.FromRgb( (0.9807f),  (0.8342f),  (0.1787f )),
            Color.FromRgb( (0.9778f),  (0.8404f),  (0.1757f )),
            Color.FromRgb( (0.9748f),  (0.8467f),  (0.1726f )),
            Color.FromRgb( (0.972f),  (0.8529f),  (0.1695f )),
            Color.FromRgb( (0.9694f),  (0.8591f),  (0.1665f )),
            Color.FromRgb( (0.9671f),  (0.8654f),  (0.1636f )),
            Color.FromRgb( (0.9651f),  (0.8716f),  (0.1608f )),
            Color.FromRgb( (0.9634f),  (0.8778f),  (0.1582f )),
            Color.FromRgb( (0.9619f),  (0.884f),  (0.1557f )),
            Color.FromRgb( (0.9608f),  (0.8902f),  (0.1532f )),
            Color.FromRgb( (0.9601f),  (0.8963f),  (0.1507f )),
            Color.FromRgb( (0.9596f),  (0.9023f),  (0.148f )),
            Color.FromRgb( (0.9595f),  (0.9084f),  (0.145f )),
            Color.FromRgb( (0.9597f),  (0.9143f),  (0.1418f )),
            Color.FromRgb( (0.9601f),  (0.9203f),  (0.1382f )),
            Color.FromRgb( (0.9608f),  (0.9262f),  (0.1344f )),
            Color.FromRgb( (0.9618f),  (0.932f),  (0.1304f )),
            Color.FromRgb( (0.9629f),  (0.9379f),  (0.1261f )),
            Color.FromRgb( (0.9642f),  (0.9437f),  (0.1216f )),
            Color.FromRgb( (0.9657f),  (0.9494f),  (0.1168f )),
            Color.FromRgb( (0.9674f),  (0.9552f),  (0.1116f )),
            Color.FromRgb( (0.9692f),  (0.9609f),  (0.1061f )),
            Color.FromRgb( (0.9711f),  (0.9667f),  (0.1001f )),
            Color.FromRgb( (0.973f),  (0.9724f),  (0.0938f )),
            Color.FromRgb( (0.9749f),  (0.9782f),  (0.0872f )),
            Color.FromRgb( (0.9769f),  (0.9839f),  (0.0805f ))
        };
    }


    internal class LevoyCabralSamplingVolume : IVoxelVolume
    {
        private readonly IVoxelVolume _source;

        internal LevoyCabralSamplingVolume(IVoxelVolume source)
        {
            _source = source;

            var xdim = _source.XValues.GetLength(0) - 1;
            var ydim = _source.XValues.GetLength(1) - 1;
            var zdim = _source.XValues.GetLength(2) - 1;

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

        public double XMin => XValues[0, 0, 0];
        public double XMax => XValues[1, 1, 1];

        public double YMin => YValues[0, 0, 0];
        public double YMax => YValues[1, 1, 1];

        public double ZMin => ZValues[0, 0, 0];
        public double ZMax => ZValues[1, 1, 1];

        public double MinDist { get; private set; }
        public double MaxDist { get; private set; }
        public double[,,] Values { get; }
        public double[,,] XValues { get; }
        public double[,,] YValues { get; }
        public double[,,] ZValues { get; }

        public Vector3 TexGen(Vector3 point)
        {
            return new Vector3(
                (float) ((point.X - XMin) / (XMax - XMin)),
                (float) ((point.Y - YMin) / (YMax - YMin)),
                (float) ((point.Z - ZMin) / (ZMax - ZMin))
            );
        }

        public void UpdateDistances(Vector3 eyePoint)
        {
            MinDist = double.MaxValue;
            MaxDist = double.MinValue;

            for (var z = 0; z < 2; ++z)
            for (var y = 0; y < 2; ++y)
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

    internal class LevoyCabralEdge : IComparable<LevoyCabralEdge>
    {
        public IVertex3D V1;
        public IVertex3D V2;

        public LevoyCabralEdge(IVertex3D v1, IVertex3D v2)
        {
            V1 = v1;
            V2 = v2;
        }

        public int CompareTo(LevoyCabralEdge other)
        {
            return Equals(other) ? 0 : 1;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LevoyCabralEdge);
        }

        public override int GetHashCode()
        {
            return 1;
        }

        public bool Equals(LevoyCabralEdge other)
        {
            // If parameter is null, return false.
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;

            var tol = 1e-6;

            if (System.Math.Abs(V1.X - other.V1.X) < tol &&
                System.Math.Abs(V1.Y - other.V1.Y) < tol &&
                System.Math.Abs(V1.Z - other.V1.Z) < tol &&
                System.Math.Abs(V2.X - other.V2.X) < tol &&
                System.Math.Abs(V2.Y - other.V2.Y) < tol &&
                System.Math.Abs(V2.Z - other.V2.Z) < tol)
                return true;

            if (System.Math.Abs(V1.X - other.V2.X) < tol &&
                System.Math.Abs(V1.Y - other.V2.Y) < tol &&
                System.Math.Abs(V1.Z - other.V2.Z) < tol &&
                System.Math.Abs(V2.X - other.V1.X) < tol &&
                System.Math.Abs(V2.Y - other.V1.Y) < tol &&
                System.Math.Abs(V2.Z - other.V1.Z) < tol)
                return true;

            return false;
        }

        public static bool operator ==(LevoyCabralEdge lhs, LevoyCabralEdge rhs)
        {
            // Check for null on left side.
            if (ReferenceEquals(lhs, null))
            {
                if (ReferenceEquals(rhs, null))
                    // null == null = true.
                    return true;

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