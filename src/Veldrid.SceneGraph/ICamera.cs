using System.Numerics;

namespace Veldrid.SceneGraph
{
    public interface ICamera : ITransform
    {
        View View { get; set; }
        Matrix4x4 ProjectionMatrix { get; set; }
        Matrix4x4 ViewMatrix { get; set; }
        
        Vector3 Up { get; }
        Vector3 Look { get; }
        Vector3 Position { get; }
        
        float AspectRatio { get; }
        float Fov { get; }
        
        float Near { get; }
        float Far { get; }
        
        float Yaw { get; set; }
        float Pitch { get; set; }
        IGraphicsDeviceOperation Renderer { get; set; }

        void SetViewMatrixToLookAt(Vector3 position, Vector3 target, Vector3 upDirection);
        
        /// <summary>
        /// Create a symmetrical perspective projection. 
        /// </summary>
        /// <param name="vfov"></param>
        /// <param name="aspectRatio"></param>
        /// <param name="zNear"></param>
        /// <param name="zFar"></param>
        void SetProjectionMatrixAsPerspective(float vfov, float aspectRatio, float zNear, float zFar);

        Vector3 NormalizedScreenToWorld(Vector3 screenCoords);
    }
}