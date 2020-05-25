//
// This file is part of IMAGEFrac (R) and related technologies.
//
// Copyright (c) 2017-2020 Reveal Energy Services.  All Rights Reserved.
//
// LEGAL NOTICE:
// IMAGEFrac contains trade secrets and otherwise confidential information
// owned by Reveal Energy Services. Access to and use of this information is 
// strictly limited and controlled by the Company. This file may not be copied,
// distributed, or otherwise disclosed outside of the Company's facilities 
// except under appropriate precautions to maintain the confidentiality hereof, 
// and may not be used in any way not expressly authorized by the Company.
//

using System;
using System.Collections.Generic;
using System.Numerics;
using SixLabors.ImageSharp.ColorSpaces;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Util.Shape
{
    internal class BuildCylinderGeometry<T> : GeometryBuilderBase<T> where T: struct, ISettablePrimitiveElement
    {
        internal void Build(IGeometry<T> geometry, ITessellationHints hints, Vector3[] colors, ICylinder cylinder)
        {
            const uint MIN_NUM_ROWS = 3;
            const uint MIN_NUM_SEGMENTS = 5;
            
            if (hints.NormalsType == NormalsType.PerFace)
            {
                throw new ArgumentException("Per-Face Normals are not supported for cylinder");
            }

            if (hints.ColorsType == ColorsType.ColorPerFace)
            {
                throw new ArgumentException("Per-Face Colors are not supported for cylinder");
            }

            if (hints.ColorsType == ColorsType.ColorPerVertex)
            {
                throw new ArgumentException("Per-Vertex Colors are not supported for cylinder");
            }

            if (colors.Length < 1)
            {
                throw new ArgumentException("Must provide at least one color for cylinder");
            }
            
            uint numSegments = 40;
            uint numRows = 20;

            var ratio = hints.DetailRatio;
            if (ratio > 0.0f && ratio != 1.0f) {
                numRows = (uint) (numRows * ratio);
                if (numRows < MIN_NUM_ROWS)
                    numRows = MIN_NUM_ROWS;
                numSegments = (uint) (numSegments * ratio);
                if (numSegments < MIN_NUM_SEGMENTS)
                    numSegments = MIN_NUM_SEGMENTS;
            }

            var r = cylinder.Radius;
            var h = cylinder.Height;

            var basez = -h*0.5f;
            var topz = h*0.5f;

            var angle = 0.0f;
            var texCoord = 0.0f;
            
            var angleDelta = (float)(2.0f*System.Math.PI/(float)numSegments);
            var texCoordDelta = 1.0f / (float) numSegments;
            
            BeginQuadStrip();
            if (hints.CreateFrontFace)
            {
                for(uint bodyi=0;
                bodyi<numSegments;
                ++bodyi,angle+=angleDelta,texCoord+=texCoordDelta)
                {
                    var c = (float)System.Math.Cos(angle);
                    var s = (float)System.Math.Sin(angle);
                    var n = new Vector3(c,s,0.0f);

                    Normal3f(n);
                    TexCoord2f(texCoord,1.0f);
                    Vertex3f(c*r,s*r,topz);

                    Normal3f(n);
                    TexCoord2f(texCoord,0.0f);
                    Vertex3f(c*r,s*r,basez);
                }

                // do last point by hand to ensure no round off errors.
                var lastN = new Vector3(1.0f, 0.0f, 0.0f);

                Normal3f(lastN);
                TexCoord2f(1.0f,1.0f);
                Vertex3f(r,0.0f,topz);

                Normal3f(lastN);
                TexCoord2f(1.0f,0.0f); 
                Vertex3f(r,0.0f,basez);
            }

            if (hints.CreateBackFace)
            {
                for(uint bodyi=0;
                    bodyi<numSegments;
                    ++bodyi,angle+=angleDelta,texCoord+=texCoordDelta)
                {
                    var c = (float)System.Math.Cos(angle);
                    var s = (float)System.Math.Sin(angle);
                    var n = new Vector3(-c,-s,0.0f);

                    Normal3f(n);
                    TexCoord2f(texCoord,1.0f);
                    Vertex3f(c*r,s*r,topz);

                    Normal3f(n);
                    TexCoord2f(texCoord,0.0f);
                    Vertex3f(c*r,s*r,basez);
                }

                // do last point by hand to ensure no round off errors.
                var lastN = new Vector3(-1.0f, 0.0f, 0.0f);

                Normal3f(lastN);
                TexCoord2f(1.0f,1.0f);
                Vertex3f(r,0.0f,topz);

                Normal3f(lastN);
                TexCoord2f(1.0f,0.0f); 
                Vertex3f(r,0.0f,basez);
            }

            End();

            if (hints.CreateTop)
            {
                BeginTriangleFan();

                Normal3f(0.0f,0.0f,1.0f);
                TexCoord2f(0.5f,0.5f);
                Vertex3f(0.0f,0.0f,topz);

                angle = 0.0f;
                texCoord = 0.0f;
                for(uint topi=0;
                topi<numSegments;
                ++topi,angle+=angleDelta,texCoord+=texCoordDelta)
                {
                    var c = (float)System.Math.Cos(angle);
                    var s = (float)System.Math.Sin(angle);

                    Normal3f(0.0f,0.0f,1.0f);
                    TexCoord2f(c*0.5f+0.5f,s*0.5f+0.5f);
                    Vertex3f(s*r,c*r,topz);
                }

                Normal3f(0.0f,0.0f,1.0f);
                TexCoord2f(1.0f,0.5f);
                Vertex3f(0.0f,r,topz);

                End();
            }

            if (hints.CreateBottom) 
            {
                BeginTriangleFan();

                Normal3f(0.0f,0.0f,-1.0f);
                TexCoord2f(0.5f,0.5f);
                Vertex3f(0.0f,0.0f,basez);

                angle = (float)System.Math.PI*2.0f;
                texCoord = 1.0f;
                for(uint bottomi=0;
                    bottomi<numSegments;
                    ++bottomi,angle-=angleDelta,texCoord-=texCoordDelta)
                {
                    var c = (float)System.Math.Cos(angle);
                    var s = (float)System.Math.Sin(angle);

                    Normal3f(0.0f,0.0f,-1.0f);
                    TexCoord2f(c*0.5f+0.5f,s*0.5f+0.5f);
                    Vertex3f(s*r,c*r,basez);
                }

                Normal3f(0.0f,0.0f,-1.0f);
                TexCoord2f(1.0f,0.5f);
                Vertex3f(0.0f,r,basez);

                End();
            }
            
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
    }
}