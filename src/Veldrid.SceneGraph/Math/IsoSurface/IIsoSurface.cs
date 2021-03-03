using System.Collections.Generic;

namespace Veldrid.SceneGraph.Math.IsoSurface
{
    public interface IVertex3D
    {
        double X { get; }
        double Y { get; }
        double Z { get; }
    }

    public interface IIsoSurface
    {
        double IsoValue { get; }

        LinkedList<IVertex3D> IsoSurfaceVertices { get; }
    }
}