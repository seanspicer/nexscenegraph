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
                vtx.SetTexCoord2(Vector2.Zero);
                vtx.SetColor3(colors[0]);
                vertexDataList.Add(vtx);
                
                indexDataList.Add((uint)i);
            }
            
            geometry.VertexData = vertexDataList.ToArray();
            geometry.IndexData = indexDataList.ToArray();
            
            geometry.VertexLayouts = new List<VertexLayoutDescription>()
                {VertexLayoutHelpers.GetLayoutDescription(typeof(T))};
            
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

            var shape = new Vector2[nSegments];
            for (var i = 0; i < nSegments; ++i)
            {
                var theta = i * 2 * System.Math.PI / nSegments;
                var r = hints.Radius;

                shape[i] = new Vector2((float)(r*System.Math.Sin(theta)), (float)(r*System.Math.Cos(theta)));
            }

            var extrusion =  Util.Math.ExtrudeShape(shape, path.PathLocations);

            // Build from quad strips
            for (var j = 0; j < nSegments-1; ++j)
            {
                BeginQuadStrip();
                
                BuildFromIndicies(j, j+1, extrusion, path);
                
                End();
            }
            
            // Join the last bit...
            BuildFromIndicies(nSegments-1, 0, extrusion, path);

            if (hints.CreateEndCaps)
            {
                // Build end caps
                BuildEndCaps(extrusion, path); 
            }
            
            BuildVertexAndIndexArrays(out var vertexArray, out var indexArray, colors);
            
            geometry.VertexData = vertexArray;
            geometry.IndexData = indexArray;

            geometry.VertexLayouts = new List<VertexLayoutDescription>()
                {VertexLayoutHelpers.GetLayoutDescription(typeof(T))};
            
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
            BeginQuadStrip();
                
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

        private void BuildEndCaps(Vector3[,] extrusion, IPath path)
        {
            BuildEndCap(0, extrusion, path);
            BuildEndCap(path.PathLocations.Length-1, extrusion, path);
        }
        
        private void BuildEndCap(int idx, Vector3[,] extrusion, IPath path) 
        {
            var npts = extrusion.GetLength(1);
            
            // Begin
            var beginCtr = path.PathLocations[idx];
            
            // Begin Normal
            var beginVtx0 = extrusion[idx, 0];
            var beginVtx1 = extrusion[idx, 1];

            var bV1 = beginVtx0 - beginCtr;
            var bV2 = beginVtx1 - beginCtr;
            var bNrm = Vector3.Normalize(Vector3.Cross(bV1, bV2));

            if (idx > 0)
            {
                bNrm = -bNrm;
            }
            
            BeginTriangleFan();
            
            Vertex3f(beginCtr);
            Normal3f(bNrm);
            TexCoord2f(1.0f, 0.0f);
            
            for (var i = 0; i < npts; ++i)
            {
                Vertex3f(extrusion[idx, i]);
                Normal3f(bNrm);
                TexCoord2f(1.0f, 0.0f);
            }

            Vertex3f(extrusion[idx, 0]);
            Normal3f(bNrm);
            TexCoord2f(1.0f, 0.0f);
            
            End();

        }
    }
}