using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security;

namespace Veldrid.SceneGraph
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix4x4d
    {
        /// <summary>
        ///     Value at row 1, column 1 of the matrix.
        /// </summary>
        public double M11;

        /// <summary>
        ///     Value at row 1, column 2 of the matrix.
        /// </summary>
        public double M12;

        /// <summary>
        ///     Value at row 1, column 3 of the matrix.
        /// </summary>
        public double M13;

        /// <summary>
        ///     Value at row 1, column 4 of the matrix.
        /// </summary>
        public double M14;

        /// <summary>
        ///     Value at row 2, column 1 of the matrix.
        /// </summary>
        public double M21;

        /// <summary>
        ///     Value at row 2, column 2 of the matrix.
        /// </summary>
        public double M22;

        /// <summary>
        ///     Value at row 2, column 3 of the matrix.
        /// </summary>
        public double M23;

        /// <summary>
        ///     Value at row 2, column 4 of the matrix.
        /// </summary>
        public double M24;

        /// <summary>
        ///     Value at row 3, column 1 of the matrix.
        /// </summary>
        public double M31;

        /// <summary>
        ///     Value at row 3, column 2 of the matrix.
        /// </summary>
        public double M32;

        /// <summary>
        ///     Value at row 3, column 3 of the matrix.
        /// </summary>
        public double M33;

        /// <summary>
        ///     Value at row 3, column 4 of the matrix.
        /// </summary>
        public double M34;

        /// <summary>
        ///     Value at row 4, column 1 of the matrix.
        /// </summary>
        public double M41;

        /// <summary>
        ///     Value at row 4, column 2 of the matrix.
        /// </summary>
        public double M42;

        /// <summary>
        ///     Value at row 4, column 3 of the matrix.
        /// </summary>
        public double M43;

        /// <summary>
        ///     Value at row 4, column 4 of the matrix.
        /// </summary>
        public double M44;

        /// <summary>
        ///     Constructs a Matrix4x4 from the given components.
        /// </summary>
        public Matrix4x4d(double m11, double m12, double m13, double m14,
            double m21, double m22, double m23, double m24,
            double m31, double m32, double m33, double m34,
            double m41, double m42, double m43, double m44)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M14 = m14;

            M21 = m21;
            M22 = m22;
            M23 = m23;
            M24 = m24;

            M31 = m31;
            M32 = m32;
            M33 = m33;
            M34 = m34;

            M41 = m41;
            M42 = m42;
            M43 = m43;
            M44 = m44;
        }

        public Matrix4x4d(Matrix4x4 matrix)
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

        /// <summary>
        ///     Returns the multiplicative identity matrix.
        /// </summary>
        public static Matrix4x4d Identity { get; } = new Matrix4x4d
        (
            1d, 0d, 0d, 0d,
            0d, 1d, 0d, 0d,
            0d, 0d, 1d, 0d,
            0d, 0d, 0d, 1d
        );

        /// <summary>
        ///     Returns whether the matrix is the identity matrix.
        /// </summary>
        public readonly bool IsIdentity =>
            M11 == 1d && M22 == 1d && M33 == 1d && M44 == 1d && // Check diagonal element first for early out.
            M12 == 0d && M13 == 0d && M14 == 0d &&
            M21 == 0d && M23 == 0d && M24 == 0d &&
            M31 == 0d && M32 == 0d && M34 == 0d &&
            M41 == 0d && M42 == 0d && M43 == 0d;


        public unsafe double this[int index1, int index2]
        {
            get
            {
                fixed (double* m = &M11)
                {
                    return *(m + index1 * 4 + index2);
                }
            }

            set
            {
                fixed (double* m = &M11)
                {
                    *(m + index1 * 4 + index2) = value;
                }
            }
        }

        public double GetDeterminant()
        {
            double a = M11, b = M12, c = M13, d = M14;
            double e = M21, f = M22, g = M23, h = M24;
            double i = M31, j = M32, k = M33, l = M34;
            double m = M41, n = M42, o = M43, p = M44;

            var kp_lo = k * p - l * o;
            var jp_ln = j * p - l * n;
            var jo_kn = j * o - k * n;
            var ip_lm = i * p - l * m;
            var io_km = i * o - k * m;
            var in_jm = i * n - j * m;

            return a * (f * kp_lo - g * jp_ln + h * jo_kn) -
                   b * (e * kp_lo - g * ip_lm + h * io_km) +
                   c * (e * jp_ln - f * ip_lm + h * in_jm) -
                   d * (e * jo_kn - f * io_km + g * in_jm);
        }

        public static bool Invert(Matrix4x4d matrix, out Matrix4x4d result)
        {
            double a = matrix.M11, b = matrix.M12, c = matrix.M13, d = matrix.M14;
            double e = matrix.M21, f = matrix.M22, g = matrix.M23, h = matrix.M24;
            double i = matrix.M31, j = matrix.M32, k = matrix.M33, l = matrix.M34;
            double m = matrix.M41, n = matrix.M42, o = matrix.M43, p = matrix.M44;

            var kp_lo = k * p - l * o;
            var jp_ln = j * p - l * n;
            var jo_kn = j * o - k * n;
            var ip_lm = i * p - l * m;
            var io_km = i * o - k * m;
            var in_jm = i * n - j * m;

            var a11 = +(f * kp_lo - g * jp_ln + h * jo_kn);
            var a12 = -(e * kp_lo - g * ip_lm + h * io_km);
            var a13 = +(e * jp_ln - f * ip_lm + h * in_jm);
            var a14 = -(e * jo_kn - f * io_km + g * in_jm);

            var det = a * a11 + b * a12 + c * a13 + d * a14;

            if (System.Math.Abs(det) < double.Epsilon)
            {
                result = new Matrix4x4d(double.NaN, double.NaN, double.NaN, double.NaN,
                    double.NaN, double.NaN, double.NaN, double.NaN,
                    double.NaN, double.NaN, double.NaN, double.NaN,
                    double.NaN, double.NaN, double.NaN, double.NaN);
                return false;
            }

            var invDet = 1.0f / det;

            result.M11 = a11 * invDet;
            result.M21 = a12 * invDet;
            result.M31 = a13 * invDet;
            result.M41 = a14 * invDet;

            result.M12 = -(b * kp_lo - c * jp_ln + d * jo_kn) * invDet;
            result.M22 = +(a * kp_lo - c * ip_lm + d * io_km) * invDet;
            result.M32 = -(a * jp_ln - b * ip_lm + d * in_jm) * invDet;
            result.M42 = +(a * jo_kn - b * io_km + c * in_jm) * invDet;

            var gp_ho = g * p - h * o;
            var fp_hn = f * p - h * n;
            var fo_gn = f * o - g * n;
            var ep_hm = e * p - h * m;
            var eo_gm = e * o - g * m;
            var en_fm = e * n - f * m;

            result.M13 = +(b * gp_ho - c * fp_hn + d * fo_gn) * invDet;
            result.M23 = -(a * gp_ho - c * ep_hm + d * eo_gm) * invDet;
            result.M33 = +(a * fp_hn - b * ep_hm + d * en_fm) * invDet;
            result.M43 = -(a * fo_gn - b * eo_gm + c * en_fm) * invDet;

            var gl_hk = g * l - h * k;
            var fl_hj = f * l - h * j;
            var fk_gj = f * k - g * j;
            var el_hi = e * l - h * i;
            var ek_gi = e * k - g * i;
            var ej_fi = e * j - f * i;

            result.M14 = -(b * gl_hk - c * fl_hj + d * fk_gj) * invDet;
            result.M24 = +(a * gl_hk - c * el_hi + d * ek_gi) * invDet;
            result.M34 = -(a * fl_hj - b * el_hi + d * ej_fi) * invDet;
            result.M44 = +(a * fk_gj - b * ek_gi + c * ej_fi) * invDet;

            return true;
        }

        private struct CanonicalBasis
        {
            public Vector3d Row0;
            public Vector3d Row1;
            public Vector3d Row2;
        }

        [SecuritySafeCritical]
        private struct VectorBasis
        {
            public unsafe Vector3d* Element0;
            public unsafe Vector3d* Element1;
            public unsafe Vector3d* Element2;
        }

        [SecuritySafeCritical]
        public static bool Decompose(Matrix4x4d matrix, out Vector3d scale, out Quaternion rotation,
            out Vector3d translation)
        {
            var result = true;

            unsafe
            {
                fixed (Vector3d* scaleBase = &scale)
                {
                    var pfScales = (double*) scaleBase;
                    const double EPSILON = 0.0001d;
                    double det;

                    VectorBasis vectorBasis;
                    var pVectorBasis = (Vector3d**) &vectorBasis;

                    var matTemp = Identity;
                    var canonicalBasis = new CanonicalBasis();
                    var pCanonicalBasis = &canonicalBasis.Row0;

                    canonicalBasis.Row0 = new Vector3d(1.0d, 0.0d, 0.0d);
                    canonicalBasis.Row1 = new Vector3d(0.0f, 1.0f, 0.0f);
                    canonicalBasis.Row2 = new Vector3d(0.0f, 0.0f, 1.0f);

                    translation = new Vector3d(
                        matrix.M41,
                        matrix.M42,
                        matrix.M43);

                    pVectorBasis[0] = (Vector3d*) &matTemp.M11;
                    pVectorBasis[1] = (Vector3d*) &matTemp.M21;
                    pVectorBasis[2] = (Vector3d*) &matTemp.M31;

                    *pVectorBasis[0] = new Vector3d(matrix.M11, matrix.M12, matrix.M13);
                    *pVectorBasis[1] = new Vector3d(matrix.M21, matrix.M22, matrix.M23);
                    *pVectorBasis[2] = new Vector3d(matrix.M31, matrix.M32, matrix.M33);

                    scale.X = pVectorBasis[0]->Length();
                    scale.Y = pVectorBasis[1]->Length();
                    scale.Z = pVectorBasis[2]->Length();

                    uint a, b, c;

                    #region Ranking

                    double x = pfScales[0], y = pfScales[1], z = pfScales[2];
                    if (x < y)
                    {
                        if (y < z)
                        {
                            a = 2;
                            b = 1;
                            c = 0;
                        }
                        else
                        {
                            a = 1;

                            if (x < z)
                            {
                                b = 2;
                                c = 0;
                            }
                            else
                            {
                                b = 0;
                                c = 2;
                            }
                        }
                    }
                    else
                    {
                        if (x < z)
                        {
                            a = 2;
                            b = 0;
                            c = 1;
                        }
                        else
                        {
                            a = 0;

                            if (y < z)
                            {
                                b = 2;
                                c = 1;
                            }
                            else
                            {
                                b = 1;
                                c = 2;
                            }
                        }
                    }

                    #endregion

                    if (pfScales[a] < EPSILON) *pVectorBasis[a] = pCanonicalBasis[a];

                    *pVectorBasis[a] = Vector3d.Normalize(*pVectorBasis[a]);

                    if (pfScales[b] < EPSILON)
                    {
                        uint cc;
                        double fAbsX, fAbsY, fAbsZ;

                        fAbsX = System.Math.Abs(pVectorBasis[a]->X);
                        fAbsY = System.Math.Abs(pVectorBasis[a]->Y);
                        fAbsZ = System.Math.Abs(pVectorBasis[a]->Z);

                        #region Ranking

                        if (fAbsX < fAbsY)
                        {
                            if (fAbsY < fAbsZ)
                            {
                                cc = 0;
                            }
                            else
                            {
                                if (fAbsX < fAbsZ)
                                    cc = 0;
                                else
                                    cc = 2;
                            }
                        }
                        else
                        {
                            if (fAbsX < fAbsZ)
                            {
                                cc = 1;
                            }
                            else
                            {
                                if (fAbsY < fAbsZ)
                                    cc = 1;
                                else
                                    cc = 2;
                            }
                        }

                        #endregion

                        *pVectorBasis[b] = Vector3d.Cross(*pVectorBasis[a], *(pCanonicalBasis + cc));
                    }

                    *pVectorBasis[b] = Vector3d.Normalize(*pVectorBasis[b]);

                    if (pfScales[c] < EPSILON) *pVectorBasis[c] = Vector3d.Cross(*pVectorBasis[a], *pVectorBasis[b]);

                    *pVectorBasis[c] = Vector3d.Normalize(*pVectorBasis[c]);

                    det = matTemp.GetDeterminant();

                    // use Kramer's rule to check for handedness of coordinate system
                    if (det < 0.0f)
                    {
                        // switch coordinate system by negating the scale and inverting the basis vector on the x-axis
                        pfScales[a] = -pfScales[a];
                        *pVectorBasis[a] = -(*pVectorBasis[a]);

                        det = -det;
                    }

                    det -= 1.0f;
                    det *= det;

                    if (EPSILON < det)
                    {
                        // Non-SRT matrix encountered
                        rotation = Quaternion.Identity;
                        result = false;
                    }
                    else
                    {
                        // generate the quaternion from the matrix
                        rotation = Quaternion.CreateFromRotationMatrix(matTemp.ToMatrix4x4());
                    }
                }
            }

            return result;
        }
    }
}