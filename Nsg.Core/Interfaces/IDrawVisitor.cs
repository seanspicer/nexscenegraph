namespace Nsg.Core.Interfaces
{
    public interface IDrawVisitor
    {
        void Draw<T>(Geometry<T> geometry) where T : struct;
    }
}