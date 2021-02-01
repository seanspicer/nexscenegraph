

using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IProjector
    {
        bool Project(IPointerInfo pi, out Vector3 projectedPoint);
        
        Matrix4x4 LocalToWorld { get; set; }
        Matrix4x4 WorldToLocal { get; }
        
    }
    
    public abstract class Projector : IProjector
    {
        protected bool WorldToLocalDirty = true;
        
        private Matrix4x4 _localToWorld = Matrix4x4.Identity;
        public Matrix4x4 LocalToWorld
        {
            get => _localToWorld;
            set
            {
                _localToWorld = value;
                WorldToLocalDirty = true;
            }
        }

        private Matrix4x4 _worldToLocal = Matrix4x4.Identity;
        public Matrix4x4 WorldToLocal
        {
            get
            {
                if (WorldToLocalDirty)
                {
                    if (Matrix4x4.Invert(_localToWorld, out var worldToLocal))
                    {
                        _worldToLocal = worldToLocal;
                        WorldToLocalDirty = false;
                    }
                }

                return _worldToLocal;
            }
        }

        
        public abstract bool Project(IPointerInfo pi, out Vector3 projectedPoint);
    }
}