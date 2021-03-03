using System.Numerics;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Util.Shape;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IProjector
    {
        Matrix4x4 LocalToWorld { get; set; }
        Matrix4x4 WorldToLocal { get; }
        bool Project(IPointerInfo pi, out Vector3 projectedPoint);
    }

    public abstract class Projector : IProjector
    {
        private Matrix4x4 _localToWorld = Matrix4x4.Identity;

        private Matrix4x4 _worldToLocal = Matrix4x4.Identity;
        protected bool WorldToLocalDirty = true;

        public Matrix4x4 LocalToWorld
        {
            get => _localToWorld;
            set
            {
                _localToWorld = value;
                WorldToLocalDirty = true;
            }
        }

        public Matrix4x4 WorldToLocal
        {
            get
            {
                if (WorldToLocalDirty)
                    if (Matrix4x4.Invert(_localToWorld, out var worldToLocal))
                    {
                        _worldToLocal = worldToLocal;
                        WorldToLocalDirty = false;
                    }

                return _worldToLocal;
            }
        }


        public abstract bool Project(IPointerInfo pi, out Vector3 projectedPoint);

        protected static bool GetPlaneLineIntersection(Vector4 plane, Vector3 lineStart, Vector3 lineEnd,
            out Vector3 isect)
        {
            var deltaX = lineEnd.X - lineStart.X;
            var deltaY = lineEnd.Y - lineStart.Y;
            var deltaZ = lineEnd.Z - lineStart.Z;

            var denominator = plane.X * deltaX + plane.Y * deltaY + plane.Z * deltaZ;
            if (0 == denominator)
            {
                isect = Vector3.Zero;
                return false;
            }

            var c = (plane.X * lineStart.X + plane.Y * lineStart.Y + plane.Z * lineStart.Z + plane.W) / denominator;

            isect = new Vector3(
                lineStart.X - deltaX * c,
                lineStart.Y - deltaY * c,
                lineStart.Z - deltaZ * c
            );

            return true;
        }

        protected static IPlane ComputeIntersectionPlane(
            Vector3 eyeDir, Matrix4x4 localToWorld,
            Vector3 axisDir, ICylinder cylinder,
            bool front,
            ref Vector3 planeLineStart, ref Vector3 planeLineEnd,
            ref bool parallelPlane)
        {
            var unitAxisDir = Vector3.Normalize(axisDir);
            var perpDir = Vector3.Cross(unitAxisDir, GetLocalEyeDirection(eyeDir, localToWorld));

            // Check to make sure eye and cylinder axis are not too close
            if (perpDir.LengthSquared() < 1e-1)
            {
                // Too close, so instead return plane perpendicular to cylinder axis.
                parallelPlane = false;
                return Plane.Create(unitAxisDir, cylinder.Center);
            }

            // Otherwise compute plane along axisDir oriented towards eye
            var planeDir = Vector3.Normalize(Vector3.Cross(perpDir, axisDir));
            if (!front) planeDir = -planeDir;

            var planePoint = planeDir * cylinder.Radius + axisDir;
            planeLineStart = planePoint;
            planeLineEnd = planePoint + axisDir;
            parallelPlane = true;
            return Plane.Create(planeDir, planePoint);
        }

        protected static Vector3 GetLocalEyeDirection(Vector3 eyeDir, Matrix4x4 localToWorld)
        {
            return Vector3.Normalize(localToWorld.PostMultiply(eyeDir));
        }
    }
}