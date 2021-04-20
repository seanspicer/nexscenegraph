using System.Runtime.InteropServices;

namespace Veldrid.SceneGraph
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4d
    {
        /// <summary>
        ///     Value at row 1, column 1 of the matrix.
        /// </summary>
        public double X;

        /// <summary>
        ///     Value at row 1, column 2 of the matrix.
        /// </summary>
        public double Y;

        /// <summary>
        ///     Value at row 1, column 3 of the matrix.
        /// </summary>
        public double Z;

        /// <summary>
        ///     Value at row 1, column 4 of the matrix.
        /// </summary>
        public double W;

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
    }
}