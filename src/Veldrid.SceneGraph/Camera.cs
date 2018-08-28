



using System.Numerics;
using SharpDX.Mathematics.Interop;

namespace Veldrid.SceneGraph
{
    public class Camera : Transform
    {
        public View View { get; set; }
        
        public Matrix4x4 ProjectionMatrix { get; set; }
        public Matrix4x4 ViewMatrix { get; set; }

        public IGraphicsDeviceOperation Renderer { get; set; }
        
        public Camera()
        {
            ProjectionMatrix = Matrix4x4.Identity;
            ViewMatrix = Matrix4x4.Identity;
        }

        /// <summary>
        /// Create a symmetrical perspective projection. 
        /// </summary>
        /// <param name="vfov"></param>
        /// <param name="aspectRatio"></param>
        /// <param name="zNear"></param>
        /// <param name="zFar"></param>
        public void SetProjectionMatrixAsPerspective(float vfov, float aspectRatio, float zNear, float zFar)
        {
            //ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(vfov, aspectRatio, zNear, zFar);
        }
        
        
    }
}