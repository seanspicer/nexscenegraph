using Veldrid.SceneGraph.Wpf.Element;

namespace Veldrid.SceneGraph.Wpf
{
    public abstract class BaseRenderer : IDirect3D
    {
        /// <summary>
        /// 
        /// </summary>
        private Element.D3D11 _context;

        /// <summary>
        /// 
        /// </summary>
        protected Element.D3D11 Renderer 
        {
            get { return _context; }
            set
            {
                if (Renderer != null)
                {
                    Renderer.Rendering -= ContextRendering;
                    Detach();
                }
                _context = value;
                if (Renderer != null)
                {
                    Renderer.Rendering += ContextRendering;
                    Attach();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public abstract void RenderCore(DrawEventArgs args);
        
        /// <summary>
        /// 
        /// </summary>
        protected abstract void Attach();

        /// <summary>
        /// 
        /// </summary>
        protected abstract void Detach();

        protected abstract void ResetCore(DrawEventArgs args);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        void IDirect3D.Reset(DrawEventArgs args)
        {
            if (Renderer != null)
            {
                ResetCore(args);
                Renderer.Reset(args);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        void IDirect3D.Render(DrawEventArgs args)
        {
            if (Renderer != null)
                Renderer.Render(args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aCtx"></param>
        /// <param name="args"></param>
        private void ContextRendering(object aCtx, DrawEventArgs args) { RenderCore(args); }

    }
}