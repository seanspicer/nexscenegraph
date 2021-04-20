using System.Numerics;

namespace Veldrid.SceneGraph.Util
{
    public class VectorExtensions
    {
        public static Vector3 Transform3X3(Vector3 position, Matrix4x4 matrix)
        {
            return new Vector3(
                (float) (position.X * (double) matrix.M11 + position.Y * (double) matrix.M21 +
                         position.Z * (double) matrix.M31),
                (float) (position.X * (double) matrix.M12 + position.Y * (double) matrix.M22 +
                         position.Z * (double) matrix.M32),
                (float) (position.X * (double) matrix.M13 + position.Y * (double) matrix.M23 +
                         position.Z * (double) matrix.M33));
        }
    }
}