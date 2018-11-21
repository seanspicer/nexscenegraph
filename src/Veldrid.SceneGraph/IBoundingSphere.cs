using System.Numerics;

namespace Veldrid.SceneGraph
{
    public interface IBoundingSphere
    {
        Vector3 Center { get; set; }
        float Radius { get; set; }
        float Radius2 { get; }
        void Init();
        void Set(Vector3 center, float radius);
        bool Valid();
        int GetHashCode();
        bool Equals(object Obj);

        /// <summary>
        /// Expand the sphere to include the coordinate v.  Repositions
        /// The sphere center to minimize the radius increase.
        /// </summary>
        /// <param name="v"></param>
        void ExpandBy(Vector3 v);

        /// <summary>
        /// Expand the sphere to include the coordinate v.  Does not
        /// reposition the sphere center.
        /// </summary>
        /// <param name="v"></param>
        void ExpandRadiusBy(Vector3 v);

        /// <summary>
        /// Expand bounding sphere to include sh. Repositions
        /// The sphere center to minimize the radius increase.
        /// </summary>
        /// <param name="sh"></param>
        void ExpandBy(IBoundingSphere sh);

        /// <summary>
        /// Expand the bounding sphere by sh. Does not
        /// reposition the sphere center.
        /// </summary>
        /// <param name="sh"></param>
        void ExpandRadiusBy(IBoundingSphere sh);

        /// <summary>
        /// Expand bounding sphere to include bb. Repositions
        /// The sphere center to minimize the radius increase.
        /// </summary>
        /// <param name="sh"></param>
        void ExpandBy(IBoundingBox bb);

        /// <summary>
        /// Expand the bounding sphere by bb. Does not
        /// reposition the sphere center.
        /// </summary>
        /// <param name="sh"></param>
        void ExpandRadiusBy(IBoundingBox bb);
    }
}