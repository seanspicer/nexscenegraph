using System.Collections.Generic;

namespace Veldrid.SceneGraph
{
    public interface IDrawable : IObject
    {
        string Name { get; set; }
        IBoundingBox InitialBoundingBox { get; set; }
        VertexLayoutDescription VertexLayout { get; set; }
        List<IPrimitiveSet> PrimitiveSets { get; }
        PipelineState PipelineState { get; set; }
        bool HasPipelineState { get; }
        void ConfigureDeviceBuffers(GraphicsDevice device, ResourceFactory factory);
        DeviceBuffer GetVertexBufferForDevice(GraphicsDevice device);
        DeviceBuffer GetIndexBufferForDevice(GraphicsDevice device);
        void DirtyBound();
        IBoundingBox GetBoundingBox();
    }
}