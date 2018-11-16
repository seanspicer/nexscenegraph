using System;
using System.Collections.Generic;

namespace Veldrid.SceneGraph
{
    public interface IGeode : INode
    {
        IReadOnlyList<IDrawable> Drawables { get; }
        event Func<INode, BoundingBox> ComputeBoundingBoxCallback;
        void AddDrawable(IDrawable drawable);
    }
}