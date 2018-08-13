using System;

namespace Nsg.Core.Interfaces
{
    public interface IGraphicsDevice
    {
        IResourceFactory ResourceFactory { get; }

        void UpdateBuffer<T>(IDeviceBuffer buffer, uint bufferOffsetInBytes, T[] source) where T : struct;
    }
}