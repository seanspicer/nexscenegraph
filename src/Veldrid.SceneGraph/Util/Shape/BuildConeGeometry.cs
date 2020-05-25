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
    internal class BuildConeGeometry<T> : GeometryBuilderBase<T> where T: struct, ISettablePrimitiveElement
    {
        internal void Build(IGeometry<T> geometry, ITessellationHints hints, Vector3[] colors, ICone cone)
        {
            const uint MIN_NUM_ROWS = 3;
            const uint MIN_NUM_SEGMENTS = 5;
            
            if (hints.NormalsType == NormalsType.PerFace)
            {
                throw new ArgumentException("Per-Face Normals are not supported for cones");
            }

            if (hints.ColorsType == ColorsType.ColorPerFace)
            {
                throw new ArgumentException("Per-Face Colors are not supported for cones");
            }

            if (hints.ColorsType == ColorsType.ColorPerVertex)
            {
                throw new ArgumentException("Per-Vertex Colors are not supported for cones");
            }

            if (colors.Length < 1)
            {
                throw new ArgumentException("Must provide at least one color for cones");
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

            var r = cone.Radius;
            var h = cone.Height;

            var normalz = (float)(r/(System.Math.Sqrt(r*r+h*h)));
            var normalRatio = (float)(1.0f/(System.Math.Sqrt(1.0f+normalz*normalz)));
            normalz *= normalRatio;

            var angleDelta = (float)(2.0f*System.Math.PI/(float)numSegments);
            var texCoordHorzDelta = 1.0f/(float)numSegments;
            var texCoordRowDelta = (float)(1.0f/(float)numRows);
            var hDelta = (cone.Height)/(float)numRows;
            var rDelta = cone.Radius/(float)numRows;

            var topz=cone.Center.Z + cone.Height;
            var topr=0.0f;
            var topv=1.0f;
            var basez = topz-hDelta;
            var baser=rDelta;
            var basev=(float) (topv-texCoordRowDelta);
            float angle;
            float texCoord;
            
            if (hints.CreateBody)
            {
                for(uint rowi=0; rowi<numRows; 
                    ++rowi, topz=basez, basez -= hDelta, topr=baser, baser+=rDelta, topv=basev, basev-=texCoordRowDelta)
                {
                    BeginQuadStrip();

                    angle = 0.0f;
                    texCoord = 0.0f;

                    for(uint topi=0; topi<numSegments;
                    ++topi,angle+=angleDelta,texCoord+=texCoordHorzDelta)
                    {
                        var c = (float)System.Math.Cos(angle);
                        var s = (float)System.Math.Sin(angle);

                        Normal3f((c*normalRatio),s*normalRatio,normalz);
                        TexCoord2f(texCoord,topv);
                        Vertex3f(c*topr,s*topr,topz);

                        Normal3f(c*normalRatio,s*normalRatio,normalz);
                        TexCoord2f(texCoord,basev);
                        Vertex3f(c*baser,s*baser,basez);
                    }
                    
                    // do last point by hand to ensure no round off errors.
                    Normal3f(normalRatio,0.0f,normalz);
                    TexCoord2f(1.0f,topv);
                    Vertex3f(topr,0.0f,topz);

                    Normal3f(normalRatio,0.0f,normalz);
                    TexCoord2f(1.0f,basev);
                    Vertex3f(baser,0.0f,basez);

                    End(); 
                }
                
                if (hints.CreateBottom) {
                    
                    BeginTriangleFan();

                    angle = (float) System.Math.PI*2.0f;
                    texCoord = 1.0f;
                    basez = cone.Center.Z;

                    Normal3f(0.0f,0.0f,-1.0f);
                    TexCoord2f(0.5f,0.5f);
                    Vertex3f(0.0f,0.0f,basez);

                    for(uint bottomi=0; bottomi<numSegments;
                        ++bottomi,angle-=angleDelta,texCoord-=texCoordHorzDelta) {

                        var c = (float)System.Math.Cos(angle);
                        var s = (float)System.Math.Sin(angle);

                        Normal3f(0.0f,0.0f,-1.0f);
                        TexCoord2f(c*0.5f+0.5f,s*0.5f+0.5f);
                        Vertex3f(s*r,c*r,basez);
                    }

                    Normal3f(0.0f,0.0f,-1.0f);
                    TexCoord2f(1.0f,0.0f);
                    Vertex3f(0.0f,r,basez);

                    End();
                }
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