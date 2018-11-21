using System;

namespace Veldrid.SceneGraph
{
    public interface IPrimitiveSet : IObject
    {
        IBoundingBox InitialBoundingBox { get; set; }
        IDrawable Drawable { get; }
        PrimitiveTopology PrimitiveTopology { get; set; }
        event Func<PrimitiveSet, IBoundingBox> ComputeBoundingBoxCallback;
        void DirtyBound();
        IBoundingBox GetBoundingBox();
        void Draw(CommandList commandList);
    }
}