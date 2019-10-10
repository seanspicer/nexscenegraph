using System.Text;

namespace Veldrid.SceneGraph.Util.Shape
{
    public enum NormalsType
    {
        PerFace,
        PerVertex
    }
    
    public interface ITessellationHints
    {
        NormalsType NormalsType { get; set; }
    }
    
    public class TessellationHints : ITessellationHints
    {
        public NormalsType NormalsType { get; set; }

        public static ITessellationHints Create()
        {
            return new TessellationHints();
        }

        internal TessellationHints()
        {
            NormalsType = NormalsType.PerVertex;
        }
    }
}