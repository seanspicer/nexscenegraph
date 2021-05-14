using System;
using System.Numerics;

namespace Veldrid.SceneGraph
{
    internal struct Quat
    {
        public float x;
        public float y;
        public float z;
        public float w;
        
    }; /* Quaternion */

    internal enum QuatPart
    {
        X=0, 
        Y=1, 
        Z=2,
        W=3
    };
    
    internal struct HVect
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }; /* Homogeneous 3D vector */
    
    internal struct AffineParts
    {
        HVect t;	/* Translation components */
        Quat  q;	/* Essential rotation	  */
        Quat  u;	/* Stretch rotation	  */
        HVect k;	/* Stretch factors	  */
        float f;	/* Sign of determinant	  */
    };

    public class MatrixDecomposition
    {
        public float[][] MakeHMatrix()
        {
            var mat = new float[4][];
            mat[0] = new float[] {0,0,0,0};
            mat[1] = new float[] {0,0,0,0};
            mat[2] = new float[] {0,0,0,0};
            mat[3] = new float[] {0,0,0,0};

            return mat;
        }
        
        /** Fill out 3x3 matrix to 4x4 **/
        private void mat_pad(float[][] A)
        {
            A[(int) QuatPart.W][(int) QuatPart.X] = A[(int) QuatPart.X][(int) QuatPart.W] =
                A[(int) QuatPart.W][(int) QuatPart.Y] = A[(int) QuatPart.Y][(int) QuatPart.W] =
                    A[(int) QuatPart.W][(int) QuatPart.Z] = A[(int) QuatPart.Z][(int) QuatPart.W] = 0;
            
            A[(int)QuatPart.W][(int)QuatPart.W]=1;
        }
        
        /** Copy nxn matrix A to C **/
        private void mat_copy(float[][]C, float[][]A, int n) 
        {
            int i,j; 
            for(i=0;i<n;i++) 
                for(j=0;j<n;j++)
                    C[i][j] = A[i][j];
            
        }
        private void mat_copy_minus_eq(float[][]C, float[][]A, int n) 
        {
            int i,j; 
            for(i=0;i<n;i++) 
            for(j=0;j<n;j++)
                C[i][j] -= A[i][j];
            
        }
        
        /** Copy transpose of nxn matrix A to C **/
        private void mat_tpose(float[][]AT,float[][]A,int n) 
        {
            int i,j; 
            for(i=0;i<n;i++) 
                for(j=0;j<n;j++)
                    AT[i][j] = A[j][i];
        }
        
        /** Assign nxn matrix C the element-wise combination of A and B using "op" **/
        private void mat_binop_add(float[][]C, float a, float[][]A, float b, float[][]B, int n)
        {
            int i,j; 
            for(i=0;i<n;i++) 
                for(j=0;j<n;j++)
                    C[i][j] = (a*A[i][j]) + (b*B[i][j]);
            
        }
        
        /** Multiply the upper left 3x3 parts of A and B to get AB **/
        private void mat_mult(float[][] A, float[][] B, float[][] AB)
        {
            int i, j;
            for (i=0; i<3; i++) for (j=0; j<3; j++)
                AB[i][j] = A[i][0]*B[0][j] + A[i][1]*B[1][j] + A[i][2]*B[2][j];
        }

        /** Return dot product of length 3 vectors va and vb **/
        private float vdot(float [] va, float [] vb)
        {
            return (va[0]*vb[0] + va[1]*vb[1] + va[2]*vb[2]);
        }

        /** Set v to cross product of length 3 vectors va and vb **/
        private void vcross(float [] va, float [] vb, float [] v)
        {
            v[0] = va[1]*vb[2] - va[2]*vb[1];
            v[1] = va[2]*vb[0] - va[0]*vb[2];
            v[2] = va[0]*vb[1] - va[1]*vb[0];
        }

        /** Set MadjT to transpose of inverse of M times determinant of M **/
        private void adjoint_transpose(float[][]M, float[][] MadjT)
        {
            vcross(M[1], M[2], MadjT[0]);
            vcross(M[2], M[0], MadjT[1]);
            vcross(M[0], M[1], MadjT[2]);
        }
        
        /******* Quaternion Preliminaries *******/

        /* Construct a (possibly non-unit) quaternion from real components. */
        private Quat Qt_(float x, float y, float z, float w)
        {
            Quat qq;
            qq.x = x; qq.y = y; qq.z = z; qq.w = w;
            return (qq);
        }

        /* Return conjugate of quaternion. */
        private Quat Qt_Conj(Quat q)
        {
            Quat qq;
            qq.x = -q.x; qq.y = -q.y; qq.z = -q.z; qq.w = q.w;
            return (qq);
        }

        /* Return quaternion product qL * qR.  Note: order is important!
         * To combine rotations, use the product Mul(qSecond, qFirst),
         * which gives the effect of rotating by qFirst then qSecond. */
        private Quat Qt_Mul(Quat qL, Quat qR)
        {
            Quat qq;
            qq.w = qL.w*qR.w - qL.x*qR.x - qL.y*qR.y - qL.z*qR.z;
            qq.x = qL.w*qR.x + qL.x*qR.w + qL.y*qR.z - qL.z*qR.y;
            qq.y = qL.w*qR.y + qL.y*qR.w + qL.z*qR.x - qL.x*qR.z;
            qq.z = qL.w*qR.z + qL.z*qR.w + qL.x*qR.y - qL.y*qR.x;
            return (qq);
        }

        /* Return product of quaternion q by scalar w. */
        private Quat Qt_Scale(Quat q, float w)
        {
            Quat qq;
            qq.w = q.w*w; qq.x = q.x*w; qq.y = q.y*w; qq.z = q.z*w;
            return (qq);
        }
        
        /* Construct a unit quaternion from rotation matrix.  Assumes matrix is
         * used to multiply column vector on the left: vnew = mat vold.	 Works
         * correctly for right-handed coordinate system and right-handed rotations.
         * Translation and perspective components ignored. */
        Quat Qt_FromMatrix(float[][] mat)
        {
            /* This algorithm avoids near-zero divides by looking for a large component
             * - first w, then x, y, or z.  When the trace is greater than zero,
             * |w| is greater than 1/2, which is as small as a largest component can be.
             * Otherwise, the largest diagonal entry corresponds to the largest of |x|,
             * |y|, or |z|, one of which must be larger than |w|, and at least 1/2. */
            Quat qu;
            qu.x = 0;
            qu.y = 0;
            qu.z = 0;
            qu.w = 0;
            
            float tr, s;

            tr = mat[(int)QuatPart.X][(int)QuatPart.X] + mat[(int)QuatPart.Y][(int)QuatPart.Y]+ mat[(int)QuatPart.Z][(int)QuatPart.Z];
            if (tr >= 0) 
            {
                s = (float)System.Math.Sqrt(tr + mat[(int)QuatPart.W][(int)QuatPart.W]);
                qu.w = s*0.5f;
                s = 0.5f / s;
                qu.x = (mat[(int)QuatPart.Z][(int)QuatPart.Y] - mat[(int)QuatPart.Y][(int)QuatPart.Z]) * s;
                qu.y = (mat[(int)QuatPart.X][(int)QuatPart.Z] - mat[(int)QuatPart.Z][(int)QuatPart.X]) * s;
                qu.z = (mat[(int)QuatPart.Y][(int)QuatPart.X] - mat[(int)QuatPart.X][(int)QuatPart.Y]) * s;
            } 
            else 
            {
                int h = (int)QuatPart.X;
                if (mat[(int) QuatPart.Y][(int) QuatPart.Y] > mat[(int) QuatPart.X][(int) QuatPart.X])
                {
                    h = (int)QuatPart.Y;
                }

                if (mat[(int) QuatPart.Z][(int) QuatPart.Z] > mat[h][h])
                {
                    h = (int)QuatPart.Z;
                }
                
                switch (h) 
                {
                    case (int)QuatPart.X:
                        s = (float)System.Math.Sqrt( (mat[(int)QuatPart.X][(int)QuatPart.X] - (mat[(int)QuatPart.Y][(int)QuatPart.Y]+mat[(int)QuatPart.Z][(int)QuatPart.Z])) + mat[(int)QuatPart.W][(int)QuatPart.W] );
                        qu.x = s*0.5f;
                        s = 0.5f / s;
                        qu.y = (mat[(int)QuatPart.X][(int)QuatPart.Y] + mat[(int)QuatPart.Y][(int)QuatPart.X]) * s;
                        qu.z = (mat[(int)QuatPart.Z][(int)QuatPart.X] + mat[(int)QuatPart.X][(int)QuatPart.Z]) * s;
                        qu.w = (mat[(int)QuatPart.Z][(int)QuatPart.Y] - mat[(int)QuatPart.Y][(int)QuatPart.Z]) * s;
                        break;
                    case (int)QuatPart.Y:
                        s = (float)System.Math.Sqrt( (mat[(int)QuatPart.Y][(int)QuatPart.Y] - (mat[(int)QuatPart.Z][(int)QuatPart.Z]+mat[(int)QuatPart.X][(int)QuatPart.X])) + mat[(int)QuatPart.W][(int)QuatPart.W] );
                        qu.y = s*0.5f;
                        s = 0.5f / s;
                        qu.z = (mat[(int)QuatPart.Y][(int)QuatPart.Z] + mat[(int)QuatPart.Z][(int)QuatPart.Y]) * s;
                        qu.x = (mat[(int)QuatPart.X][(int)QuatPart.Y] + mat[(int)QuatPart.Y][(int)QuatPart.X]) * s;
                        qu.w = (mat[(int)QuatPart.X][(int)QuatPart.Z] - mat[(int)QuatPart.Z][(int)QuatPart.X]) * s;
                        break;
                    case (int)QuatPart.Z:
                        s = (float)System.Math.Sqrt( (mat[(int)QuatPart.Z][(int)QuatPart.Z] - (mat[(int)QuatPart.X][(int)QuatPart.X]+mat[(int)QuatPart.Y][(int)QuatPart.Y])) + mat[(int)QuatPart.W][(int)QuatPart.W] );
                        qu.z = s*0.5f;
                        s = 0.5f / s;
                        qu.x = (mat[(int)QuatPart.Z][(int)QuatPart.X] + mat[(int)QuatPart.X][(int)QuatPart.Z]) * s;
                        qu.y = (mat[(int)QuatPart.Y][(int)QuatPart.Z] + mat[(int)QuatPart.Z][(int)QuatPart.Y]) * s;
                        qu.w = (mat[(int)QuatPart.Y][(int)QuatPart.X] - mat[(int)QuatPart.X][(int)QuatPart.Y]) * s;
                        break;
                }
            }

            if (System.Math.Abs(mat[(int) QuatPart.W][(int) QuatPart.W] - 1.0f) > 1e-6)
            {
                qu = Qt_Scale(qu, 1.0f/(float)System.Math.Sqrt(mat[(int)QuatPart.W][(int)QuatPart.W]));
            }
                
            return (qu);
        }
        
        static float[][] mat_id;

        /** Compute either the 1 or infinity norm of M, depending on tpose **/
        float mat_norm(float[][] M, int tpose)
        {
            int i;
            float sum, max;
            max = 0.0f;
            for (i=0; i<3; i++) {
                if (tpose > 0)
                {
                    sum = System.Math.Abs(M[0][i])+System.Math.Abs(M[1][i])+System.Math.Abs(M[2][i]);
                }
                else
                {
                    sum = System.Math.Abs(M[i][0])+System.Math.Abs(M[i][1])+System.Math.Abs(M[i][2]);
                }
                if (max<sum) max = sum;
            }
            return max;
        }
        
        float norm_inf(float[][] M) {return mat_norm(M, 0);}
        float norm_one(float[][] M) {return mat_norm(M, 1);}

        /** Return index of column of M containing maximum abs entry, or -1 if M=0 **/
        int find_max_col(float[][] M)
        {
            float abs, max;
            int i, j, col;
            max = 0.0f; 
            col = -1;
            for (i=0; i<3; i++) {
                for (j=0; j<3; j++) {
                    abs = M[i][j]; if (abs<0.0f) abs = -abs;
                    if (abs > max)
                    {
                        max = abs; col = j;
                    }
                }
            }
            return col;
        }
        
        /** Setup u for Household reflection to zero all v components but first **/
        void make_reflector(float []v, float []u)
        {
            float s = (float)System.Math.Sqrt(vdot(v, v));
            u[0] = v[0]; u[1] = v[1];
            u[2] = v[2] + ((v[2]<0.0f) ? -s : s);
            s = (float)System.Math.Sqrt(2.0f/vdot(u, u));
            u[0] = u[0]*s; u[1] = u[1]*s; u[2] = u[2]*s;
        }

        /** Apply Householder reflection represented by u to column vectors of M **/
        void reflect_cols(float[][] M, float []u)
        {
            int i, j;
            for (i=0; i<3; i++) 
            {
                float s = u[0]*M[0][i] + u[1]*M[1][i] + u[2]*M[2][i];
                for (j=0; j<3; j++) M[j][i] -= u[j]*s;
            }
        }
        
        /** Apply Householder reflection represented by u to row vectors of M **/
        void reflect_rows(float[][] M, float []u)
        {
            int i, j;
            for (i=0; i<3; i++) 
            {
                float s = vdot(u, M[i]);
                for (j=0; j<3; j++) M[i][j] -= u[j]*s;
            }
        }
        
        /** Find orthogonal factor Q of rank 1 (or less) M **/
        void do_rank1(float[][] M, float[][] Q)
        {
            float[] v1 = new float[3];
            float[] v2 = new float[3];
            float s;
            
            int col;
            /* If rank(M) is 1, we should find a non-zero column in M */
            col = find_max_col(M);
            if (col < 0)
            {
                mat_copy(Q,mat_id,4); return;
            } /* Rank is 0 */
            
            v1[0] = M[0][col]; v1[1] = M[1][col]; v1[2] = M[2][col];
            make_reflector(v1, v1); 
            reflect_cols(M, v1);
            v2[0] = M[2][0]; v2[1] = M[2][1]; v2[2] = M[2][2];
            make_reflector(v2, v2); 
            reflect_rows(M, v2);
            s = M[2][2];
            mat_copy(Q,mat_id,4);
            if (s<0.0f) Q[2][2] = -1.0f;
            reflect_cols(Q, v1); 
            reflect_rows(Q, v2);
        }

        /** Find orthogonal factor Q of rank 2 (or less) M using adjoint transpose **/
        void do_rank2(float [][] M, float [][] MadjT, float [][] Q)
        {
            float[] v1 = new float[3];
            float[] v2 = new float[3];
            float w, x, y, z, c, s, d;
            int col;
            /* If rank(M) is 2, we should find a non-zero column in MadjT */
            col = find_max_col(MadjT);
            if (col<0) {do_rank1(M, Q); return;} /* Rank<2 */
            v1[0] = MadjT[0][col]; v1[1] = MadjT[1][col]; v1[2] = MadjT[2][col];
            make_reflector(v1, v1); reflect_cols(M, v1);
            vcross(M[0], M[1], v2);
            make_reflector(v2, v2); reflect_rows(M, v2);
            w = M[0][0]; x = M[0][1]; y = M[1][0]; z = M[1][1];
            if (w*z>x*y) {
                c = z+w; s = y-x; d = (float)System.Math.Sqrt(c*c+s*s); c = c/d; s = s/d;
                Q[0][0] = Q[1][1] = c; Q[0][1] = -(Q[1][0] = s);
            } else {
                c = z-w; s = y+x; d = (float)System.Math.Sqrt(c*c+s*s); c = c/d; s = s/d;
                Q[0][0] = -(Q[1][1] = c); Q[0][1] = Q[1][0] = s;
            }
            Q[0][2] = Q[2][0] = Q[1][2] = Q[2][1] = 0.0f; Q[2][2] = 1.0f;
            reflect_cols(Q, v1); reflect_rows(Q, v2);
        }
        
        /******* Polar Decomposition *******/

        /* Polar Decomposition of 3x3 matrix in 4x4,
         * M = QS.  See Nicholas Higham and Robert S. Schreiber,
         * Fast Polar Decomposition of An Arbitrary Matrix,
         * Technical Report 88-942, October 1988,
         * Department of Computer Science, Cornell University.
         */
        float polar_decomp(float[][] M, float[][] Q, float[][] S)
        {
            const double TOL = 1.0e-6;
            float [][] Mk = MakeHMatrix(); 
            float [][] MadjTk = MakeHMatrix();
            float [][] Ek = MakeHMatrix();
            float det, M_one, M_inf, MadjT_one, MadjT_inf, E_one, gamma, g1, g2;
            int i, j;
            mat_tpose(Mk,M,3);
            M_one = norm_one(Mk);  
            M_inf = norm_inf(Mk);
            do {
                adjoint_transpose(Mk, MadjTk);
                
                det = vdot(Mk[0], MadjTk[0]);
                if (det==0.0f) {do_rank2(Mk, MadjTk, Mk); break;}
                MadjT_one = norm_one(MadjTk); MadjT_inf = norm_inf(MadjTk);
                gamma = (float)System.Math.Sqrt(System.Math.Sqrt((MadjT_one*MadjT_inf)/(M_one*M_inf))/System.Math.Abs(det));
                g1 = gamma*0.5f;
                g2 = 0.5f/(gamma*det);
                
                mat_copy(Ek,Mk,3);
                
                mat_binop_add(Mk,g1, Mk,g2, MadjTk,3);
                
                mat_copy_minus_eq(Ek, Mk,3);
                
                E_one = norm_one(Ek);
                M_one = norm_one(Mk);  
                M_inf = norm_inf(Mk);
                
            } while (E_one>(M_one*TOL));
            
            mat_tpose(Q,Mk,3); 
            mat_pad(Q);
            mat_mult(Mk, M, S);	 
            mat_pad(S);
            for (i=0; i<3; i++) for (j=i; j<3; j++)
                S[i][j] = S[j][i] = 0.5f*(S[i][j]+S[j][i]);
            return (det);
            return 1;
        }
        
        /******* Spectral Decomposition *******/

        /* Compute the spectral decomposition of symmetric positive semi-definite S.
         * Returns rotation in U and scale factors in result, so that if K is a diagonal
         * matrix of the scale factors, then S = U K (U transpose). Uses Jacobi method.
         * See Gene H. Golub and Charles F. Van Loan. Matrix Computations. Hopkins 1983.
         */
        HVect spect_decomp(float[][] S, float[][] U)
        {
            HVect kv;
            float[] Diag = new float[3];
            float[] OffD = new float[3]; /* OffD is off-diag (by omitted index) */
            float g,h,fabsh,fabsOffDi,t,theta,c,s,tau,ta,OffDq,a,b;
            
            char [] nxt = new char[3]
            {
                (char) QuatPart.Y, 
                (char) QuatPart.Z, 
                (char) QuatPart.X
            };
            
            int sweep, i, j;
            mat_copy(U,mat_id, 4);
            Diag[(int)QuatPart.X] = S[(int)QuatPart.X][(int)QuatPart.X]; 
            Diag[(int)QuatPart.Y] = S[(int)QuatPart.Y][(int)QuatPart.Y]; 
            Diag[(int)QuatPart.Z] = S[(int)QuatPart.Z][(int)QuatPart.Z];
            
            OffD[(int)QuatPart.X] = S[(int)QuatPart.Y][(int)QuatPart.Z]; 
            OffD[(int)QuatPart.Y] = S[(int)QuatPart.Z][(int)QuatPart.X]; 
            OffD[(int)QuatPart.Z] = S[(int)QuatPart.X][(int)QuatPart.Y];
            
            for (sweep=20; sweep>0; sweep--) {
                float sm = System.Math.Abs(OffD[(int)QuatPart.X])+System.Math.Abs(OffD[(int)QuatPart.Y])+System.Math.Abs(OffD[(int)QuatPart.Z]);
                if (sm==0.0) break;
                for (i=(int)QuatPart.Z; i>=(int)QuatPart.X; i--) {
                    int p = nxt[i]; int q = nxt[p];
                    fabsOffDi = System.Math.Abs(OffD[i]);
                    g = 100.0f*fabsOffDi;
                    if (fabsOffDi>0.0f) {
                        h = Diag[q] - Diag[p];
                        fabsh = System.Math.Abs(h);
                        if (System.Math.Abs(fabsh+g - fabsh) < 1e-6) {
                            t = OffD[i]/h;
                        } else {
                            theta = 0.5f*h/OffD[i];
                            t = 1.0f/(float)(System.Math.Abs(theta)+System.Math.Sqrt(theta*theta+1.0f));
                            if (theta<0.0f) t = -t;
                        }
                        c = 1.0f/(float) System.Math.Sqrt(t*t+1.0f); s = t*c;
                        tau = s/(c+1.0f);
                        ta = t*OffD[i]; OffD[i] = 0.0f;
                        Diag[p] -= ta; Diag[q] += ta;
                        OffDq = OffD[q];
                        OffD[q] -= s*(OffD[p] + tau*OffD[q]);
                        OffD[p] += s*(OffDq   - tau*OffD[p]);
                        for (j=(int)QuatPart.Z; j>=(int)QuatPart.X; j--) {
                            a = U[j][p]; b = U[j][q];
                            U[j][p] -= s*(b + tau*a);
                            U[j][q] += s*(a - tau*b);
                        }
                    }
                }
            }
            kv.x = Diag[(int)QuatPart.X]; kv.y = Diag[(int)QuatPart.Y]; kv.z = Diag[(int)QuatPart.Z]; kv.w = 1.0f;
            return (kv);
        }
        
        public MatrixDecomposition()
        {
            mat_id = new float[4][];
            mat_id[0] = new float[] {1,0,0,0};
            mat_id[1] = new float[] {0,1,0,0};
            mat_id[2] = new float[] {0,0,1,0};
            mat_id[3] = new float[] {0,0,0,1};

        }
    }
    
    /******* Matrix Preliminaries *******/



}