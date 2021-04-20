namespace Veldrid.SceneGraph.Shaders
{
    public class ShaderSet : IShaderSet
    {
        private ShaderSet(string name, ShaderDescription vertexShader, ShaderDescription fragmentShader)
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
}