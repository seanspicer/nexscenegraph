using System.Collections.Generic;
using System.Numerics;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Util.Shape
{
    internal class PathGeometryBuilder<T> where T : struct, ISettablePrimitiveElement
    {
        internal void Build(IGeometry<T> geometry, ITessellationHints hints, Vector3[] colors, IPath path)
        {
            var vertexDataList = new List<T>();
            var indexDataList = new List<uint>();
            
            // Draw a line
            for (var i = 0; i < path.PathLocations.Length; ++i)
            {
                var vtx = new T();
                vtx.SetPosition(path.PathLocations[i]);
                vtx.SetNormal(Vector3.UnitX);
                vtx.SetTexCoord(Vector2.Zero);
                vtx.SetColor3(colors[0]);
                vertexDataList.Add(vtx);
                
                indexDataList.Add((uint)i);
            }

            geometry.VertexData = vertexDataList.ToArray();
            geometry.IndexData = indexDataList.ToArray();
            
            geometry.VertexLayout = VertexLayoutHelpers.GetLayoutDescription(typeof(T));
            
            var pSet = DrawElements<T>.Create(
                geometry, 
                PrimitiveTopology.LineStrip, 
                (uint)geometry.IndexData.Length, 
                1, 
                0, 
                0, 
                0);
            
            geometry.PrimitiveSets.Add(pSet);
        } 
    }
}