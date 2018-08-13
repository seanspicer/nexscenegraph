using System.Numerics;
using Nsg.Core.Interfaces;

namespace Nsg.Core
{
    public class Geometry : Node
    {
        public IDeviceBuffer VertexBuffer { get; set; }
        public IDeviceBuffer IndexBuffer { get; set; }
        public IShader VertexShader { get; set; }
        public IShader FragmentShader { get; set; }
        
        
    }
}