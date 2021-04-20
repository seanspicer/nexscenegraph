using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid.SceneGraph
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3d
    {
        public double X;

        public double Y;

        public double Z;

        public static Vector3d Zero => new Vector3d();

        public Vector3d(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public unsafe double this[int index]
        {
            get
            {
                fixed (double* v = &X)
                {
                    return *(v + index);
                }
            }
            set
            {
                fixed (double* v = &X)
                {
                    *(v + index) = value;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Length()
        {
            var ls = X * X + Y * Y + Z * Z;
            return System.Math.Sqrt(ls);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Normalize(Vector3d value)
        {
            var ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z;
            var length = System.Math.Sqrt(ls);
            return new Vector3d(value.X / length, value.Y / length, value.Z / length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Cross(Vector3d vector1, Vector3d vector2)
        {
            return new Vector3d(
                vector1.Y * vector2.Z - vector1.Z * vector2.Y,
                vector1.Z * vector2.X - vector1.X * vector2.Z,
                vector1.X * vector2.Y - vector1.Y * vector2.X);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator -(Vector3d left, Vector3d right)
        {
            return new Vector3d(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator -(Vector3d value)
        {
            return Zero - value;
        }
    }
}