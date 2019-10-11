using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Util.Shape
{
    public class BuildShapeGeometryVisitor<T> : IShapeVisitor where T : struct, ISettablePrimitiveElement
    {
        private IGeometry<T> _geometry;
        private ITessellationHints _tessellationHints;
        private Vector3 [] _colors;
        
        
        internal IShapeVisitor Create(IGeometry<T> geometry, ITessellationHints tessellationHints, Vector3 [] colors)
        {
            return new BuildShapeGeometryVisitor<T>(geometry, tessellationHints, colors);
        }

        internal BuildShapeGeometryVisitor(IGeometry<T> geometry, ITessellationHints tessellationHints, Vector3 [] colors)
        {
            _geometry = geometry;
            _tessellationHints = tessellationHints;
            _colors = colors;
        }

        void SetMatrix(Matrix4x4 m)
        {
            throw new System.NotImplementedException();
        }
        
        public void Apply(IShape shape)
        {
            throw new System.NotImplementedException();
        }

        public void Apply(IBox box)
        {
            var dx = box.HalfLengths.X;
            var dy = box.HalfLengths.Y;
            var dz = box.HalfLengths.Z;

            var nl = 1f / (float)System.Math.Sqrt(3f);

            var vertices = new T[24];
            
            // Top
            vertices[0].SetPosition(new Vector3(-dx, +dy, -dz));
            vertices[0].SetTexCoord(new Vector2(+0,  +0));
            
            vertices[1].SetPosition(new Vector3(+dx, +dy, -dz));
            vertices[1].SetTexCoord(new Vector2(+1,  +0));
            
            vertices[2].SetPosition(new Vector3(+dx, +dy, +dz));
            vertices[2].SetTexCoord(new Vector2(+1,  +1));
            
            vertices[3].SetPosition(new Vector3(-dx, +dy, +dz));
            vertices[3].SetTexCoord(new Vector2(+0,  +1));
            
            if (_tessellationHints.NormalsType == NormalsType.PerFace)
            {
                vertices[0].SetNormal(  new Vector3(0, 1, 0));
                vertices[1].SetNormal(  new Vector3(0, 1, 0));
                vertices[2].SetNormal(  new Vector3(0, 1, 0));
                vertices[3].SetNormal(  new Vector3(0, 1, 0));
            }
            else
            {
                vertices[0].SetNormal(  new Vector3(-nl, +nl, -nl));
                vertices[1].SetNormal(  new Vector3(+nl, +nl, -nl));
                vertices[2].SetNormal(  new Vector3(+nl, +nl, +nl));
                vertices[3].SetNormal(  new Vector3(-nl, +nl, +nl));
            }

            if (_tessellationHints.ColorsType == ColorsType.ColorOverall)
            {
                vertices[0].SetColor3(_colors[0]);
                vertices[1].SetColor3(_colors[0]);
                vertices[2].SetColor3(_colors[0]);
                vertices[3].SetColor3(_colors[0]);
            }
            else if (_tessellationHints.ColorsType == ColorsType.ColorPerFace)
            {
                vertices[0].SetColor3(_colors[0]);
                vertices[1].SetColor3(_colors[0]);
                vertices[2].SetColor3(_colors[0]);
                vertices[3].SetColor3(_colors[0]);
            }
            else
            {
                vertices[0].SetColor3(_colors[0]);
                vertices[1].SetColor3(_colors[1]);
                vertices[2].SetColor3(_colors[2]);
                vertices[3].SetColor3(_colors[3]);
            }
            
            // Bottom  
            vertices[4].SetPosition(new Vector3(-dx, -dy, +dz));
            vertices[4].SetTexCoord(new Vector2(+0,  +0));
            
            vertices[5].SetPosition(new Vector3(+dx, -dy, +dz));
            vertices[5].SetTexCoord(new Vector2(+1,  +0));
            
            vertices[6].SetPosition(new Vector3(+dx, -dy, -dz));
            vertices[6].SetTexCoord(new Vector2(+1,  +1));
            
            vertices[7].SetPosition(new Vector3(-dx, -dy, -dz));
            vertices[7].SetTexCoord(new Vector2(+0,  +1));

            if (_tessellationHints.NormalsType == NormalsType.PerFace)
            {
                vertices[4].SetNormal(  new Vector3(0, -1, 0));
                vertices[5].SetNormal(  new Vector3(0, -1, 0));
                vertices[6].SetNormal(  new Vector3(0, -1, 0));
                vertices[7].SetNormal(  new Vector3(0, -1, 0));
            }
            else
            {
                vertices[4].SetNormal(  new Vector3(-nl, -nl, +nl));
                vertices[5].SetNormal(  new Vector3(+nl, -nl, +nl));
                vertices[6].SetNormal(  new Vector3(+nl, -nl, -nl));
                vertices[7].SetNormal(  new Vector3(-nl, -nl, -nl));
            }
            
            if (_tessellationHints.ColorsType == ColorsType.ColorOverall)
            {
                vertices[4].SetColor3(_colors[0]);
                vertices[5].SetColor3(_colors[0]);
                vertices[6].SetColor3(_colors[0]);
                vertices[7].SetColor3(_colors[0]);
            }
            else if (_tessellationHints.ColorsType == ColorsType.ColorPerFace)
            {
                vertices[4].SetColor3(_colors[1]);
                vertices[5].SetColor3(_colors[1]);
                vertices[6].SetColor3(_colors[1]);
                vertices[7].SetColor3(_colors[1]);
            }
            else
            {
                vertices[4].SetColor3(_colors[4]);
                vertices[5].SetColor3(_colors[5]);
                vertices[6].SetColor3(_colors[6]);
                vertices[7].SetColor3(_colors[7]);
            }
            
            // Left  
            vertices[8].SetPosition(new Vector3(-dx, +dy, -dz));
            vertices[8].SetTexCoord(new Vector2(+0,  +0));
            
            vertices[9].SetPosition(new Vector3(-dx, +dy, +dz));
            vertices[9].SetTexCoord(new Vector2(+1,  +0));
            
            vertices[10].SetPosition(new Vector3(-dx, -dy, +dz));
            vertices[10].SetTexCoord(new Vector2(+1,  +1));
            
            vertices[11].SetPosition(new Vector3(-dx, -dy, -dz));
            vertices[11].SetTexCoord(new Vector2(+0,  +1));
            
            if (_tessellationHints.NormalsType == NormalsType.PerFace)
            {
                vertices[8].SetNormal(new Vector3(-1, 0, 0));
                vertices[9].SetNormal(new Vector3(-1, 0, 0));
                vertices[10].SetNormal(new Vector3(-1, 0, 0));
                vertices[11].SetNormal(new Vector3(-1, 0, 0));
            }
            else
            {
                vertices[8].SetNormal(new Vector3(-nl, +nl, -nl));
                vertices[9].SetNormal(new Vector3(-nl, +nl, +nl));
                vertices[10].SetNormal(new Vector3(-nl, -nl, +nl));
                vertices[11].SetNormal(new Vector3(-nl, -nl, -nl));
            }
            
            if (_tessellationHints.ColorsType == ColorsType.ColorOverall)
            {
                vertices[8].SetColor3(_colors[0]);
                vertices[9].SetColor3(_colors[0]);
                vertices[10].SetColor3(_colors[0]);
                vertices[11].SetColor3(_colors[0]);
            }
            else if (_tessellationHints.ColorsType == ColorsType.ColorPerFace)
            {
                vertices[8].SetColor3(_colors[2]);
                vertices[9].SetColor3(_colors[2]);
                vertices[10].SetColor3(_colors[2]);
                vertices[11].SetColor3(_colors[2]);
            }
            else
            {
                vertices[8].SetColor3(_colors[8]);
                vertices[9].SetColor3(_colors[9]);
                vertices[10].SetColor3(_colors[10]);
                vertices[11].SetColor3(_colors[11]);
            }
            
            // Right   
            vertices[12].SetPosition(new Vector3(+dx, +dy, +dz));
            vertices[12].SetTexCoord(new Vector2(+0,  +0));
            
            vertices[13].SetPosition(new Vector3(+dx, +dy, -dz));
            vertices[13].SetTexCoord(new Vector2(+1,  +0));
            
            vertices[14].SetPosition(new Vector3(+dx, -dy, -dz));
            vertices[14].SetTexCoord(new Vector2(+1,  +1));
            
            vertices[15].SetPosition(new Vector3(+dx, -dy, +dz));
            vertices[15].SetTexCoord(new Vector2(+0,  +1));
            
            if (_tessellationHints.NormalsType == NormalsType.PerFace)
            {
                vertices[12].SetNormal(new Vector3(1, 0, 0));
                vertices[13].SetNormal(new Vector3(1, 0, 0));
                vertices[14].SetNormal(new Vector3(1, 0, 0));
                vertices[15].SetNormal(new Vector3(1, 0, 0));
            }
            else
            {
                vertices[12].SetNormal(  new Vector3(+nl, +nl, +nl));
                vertices[13].SetNormal(  new Vector3(+nl, +nl, -nl));
                vertices[14].SetNormal(  new Vector3(+nl, -nl, -nl));
                vertices[15].SetNormal(  new Vector3(+nl, -nl, +nl));
            }
            
            if (_tessellationHints.ColorsType == ColorsType.ColorOverall)
            {
                vertices[12].SetColor3(_colors[0]);
                vertices[13].SetColor3(_colors[0]);
                vertices[14].SetColor3(_colors[0]);
                vertices[15].SetColor3(_colors[0]);
            }
            else if (_tessellationHints.ColorsType == ColorsType.ColorPerFace)
            {
                vertices[12].SetColor3(_colors[3]);
                vertices[13].SetColor3(_colors[3]);
                vertices[14].SetColor3(_colors[3]);
                vertices[15].SetColor3(_colors[3]);
            }
            else
            {
                vertices[12].SetColor3(_colors[12]);
                vertices[13].SetColor3(_colors[13]);
                vertices[14].SetColor3(_colors[14]);
                vertices[15].SetColor3(_colors[15]);
            }
            
            // Back  
            vertices[16].SetPosition(new Vector3(+dx, +dy, -dz));
            vertices[16].SetTexCoord(new Vector2(+0,  +0));
            
            vertices[17].SetPosition(new Vector3(-dx, +dy, -dz));
            vertices[17].SetTexCoord(new Vector2(+1,  +0));
            
            vertices[18].SetPosition(new Vector3(-dx, -dy, -dz));
            vertices[18].SetTexCoord(new Vector2(+1,  +1));
            
            vertices[19].SetPosition(new Vector3(+dx, -dy, -dz));
            vertices[19].SetTexCoord(new Vector2(+0,  +1));
            
            if (_tessellationHints.NormalsType == NormalsType.PerFace)
            {
                vertices[16].SetNormal(new Vector3(0, 0, -1));
                vertices[17].SetNormal(new Vector3(0, 0, -1));
                vertices[18].SetNormal(new Vector3(0, 0, -1));
                vertices[19].SetNormal(new Vector3(0, 0, -1));
            }
            else
            {
                vertices[16].SetNormal(  new Vector3(+nl, +nl, -nl));
                vertices[17].SetNormal(  new Vector3(-nl, +nl, -nl));
                vertices[18].SetNormal(  new Vector3(-nl, -nl, -nl));
                vertices[19].SetNormal(  new Vector3(+nl, -nl, -nl));
            }
            
            if (_tessellationHints.ColorsType == ColorsType.ColorOverall)
            {
                vertices[16].SetColor3(_colors[0]);
                vertices[17].SetColor3(_colors[0]);
                vertices[18].SetColor3(_colors[0]);
                vertices[19].SetColor3(_colors[0]);
            }
            else if (_tessellationHints.ColorsType == ColorsType.ColorPerFace)
            {
                vertices[16].SetColor3(_colors[4]);
                vertices[17].SetColor3(_colors[4]);
                vertices[18].SetColor3(_colors[4]);
                vertices[19].SetColor3(_colors[4]);
            }
            else
            {
                vertices[16].SetColor3(_colors[16]);
                vertices[17].SetColor3(_colors[17]);
                vertices[18].SetColor3(_colors[18]);
                vertices[19].SetColor3(_colors[19]);
            }
            
            // Front   
            vertices[20].SetPosition(new Vector3(-dx, +dy, +dz));
            vertices[20].SetTexCoord(new Vector2(+0,  +0));
            
            vertices[21].SetPosition(new Vector3(+dx, +dy, +dz));
            vertices[21].SetTexCoord(new Vector2(+1,  +0));
            
            vertices[22].SetPosition(new Vector3(+dx, -dy, +dz));
            vertices[22].SetTexCoord(new Vector2(+1,  +1));
            
            vertices[23].SetPosition(new Vector3(-dx, -dy, +dz));
            vertices[23].SetTexCoord(new Vector2(+0,  +1));
            
            if (_tessellationHints.NormalsType == NormalsType.PerFace)
            {
                vertices[20].SetNormal(new Vector3(0, 0, 1));
                vertices[21].SetNormal(new Vector3(0, 0, 1));
                vertices[22].SetNormal(new Vector3(0, 0, 1));
                vertices[23].SetNormal(new Vector3(0, 0, 1));
            }
            else
            {
                vertices[20].SetNormal(  new Vector3(-nl, +nl, +nl));
                vertices[21].SetNormal(  new Vector3(+nl, +nl, +nl));
                vertices[22].SetNormal(  new Vector3(+nl, -nl, +nl));
                vertices[23].SetNormal(  new Vector3(-nl, -nl, +nl));
            }
            
            if (_tessellationHints.ColorsType == ColorsType.ColorOverall)
            {
                vertices[20].SetColor3(_colors[0]);
                vertices[21].SetColor3(_colors[0]);
                vertices[22].SetColor3(_colors[0]);
                vertices[23].SetColor3(_colors[0]);
            }
            else if (_tessellationHints.ColorsType == ColorsType.ColorPerFace)
            {
                vertices[20].SetColor3(_colors[5]);
                vertices[21].SetColor3(_colors[5]);
                vertices[22].SetColor3(_colors[5]);
                vertices[23].SetColor3(_colors[5]);
            }
            else
            {
                vertices[20].SetColor3(_colors[20]);
                vertices[21].SetColor3(_colors[21]);
                vertices[22].SetColor3(_colors[22]);
                vertices[23].SetColor3(_colors[23]);
            }

            _geometry.VertexData = vertices;
            
            uint[] indices =
            {
                0,1,2, 0,2,3,
                4,5,6, 4,6,7,
                8,9,10, 8,10,11,
                12,13,14, 12,14,15,
                16,17,18, 16,18,19,
                20,21,22, 20,22,23,
            };

            _geometry.IndexData = indices;

            _geometry.VertexLayout = VertexLayoutHelpers.GetLayoutDescription(typeof(T));
            
            var pSet = DrawElements<T>.Create(
                _geometry, 
                PrimitiveTopology.TriangleList, 
                (uint)_geometry.IndexData.Length, 
                1, 
                0, 
                0, 
                0);
            
            _geometry.PrimitiveSets.Add(pSet);
        }
        
        public void Apply(ISphere sphere)
        {
            throw new System.NotImplementedException();
        }
    }
}