using System;
using System.Numerics;
using Nsg.Viewer;
using Nsg.Core;

namespace HelloNsg
{
    class Program
    {
        static void Main(string[] args)
        {
            var viewer = new SimpleViewer();

            var root = new Node();
            
            var geometry = new Geometry();
            Vector3[] quadVertices =
            {
                new Vector3(-1.0f, 0.0f, 0.0f),
                new Vector3( 1.0f, 1.0f, 0.0f),
                new Vector3(-1.0f,-1.0f, 0.0f),
                new Vector3( 1.0f,-1.0f, 0.0f)
            };

            geometry.VertexBuffer = quadVertices;

            root.Add(geometry);

            viewer.Root = root;

            viewer.Show();

        }
    }
}