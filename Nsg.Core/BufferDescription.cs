namespace Nsg.Core
{
    public class BufferDescription
    {
        public uint SizeInBytes { get; private set; }
        public BufferUsage Usage { get; private set; }

        public BufferDescription(uint sizeInBytes, BufferUsage usage)
        {
            SizeInBytes = sizeInBytes;
            Usage = usage;
        }
        
    }
}