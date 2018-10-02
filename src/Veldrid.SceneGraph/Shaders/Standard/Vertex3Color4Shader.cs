using System;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Shaders.Standard
{
    public class Vertex3Color4Shader
    {
        private static readonly Lazy<Vertex3Color4Shader> Lazy = new Lazy<Vertex3Color4Shader>(() => new Vertex3Color4Shader());

        public static Vertex3Color4Shader Instance => Lazy.Value;

        public ShaderDescription VertexShaderDescription { get; }
        public ShaderDescription FragmentShaderDescription { get; }
        
        private Vertex3Color4Shader()
        {
            var vertexShaderBytes = ShaderTools.LoadShaderBytes(DisplaySettings.Instance.GraphicsBackend,
                typeof(Vertex3Color4Shader).Assembly,
                "Vertex3Color4ShaderSource", ShaderStages.Vertex);
            
            var fragmentShaderBytes = ShaderTools.LoadShaderBytes(DisplaySettings.Instance.GraphicsBackend,
                typeof(Vertex3Color4Shader).Assembly,
                "Vertex3Color4ShaderSource", ShaderStages.Fragment);
            
            VertexShaderDescription = new ShaderDescription(ShaderStages.Vertex, vertexShaderBytes, "VS");
            FragmentShaderDescription = new ShaderDescription(ShaderStages.Fragment, fragmentShaderBytes, "FS");
        }
    }
}