using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Nsg.Core.Interfaces;
using Veldrid;

namespace Nsg.Core
{
    public enum PrimitiveTopolgy : byte
    {
        LineList, 
        LineStrip, 
        PointList,
        TriangleList,
        TriangleStrip
    }
    
    public class Geometry<T> : Node 
        where T : struct 
    {
        public byte[] VertexShader { get; set; }
        public byte[] FragmentShader { get; set; }

        public T[] VertexData { get; set; }
        public int SizeOfVertexData => Marshal.SizeOf(default(T));
        
        public ushort[] IndexData { get; set; }

        public PrimitiveTopolgy Topology { get; set; }

        public VertexLayoutDescription VertexLayout { get; set; }
            
        public Geometry()
        {
            Topology = PrimitiveTopolgy.TriangleList;
        }
        
        public override void Accept(IDrawVisitor drawVisitor)
        {
            // Do my thing...
            drawVisitor.Draw(this);
            
            base.Accept(drawVisitor);
        }
    }
}