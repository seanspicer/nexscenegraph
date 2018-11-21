namespace Veldrid.SceneGraph
{
    public interface IGeometry<T> : IDrawable where T : struct, IPrimitiveElement
    {
        T[] VertexData { get; set; }
        int SizeOfVertexData { get; }
        ushort[] IndexData { get; set; }
    }
}