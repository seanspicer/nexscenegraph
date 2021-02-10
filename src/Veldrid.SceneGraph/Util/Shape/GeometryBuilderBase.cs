using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Veldrid.SceneGraph.Util.Shape
{
    internal class GeometryBuilderBase<T> where T : struct, ISettablePrimitiveElement
    {
        internal abstract class ExtendedPrimitive
        {
            public enum PrimitiveType
            {
                QuadStrip,
                TriFan
            }

            public PrimitiveType Type { get; set; }
            
            public List<Vector3> Vertices = new List<Vector3>();
            public List<Vector3> Normals = new List<Vector3>();
            public List<Vector2> TexCoords = new List<Vector2>();
        }

        private class QuadStrip : ExtendedPrimitive
        {
            public QuadStrip()
            {
                Type = PrimitiveType.QuadStrip;
            }
        }
        
        internal class TriangleFan : ExtendedPrimitive
        {
            public TriangleFan()
            {
                Type = PrimitiveType.TriFan;
            }
        }

        private List<ExtendedPrimitive> _primitives = new List<ExtendedPrimitive>();
        private ExtendedPrimitive _currentPrimitive;
            
        protected Vector3 Center { get; set; } = Vector3.Zero;

        private Matrix4x4 _matrix = Matrix4x4.Identity;
        
        protected Matrix4x4 Matrix
        {
            get
            {
                return _matrix;
            }
            set
            {
                _matrix = value;
                if (Matrix4x4.Invert(_matrix, out var inverse))
                {
                    _inverse = inverse;
                }
            }
        }
        
        private Matrix4x4 _inverse = Matrix4x4.Identity;
        protected Matrix4x4 Inverse => _inverse;

        protected void BeginQuadStrip()
        {
            _currentPrimitive = new QuadStrip();
        }

        protected void BeginTriangleFan()
        {
            _currentPrimitive = new TriangleFan();
        }
        
        protected void End()
        {
            var inverseTranspose = Matrix4x4.Transpose(Inverse);
            
            for (var i = 0; i < _currentPrimitive.Vertices.Count; ++i)
            {
                _currentPrimitive.Vertices[i] = Vector3.Transform(_currentPrimitive.Vertices[i], _matrix);
            }
            
            for (var i = 0; i < _currentPrimitive.Normals.Count; ++i)
            {
                _currentPrimitive.Normals[i] = Vector3.Normalize(Vector3.Transform(_currentPrimitive.Normals[i], inverseTranspose));
            }
            
            _primitives.Add(_currentPrimitive);
        }
            
        protected void Normal3f(Vector3 nrm)
        {
            _currentPrimitive.Normals.Add(nrm);
        }
        protected void Normal3f(float x, float y, float z)
        {
            Normal3f(new Vector3(x, y, z));
        }

        protected void Vertex3f(Vector3 vtx)
        {
            _currentPrimitive.Vertices.Add(vtx+Center);
        }
        protected void Vertex3f(float x, float y, float z)
        {
            Vertex3f(new Vector3(x, y, z));
        }

        protected void TexCoord2f(Vector2 texcrd)
        {
            _currentPrimitive.TexCoords.Add(texcrd);
        }
        protected void TexCoord2f(float x, float y)
        {
            TexCoord2f(new Vector2(x, y));
        }

        protected virtual void BuildVertexAndIndexArrays(out T[] vertexArray, out uint[] indexArray, Vector3 [] colors)
        {
            var vertexDataList = new List<T>();
            var indexDataList = new List<uint>();
            var lastIdx = 0;
            
            foreach (var strip in _primitives)
            {
                var triIndicies = new List<int>();
                switch (strip.Type)
                {
                    case ExtendedPrimitive.PrimitiveType.QuadStrip:
                    {
                        var nQuads = (uint)(strip.Vertices.Count / 2 - 1);

                        // Convert QuadStrips to Triangle List
                        for (var qidx = 0; qidx < nQuads; ++qidx)
                        {
                            var q = qidx * 2;
                    
                            triIndicies.AddRange(new int[] {q, q+3, q+1});
                            triIndicies.AddRange(new int[] {q, q+2, q+3});
                        }

                    } break;

                    case ExtendedPrimitive.PrimitiveType.TriFan:
                    {
                        var nTris = (uint) (strip.Vertices.Count - 2);

                        // Convert TriStrips to Triangle List
                        for (var tidx = 0; tidx < nTris; ++tidx)
                        {
                            var t = tidx+1;
                    
                            triIndicies.AddRange(new int[] {0, t, t+1});
                        }

                        
                    } break;
                }

                indexDataList.AddRange(triIndicies.Select(idx => (uint) (lastIdx + idx)));
                
                lastIdx += strip.Vertices.Count;
                for (var idx = 0; idx < strip.Vertices.Count; ++idx)
                {
                    var vtx = new T();
                    vtx.SetPosition(strip.Vertices[idx]);
                    vtx.SetNormal(strip.Normals[idx]);
                    vtx.SetTexCoord2(strip.TexCoords[idx]);
                    vtx.SetColor3(colors[0]);
                    vertexDataList.Add(vtx);
                }
            }

            vertexArray = vertexDataList.ToArray();
            indexArray = indexDataList.ToArray();
        }
    }
}