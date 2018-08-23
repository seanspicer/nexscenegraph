using System;
using System.IO;
using System.Numerics;
using Veldrid;
using Veldrid.SceneGraph;
using Veldrid.SceneGraph.Viewer;

namespace HelloNsg
{
    
    struct VertexPositionColor
    {
        public const uint SizeInBytes = 24;
        public Vector2 Position;
        public Vector4 Color;
        public VertexPositionColor(Vector2 position, Vector4 color)
        {
            Position = position;
            Color = color;
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var viewer = new SimpleViewer();

            var root = new Node();
            
            var geometry = new Geometry<VertexPositionColor>();
            
            VertexPositionColor[] quadVertices =
            {
                new VertexPositionColor(new Vector2(-.75f, .75f),  new Vector4(1.0f, 0.0f, 0.0f, 1.0f)),
                new VertexPositionColor(new Vector2(.75f, .75f),   new Vector4(0.0f, 1.0f, 0.0f, 1.0f)),
                new VertexPositionColor(new Vector2(-.75f, -.75f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new VertexPositionColor(new Vector2(.75f, -.75f),  new Vector4(1.0f, 1.0f, 0.0f, 1.0f))
            };

            geometry.VertexData = quadVertices;
            
            ushort[] quadIndices = { 0, 1, 2, 3 };
            geometry.IndexData = quadIndices;

            geometry.VertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float4));
            
            geometry.Topology = PrimitiveTopolgy.TriangleStrip;
            
            var vsPath = Path.Combine(System.AppContext.BaseDirectory, "Shaders", "Vertex.spv");
            geometry.VertexShader = File.ReadAllBytes(vsPath);
            
            var fsPath = Path.Combine(System.AppContext.BaseDirectory, "Shaders", "Fragment.spv");
            geometry.FragmentShader = File.ReadAllBytes(fsPath);
            
            root.Add(geometry);

            viewer.Root = root;

            viewer.Show();

        }
    }
}