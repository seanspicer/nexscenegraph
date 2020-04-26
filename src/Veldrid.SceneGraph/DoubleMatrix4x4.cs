using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid.SceneGraph
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DoubleMatrix4x4
    {
        /// <summary>
        /// Value at row 1, column 1 of the matrix.
        /// </summary>
        public double M11;

        /// <summary>
        /// Value at row 1, column 2 of the matrix.
        /// </summary>
        public double M12;

        /// <summary>
        /// Value at row 1, column 3 of the matrix.
        /// </summary>
        public double M13;

        /// <summary>
        /// Value at row 1, column 4 of the matrix.
        /// </summary>
        public double M14;

        /// <summary>
        /// Value at row 2, column 1 of the matrix.
        /// </summary>
        public double M21;

        /// <summary>
        /// Value at row 2, column 2 of the matrix.
        /// </summary>
        public double M22;

        /// <summary>
        /// Value at row 2, column 3 of the matrix.
        /// </summary>
        public double M23;

        /// <summary>
        /// Value at row 2, column 4 of the matrix.
        /// </summary>
        public double M24;

        /// <summary>
        /// Value at row 3, column 1 of the matrix.
        /// </summary>
        public double M31;

        /// <summary>
        /// Value at row 3, column 2 of the matrix.
        /// </summary>
        public double M32;

        /// <summary>
        /// Value at row 3, column 3 of the matrix.
        /// </summary>
        public double M33;

        /// <summary>
        /// Value at row 3, column 4 of the matrix.
        /// </summary>
        public double M34;

        /// <summary>
        /// Value at row 4, column 1 of the matrix.
        /// </summary>
        public double M41;

        /// <summary>
        /// Value at row 4, column 2 of the matrix.
        /// </summary>
        public double M42;

        /// <summary>
        /// Value at row 4, column 3 of the matrix.
        /// </summary>
        public double M43;

        /// <summary>
        /// Value at row 4, column 4 of the matrix.
        /// </summary>
        public double M44;

        /// <summary>
        /// Constructs a Matrix4x4 from the given components.
        /// </summary>
        public DoubleMatrix4x4(double m11, double m12, double m13, double m14,
            double m21, double m22, double m23, double m24,
            double m31, double m32, double m33, double m34,
            double m41, double m42, double m43, double m44)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M13 = m13;
            this.M14 = m14;

            this.M21 = m21;
            this.M22 = m22;
            this.M23 = m23;
            this.M24 = m24;

            this.M31 = m31;
            this.M32 = m32;
            this.M33 = m33;
            this.M34 = m34;

            this.M41 = m41;
            this.M42 = m42;
            this.M43 = m43;
            this.M44 = m44;
        }
        
        public DoubleMatrix4x4(Matrix4x4 matrix)
        {
            M11 = matrix.M11;
            M12 = matrix.M12;
            M13 = matrix.M13;
            M14 = matrix.M14;

            M21 = matrix.M21;
            M22 = matrix.M22;
            M23 = matrix.M23;
            M24 = matrix.M24;

            M31 = matrix.M31;
            M32 = matrix.M32;
            M33 = matrix.M33;
            M34 = matrix.M34;

            M41 = matrix.M41;
            M42 = matrix.M42;
            M43 = matrix.M43;
            M44 = matrix.M44;
        }
        
        public Matrix4x4 ToMatrix4x4()
        {
            Matrix4x4 matrix;
            matrix.M11 = Convert.ToSingle(M11);
            matrix.M12 = Convert.ToSingle(M12);
            matrix.M13 = Convert.ToSingle(M13);
            matrix.M14 = Convert.ToSingle(M14);

            matrix.M21 = Convert.ToSingle(M21);
            matrix.M22 = Convert.ToSingle(M22);
            matrix.M23 = Convert.ToSingle(M23);
            matrix.M24 = Convert.ToSingle(M24);

            matrix.M31 = Convert.ToSingle(M31);
            matrix.M32 = Convert.ToSingle(M32);
            matrix.M33 = Convert.ToSingle(M33);
            matrix.M34 = Convert.ToSingle(M33);

            matrix.M41 = Convert.ToSingle(M41);
            matrix.M42 = Convert.ToSingle(M42);
            matrix.M43 = Convert.ToSingle(M43);
            matrix.M44 = Convert.ToSingle(M44);
            return matrix;
        }
        
        private static readonly DoubleMatrix4x4 _identity = new DoubleMatrix4x4
        (
            1d, 0d, 0d, 0d,
            0d, 1d, 0d, 0d,
            0d, 0d, 1d, 0d,
            0d, 0d, 0d, 1d
        );
        
        /// <summary>
        /// Returns the multiplicative identity matrix.
        /// </summary>
        public static DoubleMatrix4x4 Identity
        {
            get { return _identity; }
        }

        /// <summary>
        /// Returns whether the matrix is the identity matrix.
        /// </summary>
        public readonly bool IsIdentity
        {
            get
            {
                return M11 == 1d && M22 == 1d && M33 == 1d && M44 == 1d && // Check diagonal element first for early out.
                       M12 == 0d && M13 == 0d && M14 == 0d &&
                       M21 == 0d && M23 == 0d && M24 == 0d &&
                       M31 == 0d && M32 == 0d && M34 == 0d &&
                       M41 == 0d && M42 == 0d && M43 == 0d;
            }
        }
        
        
        public unsafe double this[int index1, int index2]
        {
            get
            {
                fixed (double* m = &this.M11)
                {
                    return *(m + index1 * 4 + index2);
                }
            }

            set
            {
                fixed (double* m = &this.M11)
                {
                    *(m + index1 * 4 + index2) = value;
                }
            }
        }
    }
}