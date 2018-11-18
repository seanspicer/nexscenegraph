using SixLabors.Fonts;

namespace Veldrid.SceneGraph.Text
{
    public interface ITextNode : IGeometry<VertexPositionTexture>
    {
        string Text { get; }
    }
}