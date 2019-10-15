using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reactive;
using System.Reactive.Subjects;
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
            var tangents = Util.Math.ComputePathTangents(path.PathLocations);

            var nSegments = (int)System.Math.Floor(10f * hints.DetailRatio);
            if (nSegments < 4) nSegments = 4;

            var shape = new Vector3[nSegments];
            for (var i = 0; i < nSegments; ++i)
            {
                var theta = i * 2 * System.Math.PI / nSegments;
                var r = hints.Radius;

                shape[i] = new Vector3((float)(r*System.Math.Sin(theta)), (float)(r*System.Math.Cos(theta)), 0.0f);
            }

            var extrusion = new Vector3[path.PathLocations.Length, nSegments];
            
            var axialVec = Vector3.UnitZ;
            for (var i = 0; i < path.PathLocations.Length; ++i)
            {
                var unitTangent = Vector3.Normalize(tangents[i]);
                var z = Vector3.Cross(axialVec, unitTangent);

                if (System.Math.Abs(z.Length()) > 1e-6)
                {
                    // Determine the required rotation, and build quaternion
                    var znorm = Vector3.Normalize(z);
                    var q = System.Math.Acos(Vector3.Dot(axialVec, unitTangent) / axialVec.Length());
                    var quat = Quaternion.CreateFromAxisAngle(znorm, (float)q);

                    // Transform shape by quaternion.
                    for (var j = 0; j < shape.Length; ++j)
                    {
                        shape[j] = Vector3.Transform(shape[j], quat);
                    }

                    axialVec = unitTangent;
                }

                for (var j = 0; j < shape.Length; ++j)
                {
                    extrusion[i, j] = path.PathLocations[i] + shape[j];
                }
            }
            
            geometry.VertexLayout = VertexLayoutHelpers.GetLayoutDescription(typeof(T));
            
            var vertexDataList = new List<T>();
            var indexDataList = new List<uint>();
            
            for (var j = 0; j < nSegments; ++j)
            {
                // Draw a line
                for (var i = 0; i < path.PathLocations.Length; ++i)
                {
                    var vtx = new T();
                    vtx.SetPosition(extrusion[i,j]);
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
    }
}