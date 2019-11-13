namespace Veldrid.SceneGraph.Shaders
{
    public class ShaderSet : IShaderSet
    {
        public string Name { get; private set; }
        public ShaderDescription VertexShaderDescription { get; private set; }
        public ShaderDescription FragmentShaderDescription { get; private set; }

        public static IShaderSet Create(string name, ShaderDescription vertexShader, ShaderDescription fragmentShader)
        {
            return new ShaderSet(name, vertexShader, fragmentShader);
        }

        private ShaderSet(string name, ShaderDescription vertexShader, ShaderDescription fragmentShader)
        {
            Name = name;
            VertexShaderDescription = vertexShader;
            FragmentShaderDescription = fragmentShader;
        }
    }
}