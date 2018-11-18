using System;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Shaders.Standard
{
    public class Vertex2Color4Shader
    {
        private static readonly Lazy<Vertex2Color4Shader> Lazy = new Lazy<Vertex2Color4Shader>(() => new Vertex2Color4Shader());

        public static Vertex2Color4Shader Instance => Lazy.Value;

        public ShaderDescription VertexShaderDescription { get; }
        public ShaderDescription FragmentShaderDescription { get; }
        
        private Vertex2Color4Shader()
        {
            var vertexShaderBytes = ShaderTools.LoadShaderBytes(DisplaySettings.Instance.GraphicsBackend,
                typeof(Vertex2Color4Shader).Assembly,
                "Vertex2Color4ShaderSource", ShaderStages.Vertex);
            
            var fragmentShaderBytes = ShaderTools.LoadShaderBytes(DisplaySettings.Instance.GraphicsBackend,
                typeof(Vertex2Color4Shader).Assembly,
                "Vertex2Color4ShaderSource", ShaderStages.Fragment);
            
            VertexShaderDescription = new ShaderDescription(ShaderStages.Vertex, vertexShaderBytes, "VS");
            FragmentShaderDescription = new ShaderDescription(ShaderStages.Fragment, fragmentShaderBytes, "FS");
        }
    }
}