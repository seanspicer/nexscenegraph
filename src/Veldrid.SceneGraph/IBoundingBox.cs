using System;
using System.Numerics;

namespace Veldrid.SceneGraph
{
    public interface IBoundingBox
    {
        float XMin { get; }
        float YMin { get; }
        float ZMin { get; }
        float XMax { get; }
        float YMax { get; }
        float ZMax { get; }
        
        Vector3 Min { get; }
        Vector3 Max { get; }
        
        Vector3 Center { get; }
        float Radius { get; }
        float RadiusSquared { get; }
        void Init();

        void Set(
            float xmin, float ymin, float zmin,
            float xmax, float ymax, float zmax);

        void Set(Vector3 min, Vector3 max);
        int GetHashCode();
        bool Equals(object Obj);

        /// <summary>
        /// Returns true if the bounding box extents are valid, false otherwise
        /// </summary>
        /// <returns></returns>
        bool Valid();

        /// <summary>
        /// Returns a specific corner of the bounding box.
        /// pos specifies the corner as a number between 0 and 7.
        /// Each bit selects an axis, X, Y, or Z from least- to
        /// most-significant. Unset bits select the minimum value
        /// for that axis, and set bits select the maximum.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        Vector3 Corner(uint pos);

        /// <summary>
        /// Expand the bounding box to include the given coordinate v.
        /// </summary>
        /// <param name="v"></param>
        void ExpandBy(Vector3 v);

        /// <summary>
        /// Expand the bounding box to include the coordinate (x, y, z)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        void ExpandBy(float x, float y, float z);

        /// <summary>
        /// Expand the bounding box to include the given bounding box.
        /// </summary>
        /// <param name="bb"></param>
        void ExpandBy(IBoundingBox bb);

        /// <summary>
        /// Expand the bounding box to include the given bounding sphere
        /// </summary>
        /// <param name="sh"></param>
        /// <exception cref="NotImplementedException"></exception>
        void ExpandBy(IBoundingSphere sh);

        IBoundingBox Intersect(IBoundingBox bb);
        bool Intersects(IBoundingBox bb);
        bool Contains(Vector3 v);
        bool Contains(Vector3 v, float epsilon);
    }
}