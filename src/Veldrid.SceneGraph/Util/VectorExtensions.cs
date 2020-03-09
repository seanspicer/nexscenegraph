using System.Numerics;

namespace Veldrid.SceneGraph.Util
{
    public class VectorExtensions
    {
        public static Vector3 Transform3X3(Vector3 position, Matrix4x4 matrix)
        {
            return new Vector3(
                (float) ((double) position.X * (double) matrix.M11 + (double) position.Y * (double) matrix.M21 + (double) position.Z * (double) matrix.M31), 
                (float) ((double) position.X * (double) matrix.M12 + (double) position.Y * (double) matrix.M22 + (double) position.Z * (double) matrix.M32), 
                (float) ((double) position.X * (double) matrix.M13 + (double) position.Y * (double) matrix.M23 + (double) position.Z * (double) matrix.M33));
        }
    }
}