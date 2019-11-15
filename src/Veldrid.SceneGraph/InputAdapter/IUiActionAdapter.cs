namespace Veldrid.SceneGraph.InputAdapter
{
    public interface IUiActionAdapter
    {
        void RequestRedraw();

        void RequestContinuousRedraw();
    }
}