
using System.Collections.Generic;
using System.Numerics;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IPointerInfo
    {
        void Reset();
    }
    
    public class PointerInfo : IPointerInfo
    {
        protected Vector3 NearPoint { get; set; }
        protected Vector3 FarPoint { get; set; }
        protected Vector3 EyeDir { get; set; }
        protected Matrix4x4 Mvpw { get; set; }
        protected Matrix4x4 InverseMvpw { get; set; }

        private List<LineSegmentIntersector.Intersection> _hitList = new List<LineSegmentIntersector.Intersection>();
        public IReadOnlyList<LineSegmentIntersector.Intersection> HitList => _hitList;

        public static IPointerInfo Create()
        {
            return new PointerInfo();
        }

        protected PointerInfo()
        {
            Reset();
        }
        
        public void Reset()
        {
            _hitList.Clear();
            SetCamera(null);
        }

        private void SetCamera(ICamera camera)
        {
            if (null != camera)
            {
                Mvpw = camera.ViewMatrix.PostMultiply(camera.ProjectionMatrix);
                if (null != camera.Viewport)
                {
                    Mvpw.PostMultiply(camera.Viewport.ComputeWindowMatrix4X4());
                }

                if (Matrix4x4.Invert(Mvpw, out var inverseMvpw))
                {
                    InverseMvpw = inverseMvpw;
                }

                camera.ViewMatrix.GetLookAt(out var eye, out var center, out var up, 1.0f);
                EyeDir = eye - center;
            }
            else
            {
                Mvpw = Matrix4x4.Identity;
                InverseMvpw = Matrix4x4.Identity;
                EyeDir = Vector3.UnitZ;
            }
        }
    }
}