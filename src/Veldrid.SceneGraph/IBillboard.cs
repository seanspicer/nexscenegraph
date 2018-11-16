using System.Numerics;

namespace Veldrid.SceneGraph
{
    public interface IBillboard : IGeode
    {
        Billboard.Modes Mode { get; set; }
        Matrix4x4 ComputeMatrix(Matrix4x4 modelView, Vector3 eyeLocal);
    }
}