

using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IDraggerTransformCallback : IDraggerCallback
    {
        enum HandleCommandMask
        {
            HandleTranslateInLine     = 1<<0,
            HandleTranslateInPlane    = 1<<1,
            HandleScaled1D            = 1<<2,
            HandleScaled2D            = 1<<3,
            HandleScaledUniform       = 1<<4,
            HandleRotate3D            = 1<<5,
            HandleAll                 = 0x8ffffff
        }
    }
    
    public class DraggerTransformCallback : DraggerCallback, IDraggerTransformCallback
    {
        public IDraggerTransformCallback Create(MatrixTransform transform,
            IDraggerTransformCallback.HandleCommandMask handleCommandMask =
                IDraggerTransformCallback.HandleCommandMask.HandleAll)
        {
            return new DraggerTransformCallback(transform, handleCommandMask);
        }

        protected DraggerTransformCallback(MatrixTransform transform,
            IDraggerTransformCallback.HandleCommandMask handleCommandMask =
                IDraggerTransformCallback.HandleCommandMask.HandleAll)
        {
            
        }
    }
}