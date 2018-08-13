using System;

namespace Nsg.Core.Interfaces
{
    public interface IDeviceBuffer : IDisposable
    {
        string Name { get; set; }
        uint SizeInBytes { get; }
    }
}