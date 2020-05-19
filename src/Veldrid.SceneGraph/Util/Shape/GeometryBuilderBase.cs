using System.Collections.Generic;
using System.Numerics;

namespace Veldrid.SceneGraph.Util.Shape
{
    internal class GeometryBuilderBase<T> where T : struct, ISettablePrimitiveElement
    {
        internal class QuadStrip
        {
            public List<Vector3> Vertices = new List<Vector3>();
            public List<Vector3> Normals = new List<Vector3>();
            public List<Vector2> TexCoords = new List<Vector2>();
        }
        
        private List<QuadStrip> _strips = new List<QuadStrip>();
        private QuadStrip _currentStrip;
            
        protected void Begin()
        {
            _currentStrip = new QuadStrip();
        }

        protected void End()
        {
            _strips.Add(_currentStrip);
        }
            
        protected void Normal3f(Vector3 nrm)
        {
            _currentStrip.Normals.Add(nrm);
        }
        protected void Normal3f(float x, float y, float z)
        {
            Normal3f(new Vector3(x, y, z));
        }

        protected void Vertex3f(Vector3 vtx)
        {
            _currentStrip.Vertices.Add(vtx);
        }
        protected void Vertex3f(float x, float y, float z)
        {
            Vertex3f(new Vector3(x, y, z));
        }

        protected void TexCoord2f(Vector2 texcrd)
        {
            _currentStrip.TexCoords.Add(texcrd);
        }
        protected void TexCoord2f(float x, float y)
        {
            TexCoord2f(new Vector2(x, y));
        }

        protected void BuildVertexAndIndexArrays(out T[] vertexArray, out uint[] indexArray, Vector3 [] colors)
        {
            var vertexDataList = new List<T>();
            var indexDataList = new List<uint>();
            var lastIdx = 0;
            foreach (var strip in _strips)
            {
                var nQuads = (uint)(strip.Vertices.Count / 2 - 1);

                // Convert QuadStrips to Triangle List
                var triIndicies = new List<int>();
                for (var qidx = 0; qidx < nQuads; ++qidx)
                {
                    var q = qidx * 2;
                    
                    triIndicies.AddRange(new int[] {q, q+3, q+1});
                    triIndicies.AddRange(new int[] {q, q+2, q+3});
                }

                foreach (var idx in triIndicies)
                {
                    indexDataList.Add((uint)(lastIdx+idx));
                }
                lastIdx += strip.Vertices.Count;
                for (var idx = 0; idx < strip.Vertices.Count; ++idx)
                {
                    var vtx = new T();
                    vtx.SetPosition(strip.Vertices[idx]);
                    vtx.SetNormal(strip.Normals[idx]);
                    vtx.SetTexCoord(strip.TexCoords[idx]);
                    vtx.SetColor3(colors[0]);
                    vertexDataList.Add(vtx);
                }
            }

            vertexArray = vertexDataList.ToArray();
            indexArray = indexDataList.ToArray();
        }
    }
}