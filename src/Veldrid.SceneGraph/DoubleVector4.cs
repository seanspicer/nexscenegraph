using System.Runtime.CompilerServices;

namespace Veldrid.SceneGraph
{
    public struct DoubleVector4
    {
        /// <summary>
        /// Value at row 1, column 1 of the matrix.
        /// </summary>
        public double X;

        /// <summary>
        /// Value at row 1, column 2 of the matrix.
        /// </summary>
        public double Y;

        /// <summary>
        /// Value at row 1, column 3 of the matrix.
        /// </summary>
        public double Z;

        /// <summary>
        /// Value at row 1, column 4 of the matrix.
        /// </summary>
        public double W;

        public unsafe double this[int index]
        {
            get
            {
                fixed (double* v = &this.X)
                {
                    return *(v + index);
                }
            } 
            set
            {
                fixed (double* v = &this.X)
                {
                    *(v + index) = value;
                }
            }
        }
    }
}