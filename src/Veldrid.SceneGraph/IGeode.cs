using System;
using System.Collections.Generic;

namespace Veldrid.SceneGraph
{
    public interface IGeode : INode
    {
        IReadOnlyList<Drawable> Drawables { get; }
        event Func<Node, BoundingBox> ComputeBoundingBoxCallback;
        void AddDrawable(Drawable drawable);
    }
}