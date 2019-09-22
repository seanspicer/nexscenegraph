using Veldrid.SceneGraph.Wpf.Element;

namespace Veldrid.SceneGraph.Wpf
{
    public class VeldridSceneGraphElement : DXElement
    {
        public VeldridSceneGraphElement()
        {
            Renderer = new VeldridSceneGraphRenderer();
        }
    }
}