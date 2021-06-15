using System.Reflection;

namespace Veldrid.SceneGraph.Shaders
{
    public interface IShaderSet
    {
        string Name { get; }
        ShaderDescription VertexShaderDescription { get; }
        ShaderDescription FragmentShaderDescription { get; }
    }

    public interface IEmbeddedShaderSet : IShaderSet
    {
        Assembly Assembly { get; }
    }
}