using System;
using System.Collections.Generic;

namespace Veldrid.SceneGraph
{
    public interface IGeode : INode
    {
        IReadOnlyList<IDrawable> Drawables { get; }
        IBoundingBox GetBoundingBox();
        event Func<INode, IBoundingBox> ComputeBoundingBoxCallback;
        void AddDrawable(IDrawable drawable);
    }
}