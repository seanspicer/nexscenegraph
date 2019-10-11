using System.Drawing;
using System.Text;

namespace Veldrid.SceneGraph.Util.Shape
{
    public enum NormalsType
    {
        PerFace,
        PerVertex
    }

    public enum ColorsType
    {
        ColorOverall,
        ColorPerFace,
        ColorPerVertex,
    }
    
    public interface ITessellationHints
    {
        NormalsType NormalsType { get; set; }
        ColorsType ColorsType { get; set; }
    }
    
    public class TessellationHints : ITessellationHints
    {
        public NormalsType NormalsType { get; set; }
        public ColorsType ColorsType { get; set; }

        public static ITessellationHints Create()
        {
            return new TessellationHints();
        }

        internal TessellationHints()
        {
            NormalsType = NormalsType.PerVertex;
            ColorsType = ColorsType.ColorOverall;
        }
    }
}