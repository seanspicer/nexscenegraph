using System;

namespace Veldrid.SceneGraph
{
    public interface IPrimitiveSet : IObject
    {
        BoundingBox InitialBoundingBox { get; set; }
        IDrawable Drawable { get; }
        PrimitiveTopology PrimitiveTopology { get; set; }
        event Func<PrimitiveSet, BoundingBox> ComputeBoundingBoxCallback;
        void DirtyBound();
        BoundingBox GetBoundingBox();
        void Draw(CommandList commandList);
    }
}