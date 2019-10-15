using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reflection;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Util.Shape
{
    internal class PathGeometryBuilder<T> : GeometryBuilderBase<T> where T : struct, ISettablePrimitiveElement
    {
        internal void Build(IGeometry<T> geometry, ITessellationHints hints, Vector3[] colors, IPath path)
        {
            if (path.PathLocations.Length < 2)
            {
                throw new ArgumentException("Not enough vertices for a path");
            }

            if (hints.Radius < 0)
            {
                throw new ArgumentException("Negative radius is not valid");
            }
            
            if (System.Math.Abs(hints.Radius) < 1e-8)
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
            
            // Build from quad strips
            for (var j = 0; j < nSegments-1; ++j)
            {
                Begin();
                
                BuildFromIndicies(j, j+1, extrusion, path);
                
                End();
            }
            
            // Join the last bit...
            BuildFromIndicies(nSegments-1, 0, extrusion, path);
            
            BuildVertexAndIndexArrays(out var vertexArray, out var indexArray, colors);
            
            geometry.VertexData = vertexArray;
            geometry.IndexData = indexArray;
            
            geometry.VertexLayout = VertexLayoutHelpers.GetLayoutDescription(typeof(T));
            
            var pSet = DrawElements<T>.Create(
                geometry, 
                PrimitiveTopology.TriangleList, 
                (uint)geometry.IndexData.Length, 
                1, 
                0, 
                0, 
                0);
            
            geometry.PrimitiveSets.Add(pSet);
            
        }

        private void BuildFromIndicies(int a, int b, Vector3[,] extrusion, IPath path)
        {
            Begin();
                
            for (var i = 0; i < path.PathLocations.Length-1; ++i)
            {
                // C1
                var ctr0 = path.PathLocations[i];
                var vtx0 = extrusion[i, b];
                var nrm0 = Vector3.Normalize(Vector3.Subtract(vtx0, ctr0));
                    
                Vertex3f(vtx0);
                Normal3f(nrm0);
                TexCoord2f(1.0f, 0.0f);
                    
                // C2
                var vtx1 = extrusion[i, a];
                var nrm1 = Vector3.Normalize(Vector3.Subtract(vtx1, ctr0));
                    
                Vertex3f(vtx1);
                Normal3f(nrm1);
                TexCoord2f(1.0f, 0.0f);
                    
                // C3
                var ctr1 = path.PathLocations[i+1];
                var vtx2 = extrusion[i+1, b];
                var nrm2 = Vector3.Normalize(Vector3.Subtract(vtx2, ctr1));
                    
                Vertex3f(vtx2);
                Normal3f(nrm2);
                TexCoord2f(1.0f, 0.0f);
                    
                // C4
                var vtx3 = extrusion[i+1, a];
                var nrm3 = Vector3.Normalize(Vector3.Subtract(vtx3, ctr1));
                    
                Vertex3f(vtx3);
                Normal3f(nrm3);
                TexCoord2f(1.0f, 0.0f);
            }
                
            End();
        }
    }
}