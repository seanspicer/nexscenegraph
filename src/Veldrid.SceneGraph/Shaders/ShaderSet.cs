using System.Reflection;

namespace Veldrid.SceneGraph.Shaders
{
    public class ShaderSet : IShaderSet
    {
        protected ShaderSet(string name, ShaderDescription vertexShader, ShaderDescription fragmentShader)
        {
            Name = name;
            VertexShaderDescription = vertexShader;
            FragmentShaderDescription = fragmentShader;
        }

        public string Name { get; }
        public ShaderDescription VertexShaderDescription { get; }
        public ShaderDescription FragmentShaderDescription { get; }

        public static IShaderSet Create(string name, ShaderDescription vertexShader, ShaderDescription fragmentShader)
        {
            return new ShaderSet(name, vertexShader, fragmentShader);
        }
    }

    public class EmbeddedShaderSet : IEmbeddedShaderSet
    {
        public string Name { get; }
        public ShaderDescription VertexShaderDescription { get; }
        public ShaderDescription FragmentShaderDescription { get; }
        
        public Assembly Assembly { get; }
        
        protected EmbeddedShaderSet(string name, Assembly assembly)
        {
            Name = name;
            Assembly = assembly;
        }
        public static IEmbeddedShaderSet Create(string name, Assembly assembly)
        {
            return new EmbeddedShaderSet(name, assembly);
        }
    }
}