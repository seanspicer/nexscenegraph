namespace Veldrid.SceneGraph.Shaders
{
    public interface IShaderSet
    {
        string Name { get; }
        ShaderDescription VertexShaderDescription { get; }
        ShaderDescription FragmentShaderDescription { get; }
    }
}