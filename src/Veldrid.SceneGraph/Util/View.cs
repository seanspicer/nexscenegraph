using System.Numerics;

namespace Veldrid.SceneGraph.Util
{
    public class SceneView
    {
        //
        // Public Properties
        // 
        public Node SceneData { get; set; }
        public Vector4 ClearColor { get; set; }

        private GraphicsDevice _graphicsDevice;
        public GraphicsDevice GraphicsDevice
        {
            get => _graphicsDevice;
            set
            {
                _graphicsDevice = value;
               
                // Cleanup old visitor (if necessary)
                _drawVisitor?.DisposeResources(); 
                
                // Construct new visitor
                _drawVisitor = new DrawVisitor(_graphicsDevice);
            }
        }
        
        //
        // Private
        //
        private DrawVisitor _drawVisitor;
        
        
        public SceneView()
        {
            SceneData = null;
            ClearColor = Vector4.Zero;
        }

        public virtual void Draw()
        {
            _drawVisitor.BeginDraw();
            SceneData?.Accept(_drawVisitor);
            _drawVisitor.EndDraw();
        }
    }
}