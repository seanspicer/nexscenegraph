
using System.Numerics;

namespace Veldrid.SceneGraph.Util
{
    public static class QuaternionExtensions
    {
        /** Make a rotation Quat which will rotate vec1 to vec2
            This routine uses only fast geometric transforms, without costly acos/sin computations.
            It's exact, fast, and with less degenerate cases than the acos/sin method.
            
            For an explanation of the math used, you may see for example:
            
            http://logiciels.cnes.fr/MARMOTTES/marmottes-mathematique.pdf
            
            NOTE - This is the rotation with shortest angle, which is the one equivalent to the
            acos/sin transform method. Other rotations exists, for example to additionally keep
            a local horizontal attitude.
            
            Adapted from original work of Nicolas Brodu in OpenSceneGraph
        */
        public static Quaternion MakeRotate(Vector3 from, Vector3 to)
        {
            // This routine takes any vector as argument but normalized
            // vectors are necessary, if only for computing the dot product.
            // Too bad the API is that generic, it leads to performance loss.
            // Even in the case the 2 vectors are not normalized but same length,
            // the sqrt could be shared, but we have no way to know beforehand
            // at this point, while the caller may know.
            // So, we have to test... in the hope of saving at least a sqrt
            Vector3 sourceVector = from;
            Vector3 targetVector = to;

            var fromLen2 = from.LengthSquared();
            float fromLen;
            // normalize only when necessary, epsilon test
            if ((fromLen2 < 1.0-1e-7) || (fromLen2 > 1.0+1e-7)) {
                fromLen = (float)System.Math.Sqrt(fromLen2);
                sourceVector /= fromLen;
            } else fromLen = 1.0f;

            var toLen2 = to.LengthSquared();
            // normalize only when necessary, epsilon test
            if ((toLen2 < 1.0-1e-7) || (toLen2 > 1.0+1e-7)) {
                float toLen;
                // re-use fromLen for case of mapping 2 vectors of the same length
                if ((toLen2 > fromLen2-1e-7) && (toLen2 < fromLen2+1e-7)) {
                    toLen = fromLen;
                }
                else toLen = (float)System.Math.Sqrt(toLen2);
                targetVector /= toLen;
            }


            // Now let's get into the real stuff
            // Use "dot product plus one" as test as it can be re-used later on
            double dotProdPlus1 = 1.0 + Vector3.Dot(sourceVector,targetVector);

            var q = new double[4]; 
            
            // Check for degenerate case of full u-turn. Use epsilon for detection
            if (dotProdPlus1 < 1e-7) {

                // Get an orthogonal vector of the given vector
                // in a plane with maximum vector coordinates.
                // Then use it as quaternion axis with pi angle
                // Trick is to realize one value at least is >0.6 for a normalized vector.
                if (System.Math.Abs(sourceVector.X) < 0.6) {
                    var norm = System.Math.Sqrt(1.0 - sourceVector.X * sourceVector.X);
                    q[0] = 0.0;
                    q[1] = sourceVector.Z / norm;
                    q[2] = -sourceVector.Y / norm;
                    q[3] = 0.0;
                } else if (System.Math.Abs(sourceVector.Y) < 0.6) {
                    var norm = System.Math.Sqrt(1.0 - sourceVector.Y * sourceVector.Y);
                    q[0] = -sourceVector.Z / norm;
                    q[1] = 0.0;
                    q[2] = sourceVector.X / norm;
                    q[3] = 0.0;
                } else {
                    var norm = System.Math.Sqrt(1.0 - sourceVector.Z * sourceVector.Z);
                    q[0] = sourceVector.Y / norm;
                    q[1] = -sourceVector.X / norm;
                    q[2] = 0.0;
                    q[3] = 0.0;
                }
            }

            else {
                // Find the shortest angle quaternion that transforms normalized vectors
                // into one other. Formula is still valid when vectors are colinear
                var s = System.Math.Sqrt(0.5 * dotProdPlus1);
                Vector3 tmp = Vector3.Divide(Vector3.Cross(sourceVector, targetVector), (float)(2.0*s));
                q[0] = tmp.X;
                q[1] = tmp.Y;
                q[2] = tmp.Z;
                q[3] = s;
            }

            return new Quaternion((float) q[0], (float) q[1], (float) q[2], (float) q[3]);
        }
    }
}