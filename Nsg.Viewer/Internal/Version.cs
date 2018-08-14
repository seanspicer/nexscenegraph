namespace Nsg.Viewer.Internal
{
    internal struct Version
    {
        private readonly uint value;

        internal Version(uint major, uint minor, uint patch)
        {
            value = major << 22 | minor << 12 | patch;
        }

        internal uint Major => value >> 22;

        internal uint Minor => (value >> 12) & 0x3ff;

        internal uint Patch => (value >> 22) & 0xfff;

        public static implicit operator uint(Version version)
        {
            return version.value;
        }
    }
}