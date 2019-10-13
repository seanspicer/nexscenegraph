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
            if (path.PathLocations.Length < 2)
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
            var radii = ComputeRadii(path, hints.Radius);

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
                    p += (float)System.Math.Sin(theta)*r*normals[i] + (float)System.Math.Cos(theta)*radii[i]*binormals[i];
                    
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

            var defaultNormal = false;
            
            // Calculate the 1st normal
            var v0 = Vector3.Subtract(path.PathLocations[1], path.PathLocations[0]);
            var c0 = Vector3.Cross(v0, tangents[0]);
            if (c0 == Vector3.Zero)
            {
                var r = Math.CalculateRotationBetweenVectors(Vector3.UnitZ, v0);
                var n = Vector3.Transform(Vector3.UnitX, r);
                normals[0] = Vector3.Normalize(n);
                defaultNormal = true;
            }
            else
            {
                normals[0] = Vector3.Normalize(c0);
                defaultNormal = false;
            }
            
            for (var i = 1; i < nVerts - 1; ++i)
            {
                var v1 = Vector3.Subtract(path.PathLocations[i], path.PathLocations[i - 1]);
                var crossProduct = Vector3.Cross(v1, tangents[i]);
                
                // Handle situation where tangent is equal to the segment vector
                if (crossProduct == Vector3.Zero)
                {
                    normals[i] = normals[i - 1];
                }
                else
                {
                    normals[i] = Vector3.Normalize(crossProduct);
                    if (defaultNormal)
                    {
                        for (var j = i - 1; j >= 0; --j)
                        {
                            normals[j] = normals[i];
                        }

                        defaultNormal = false;
                    }
                }
                
            }
            
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

        private float[] ComputeRadii(IPath path, float baseRadius)
        {
            var nVerts = path.PathLocations.Length;
            var radii = new float[path.PathLocations.Length];

            radii[0] = baseRadius;
            for (var i = 1; i < nVerts - 1; ++i)
            {
                var v1 = Vector3.Subtract(path.PathLocations[i + 1],path.PathLocations[i]);
                var v2 = Vector3.Subtract(path.PathLocations[i],path.PathLocations[i-1]);
                var dp = Vector3.Dot(v1, v2);

                radii[i] = baseRadius * ((dp) + (1.0f - dp) * (float)System.Math.Sqrt(2));
            }
            radii[nVerts-1] = baseRadius;

            return radii;
        }
    }
}