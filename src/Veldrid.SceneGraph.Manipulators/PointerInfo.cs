
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IPointerInfo
    {
        IReadOnlyList<Tuple<NodePath, Vector3>> HitList { get; }
        
        void Reset();

        void AddIntersection(NodePath nodePath, Vector3 intersectionPoint);

        void SetCamera(ICamera camera);

        void SetMousePosition(float pixelX, float pixelY);

        bool Contains(INode node);
        
        Vector3 NearPoint { get; }
        Vector3 FarPoint { get; }
        Vector3 EyeDir { get; }

        void Next();
        bool Completed();
    }
    
    public class PointerInfo : IPointerInfo
    {
        public Vector3 NearPoint { get; protected set; }
        public Vector3 FarPoint { get; protected set; }
        public Vector3 EyeDir { get; protected set; }
        protected Matrix4x4 Mvpw { get; set; }
        protected Matrix4x4 InverseMvpw { get; set; }
        
        protected Matrix4x4 Mvp { get; set; }
        protected Matrix4x4 InverseMvp { get; set; }
        
        protected float PixelX { get; set; }
        protected float PixelY { get; set; }

        private List<Tuple<NodePath, Vector3>> _hitList = new List<Tuple<NodePath, Vector3>>();
        private IEnumerator<Tuple<NodePath, Vector3>> _hitIter;
        
        public IReadOnlyList<Tuple<NodePath, Vector3>> HitList => _hitList;
        
        private bool _isCompleted = false;
        
        public static IPointerInfo Create()
        {
            return new PointerInfo();
        }

        public static IPointerInfo Create(IPointerInfo pointerInfo)
        {
            return new PointerInfo(pointerInfo);
        }
        
        protected PointerInfo()
        {
            Reset();
        }

        protected PointerInfo(IPointerInfo other) : this()
        {
            foreach (var elt in other.HitList)
            {
                _hitList.Add(elt);
            }

            if (_hitList.Count > 0)
            {
                _hitIter = _hitList.GetEnumerator();
                _hitIter.MoveNext();
                _isCompleted = false;
            }
            
            NearPoint = other.NearPoint;
            FarPoint = other.FarPoint;
            EyeDir = other.EyeDir;
            
        }
        
        public void Reset()
        {
            _hitList.Clear();
            _hitIter = HitList.GetEnumerator();
            _isCompleted = false;
            SetCamera(null);
        }

        public void Next()
        {
            _isCompleted = !_hitIter.MoveNext();
        }

        public bool Completed()
        {
            return _isCompleted;
        }

        public void AddIntersection(NodePath nodePath, Vector3 intersectionPoint)
        {
            _hitList.Add(Tuple.Create(nodePath, intersectionPoint));
            _hitIter = _hitList.GetEnumerator();
            _hitIter.MoveNext();
            _isCompleted = false;
        }
        
        public void SetCamera(ICamera camera)
        {
            if (null != camera)
            {
                Mvpw = camera.ViewMatrix.PostMultiply(camera.ProjectionMatrix);
                if (null != camera.Viewport)
                {
                    Mvpw = Mvpw.PostMultiply(camera.Viewport.ComputeWindowMatrix4X4());
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
            PixelX = pixelX;
            PixelY = pixelY;
            ProjectWindowXyIntoObject(new Vector2(pixelX, pixelY));
        }

        public bool Contains(INode node)
        {
            if (null != node && null != _hitIter.Current)
            {
                var nodePath = _hitIter.Current.Item1;
                foreach (var hitNode in nodePath)
                {
                    if (hitNode == node)
                    {
                        return true;
                    }
                }
                
            }

            return false;
        }

        protected bool ProjectWindowXyIntoObject(Vector2 windowCoord)
        {
            NearPoint = InverseMvpw.PreMultiply(new Vector3(windowCoord.X, windowCoord.Y, 0.0f));
            FarPoint = InverseMvpw.PreMultiply(new Vector3(windowCoord.X, windowCoord.Y, 1.0f));

            return true;
        }
        
        
        
    }
}