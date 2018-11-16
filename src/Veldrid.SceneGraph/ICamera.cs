using System.Numerics;

namespace Veldrid.SceneGraph
{
    public interface ICamera : ITransform
    {
        View View { get; set; }
        Matrix4x4 ProjectionMatrix { get; set; }
        Matrix4x4 ViewMatrix { get; set; }
        float Yaw { get; set; }
        float Pitch { get; set; }
        IGraphicsDeviceOperation Renderer { get; set; }

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