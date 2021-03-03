using System.Collections.Generic;

namespace Veldrid.SceneGraph.Viewer
{
    public abstract class ViewerBase : Object
    {
        protected abstract ContextList GetContexts(bool onlyValid);

        protected virtual WindowList GetWindows(bool onlyValid)
        {
            var windowList = new WindowList();
            var contextList = GetContexts(onlyValid);
            foreach (var context in contextList)
                if (context is IGraphicsWindow window)
                    windowList.Add(window);

            return windowList;
        }

        public class ContextList : List<IGraphicsContext>
        {
        }

        public class WindowList : List<IGraphicsWindow>
        {
        }
    }
}