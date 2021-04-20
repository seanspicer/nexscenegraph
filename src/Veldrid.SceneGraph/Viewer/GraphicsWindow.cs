using System.Collections.Generic;
using Veldrid.SceneGraph.InputAdapter;

namespace Veldrid.SceneGraph.Viewer
{
    public interface IGraphicsWindow : IGraphicsContext, IUiActionAdapter
    {
        //IEventQueue EventQueue { get; set; }
    }

    public abstract class GraphicsWindow : GraphicsContext, IGraphicsWindow
    {
        public virtual void RequestRedraw()
        {
            var views = GetViews();
            foreach (var view in views) view.RequestRedraw();
        }

        public virtual void RequestContinuousUpdate(bool flag)
        {
        }

        public virtual void RequestWarpPointer(float x, float y)
        {
        }

        protected ViewSet GetViews()
        {
            var views = new ViewSet();
            foreach (var camera in Cameras)
                if (camera.View is IView view)
                    views.Add(view);

            return views;
        }
        //public IEventQueue EventQueue { get; set; }

        public class ViewSet : HashSet<IView>
        {
        }
    }
}