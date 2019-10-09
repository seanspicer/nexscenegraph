using System;
using System.Numerics;

namespace Veldrid.SceneGraph.Nodes
{
    public class CubeGeometry
    {
        public enum VertexType
        {
            Position3Texture2Color3Normal3
        }
        
        public enum TopologyType
        {
            IndexedTriangleList
        }
        
        public static IGeode Create(VertexType vertexType, TopologyType topologyType)
        {
            var geometry = Geometry<Position3Texture2Color3Normal3>.Create();

            var nl = 1f / (float)Math.Sqrt(3f);
            
            var vertices = new Position3Texture2Color3Normal3[]
            {
                // Top
                new Position3Texture2Color3Normal3(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0), new Vector3(1, 0, 0), new Vector3(-nl,  nl, -nl)),
                new Position3Texture2Color3Normal3(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0), new Vector3(1, 0, 0), new Vector3( nl,  nl, -nl)),
                new Position3Texture2Color3Normal3(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 1), new Vector3(1, 0, 0), new Vector3( nl,  nl,  nl)),
                new Position3Texture2Color3Normal3(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 1), new Vector3(1, 0, 0), new Vector3(-nl,  nl,  nl)),
                // Bottom                                                             
                new Position3Texture2Color3Normal3(new Vector3(-0.5f,-0.5f, +0.5f),  new Vector2(0, 0), new Vector3(1, 1, 0), new Vector3(-nl, -nl,  nl)),
                new Position3Texture2Color3Normal3(new Vector3(+0.5f,-0.5f, +0.5f),  new Vector2(1, 0), new Vector3(1, 1, 0), new Vector3( nl, -nl,  nl)),
                new Position3Texture2Color3Normal3(new Vector3(+0.5f,-0.5f, -0.5f),  new Vector2(1, 1), new Vector3(1, 1, 0), new Vector3( nl, -nl, -nl)),
                new Position3Texture2Color3Normal3(new Vector3(-0.5f,-0.5f, -0.5f),  new Vector2(0, 1), new Vector3(1, 1, 0), new Vector3(-nl, -nl, -nl)),
                // Left                                                               
                new Position3Texture2Color3Normal3(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0), new Vector3(0, 1, 0), new Vector3(-nl,  nl, -nl)),
                new Position3Texture2Color3Normal3(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(1, 0), new Vector3(0, 1, 0), new Vector3(-nl,  nl,  nl)),
                new Position3Texture2Color3Normal3(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(1, 1), new Vector3(0, 1, 0), new Vector3(-nl, -nl,  nl)),
                new Position3Texture2Color3Normal3(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(0, 1), new Vector3(0, 1, 0), new Vector3(-nl, -nl, -nl)),
                // Right                                                              
                new Position3Texture2Color3Normal3(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(0, 0), new Vector3(0, 1, 1), new Vector3( nl,  nl,  nl)),
                new Position3Texture2Color3Normal3(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0), new Vector3(0, 1, 1), new Vector3( nl,  nl, -nl)),
                new Position3Texture2Color3Normal3(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(1, 1), new Vector3(0, 1, 1), new Vector3( nl, -nl, -nl)),
                new Position3Texture2Color3Normal3(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(0, 1), new Vector3(0, 1, 1), new Vector3( nl, -nl,  nl)),
                // Back                                                               
                new Position3Texture2Color3Normal3(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(0, 0), new Vector3(0, 0, 1), new Vector3( nl,  nl, -nl)),
                new Position3Texture2Color3Normal3(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(1, 0), new Vector3(0, 0, 1), new Vector3(-nl,  nl, -nl)),
                new Position3Texture2Color3Normal3(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(1, 1), new Vector3(0, 0, 1), new Vector3(-nl, -nl, -nl)),
                new Position3Texture2Color3Normal3(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(0, 1), new Vector3(0, 0, 1), new Vector3( nl, -nl, -nl)),
                // Front                                                              
                new Position3Texture2Color3Normal3(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 0), new Vector3(1, 0, 1), new Vector3(-nl,  nl,  nl)),
                new Position3Texture2Color3Normal3(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 0), new Vector3(1, 0, 1), new Vector3( nl,  nl,  nl)),
                new Position3Texture2Color3Normal3(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(1, 1), new Vector3(1, 0, 1), new Vector3( nl, -nl,  nl)),
                new Position3Texture2Color3Normal3(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(0, 1), new Vector3(1, 0, 1), new Vector3(-nl, -nl,  nl)),
            };
            
            uint[] indices =
            {
                0,1,2, 0,2,3,
                4,5,6, 4,6,7,
                8,9,10, 8,10,11,
                12,13,14, 12,14,15,
                16,17,18, 16,18,19,
                20,21,22, 20,22,23,
            };
            
            geometry.VertexData = vertices;
            geometry.IndexData = indices;
            
            geometry.VertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Texture", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3));

            var pSet = DrawElements<Position3Texture2Color3Normal3>.Create(
                geometry, 
                PrimitiveTopology.TriangleList, 
                (uint)geometry.IndexData.Length, 
                1, 
                0, 
                0, 
                0);
            
            geometry.PrimitiveSets.Add(pSet);
            
            var geode = Geode.Create();
            geode.AddDrawable(geometry);
            return geode;
        }
    }
}