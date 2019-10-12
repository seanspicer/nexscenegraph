using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Util.Shape
{
    internal class PathGeometryBuilder<T> where T : struct, ISettablePrimitiveElement
    {
        internal void Build(IGeometry<T> geometry, ITessellationHints hints, Vector3[] colors, IPath path)
        {
            if (path.PathLocations.Length < 3)
            {
                throw new ArgumentException("Not enough vertices for a path");
            }
            
            if (hints.CreatePathAsLine)
            {
                BuildLine(geometry, hints, colors, path);
            }
            else
            {
                BuildCylinder(geometry, hints, colors, path);
            }
            
        }

        private void BuildLine(IGeometry<T> geometry, ITessellationHints hints, Vector3[] colors, IPath path)
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

        private void BuildCylinder(IGeometry<T> geometry, ITessellationHints hints, Vector3[] colors, IPath path)
        {
            var tangents = ComputeTangents(path);
            var normals = ComputeNormals(path, tangents);
            var binormals = ComputeBinormals(tangents, normals);

            geometry.VertexLayout = VertexLayoutHelpers.GetLayoutDescription(typeof(T));
            
            var vertexDataList = new List<T>();
            var indexDataList = new List<uint>();

            var nSegments = (int)System.Math.Floor(10f * hints.DetailRatio);
            if (nSegments < 4) nSegments = 4;
            
            for (var j = 0; j < nSegments; ++j)
            {
                var theta = j*2*System.Math.PI / nSegments;
                var r = hints.Radius;
                
                // Draw a line
                for (var i = 0; i < path.PathLocations.Length; ++i)
                {
                    var p = path.PathLocations[i];
                    p += (float)System.Math.Sin(theta)*r*normals[i] + (float)System.Math.Cos(theta)*r*binormals[i];
                    
                    var vtx = new T();
                    vtx.SetPosition(p);
                    vtx.SetNormal(Vector3.UnitX);
                    vtx.SetTexCoord(Vector2.Zero);
                    vtx.SetColor3(colors[0]);
                    vertexDataList.Add(vtx);

                    indexDataList.Add((uint) (j * path.PathLocations.Length) + (uint)i);
                }

                var pSet = DrawElements<T>.Create(
                    geometry, 
                    PrimitiveTopology.LineStrip, 
                    (uint)path.PathLocations.Length, 
                    1, 
                    (uint)(j*path.PathLocations.Length), 
                    0, 
                    0);
            
                geometry.PrimitiveSets.Add(pSet);
            }
            
            geometry.VertexData = vertexDataList.ToArray();
            geometry.IndexData = indexDataList.ToArray();
            
        }
        
        private Vector3[] ComputeTangents(IPath path)
        {
            // PRECONDITION: at least two vertices in path

            var nVerts = path.PathLocations.Length;
            var tangents = new Vector3[path.PathLocations.Length];

            tangents[0] = Vector3.Subtract(path.PathLocations[1],path.PathLocations[0]);
            for (var i = 1; i < nVerts - 1; ++i)
            {
                tangents[i] = Vector3.Subtract(path.PathLocations[i + 1],path.PathLocations[i-1]);
            }
            tangents[nVerts-1] = Vector3.Subtract(path.PathLocations[nVerts-1],path.PathLocations[nVerts-2]);

            return tangents;
        }
        
        private Vector3[] ComputeNormals(IPath path, Vector3[] tangents)
        {
            var nVerts = path.PathLocations.Length;
            var normals = new Vector3[path.PathLocations.Length];
            for (var i = 1; i < nVerts - 1; ++i)
            {
                var v1 = Vector3.Subtract(path.PathLocations[i], path.PathLocations[i - 1]);
                normals[i] = Vector3.Normalize(Vector3.Cross(v1, tangents[i]));
            }
            
            normals[0] = normals[1];
            normals[nVerts - 1] = normals[nVerts - 2];

            return normals;
        }
        
        private Vector3[] ComputeBinormals(Vector3[] tangents, Vector3[] normals)
        {
            var nVerts = tangents.Length;
            var binormals = new Vector3[nVerts];
            for (var i = 0; i < nVerts; ++i)
            {
                binormals[i] = Vector3.Normalize(Vector3.Cross(tangents[i], normals[i]));
            }

            return binormals;
        }
    }
}