using System.Numerics;

namespace Veldrid.SceneGraph
{
    public interface IPlane
    {
        float Nx { get; }
        float Ny { get; }
        float Nz { get; }
        float D { get; }
          
        
        void Transform(Matrix4x4 matrix);
        float Distance(Vector3 v);

        /// <summary>
        /// Intersection test between plane and bounding sphere.
        /// </summary>
        /// <param name="bb"></param>
        /// <returns>
        /// return 1 if the bb is completely above plane,
        /// return 0 if the bb intersects the plane,
        /// return -1 if the bb is completely below the plane.
        /// </returns>
        int Intersect(IBoundingBox bb);
    }
}