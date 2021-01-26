
using System.Collections.Generic;
using System.Numerics;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IPointerInfo
    {
        
    }
    
    public class PointerInfo : IPointerInfo
    {
        protected Vector3 NearPoint { get; set; }
        protected Vector3 FarPoint { get; set; }
        protected Vector3 EyeDir { get; set; }
        protected Matrix4x4 Mvpw { get; set; }
        protected Matrix4x4 InverseMvpw { get; set; }
        
        public IReadOnlyList<LineSegmentIntersector.Intersection> HitList { get; set; }
    }
}