
using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IPointerInfo
    {
        IList<Tuple<NodePath, Vector3>> HitList { get; }
        
        void Reset();

        void SetCamera(ICamera camera);

        void SetMousePosition(float pixelX, float pixelY);
    }
    
    public class PointerInfo : IPointerInfo
    {
        protected Vector3 NearPoint { get; set; }
        protected Vector3 FarPoint { get; set; }
        protected Vector3 EyeDir { get; set; }
        protected Matrix4x4 Mvpw { get; set; }
        protected Matrix4x4 InverseMvpw { get; set; }

        //private List<Tuple<NodePath, Vector3>> _hitList = new List<Tuple<NodePath, Vector3>>();
        public IList<Tuple<NodePath, Vector3>> HitList { get; set; }= new List<Tuple<NodePath, Vector3>>();//HitList => _hitList;

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
            HitList.Clear();
            SetCamera(null);
        }

        public void SetCamera(ICamera camera)
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

        public void SetMousePosition(float pixelX, float pixelY)
        {
            ProjectWindowXyIntoObject(new Vector2(pixelX, pixelY));
        }

        protected bool ProjectWindowXyIntoObject(Vector2 windowCoord)
        {
            NearPoint = InverseMvpw.PreMultiply(new Vector3(windowCoord.X, windowCoord.Y, 0.0f));
            FarPoint = InverseMvpw.PreMultiply(new Vector3(windowCoord.X, windowCoord.Y, 1.0f));

            return true;
        }
    }
}