

using System;
using System.Linq;
using System.Numerics;
using Veldrid.SceneGraph.Manipulators.Commands;
using Veldrid.SceneGraph.Util;
using Vulkan;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface IDraggerTransformCallback : IDraggerCallback
    {
        [Flags]
        enum HandleCommandMask
        {
            HandleNone = 0,
            HandleTranslateInLine = 1 << 0,
            HandleTranslateInPlane = 1 << 1,
            HandleScaled1D = 1 << 2,
            HandleScaled2D = 1 << 3,
            HandleScaledUniform = 1 << 4,
            HandleRotate3D = 1 << 5,
            HandleAll = 0x8ffffff
        }

        IMatrixTransform Transform { get; }
        
    }

    public class DraggerTransformCallback : DraggerCallback, IDraggerTransformCallback
    {
        public IMatrixTransform Transform { get; protected set; }
        protected Matrix4x4 StartMotionMatrix { get; set; }
        
        protected Matrix4x4 LocalToWorld { get; set; }
        protected Matrix4x4 WorldToLocal { get; set; }
        
        protected IDraggerTransformCallback.HandleCommandMask HandleCommandMask { get; set; }
        
        public static IDraggerTransformCallback Create(IMatrixTransform transform,
            IDraggerTransformCallback.HandleCommandMask handleCommandMask =
                IDraggerTransformCallback.HandleCommandMask.HandleAll)
        {
            return new DraggerTransformCallback(transform, handleCommandMask);
        }

        protected DraggerTransformCallback(IMatrixTransform transform,
            IDraggerTransformCallback.HandleCommandMask handleCommandMask =
                IDraggerTransformCallback.HandleCommandMask.HandleAll)
        {
            Transform = transform;
            HandleCommandMask = handleCommandMask;
        }


        
        public override bool Receive(IMotionCommand command)
        {
            if (null == Transform) return false;

            switch (command.Stage)
            {
                case IMotionCommand.MotionStage.Start:
                {
                    // Save the current matrix
                    StartMotionMatrix = Transform.Matrix;
                    
                    // Get the LocalToWorld and WorldToLocal matrix for this node. 
                    var nodePathToRoot = Util.ComputeNodePathToRoot(Transform);
                    LocalToWorld = Veldrid.SceneGraph.Transform.ComputeLocalToWorld(nodePathToRoot);
                    if (Matrix4x4.Invert(LocalToWorld, out var worldToLocal))
                    {
                        WorldToLocal = worldToLocal;
                    }

                    return true;
                }
                case IMotionCommand.MotionStage.Move:
                {
                    // Transform the command's motion matrix in to a local motion matrix.
                    var localMotionMatrix = LocalToWorld
                        .PostMultiply(command.GetWorldToLocal())
                        .PostMultiply(command.GetMotionMatrix())
                        .PostMultiply(command.GetLocalToWorld())
                        .PostMultiply(WorldToLocal);
                    
                    // Transform by the local motion matrix
                    Transform.Matrix = localMotionMatrix.PostMultiply(StartMotionMatrix);

                    return true;
                }
                case IMotionCommand.MotionStage.Finish:
                {
                    return true;
                }
                case IMotionCommand.MotionStage.None:
                {
                    return false;
                }
                default:
                {
                    return false;
                }
            }
        }
        
        public override bool Receive(ITranslateInLineCommand command)
        {
            return FlagsHelper.IsSet(HandleCommandMask, IDraggerTransformCallback.HandleCommandMask.HandleTranslateInLine) && Receive(command as IMotionCommand);
        }

        public override bool Receive(ITranslateInPlaneCommand command)
        {
            return FlagsHelper.IsSet(HandleCommandMask, IDraggerTransformCallback.HandleCommandMask.HandleTranslateInPlane) && Receive(command as IMotionCommand);
        }

        public override bool Receive(IScale1DCommand command)
        {
            return FlagsHelper.IsSet(HandleCommandMask, IDraggerTransformCallback.HandleCommandMask.HandleScaled1D) && Receive(command as IMotionCommand);
        }

        public override bool Receive(IScale2DCommand command)
        {
            return FlagsHelper.IsSet(HandleCommandMask, IDraggerTransformCallback.HandleCommandMask.HandleScaled2D) && Receive(command as IMotionCommand);
        }

        public override bool Receive(IScaleUniformCommand command)
        {
            return FlagsHelper.IsSet(HandleCommandMask, IDraggerTransformCallback.HandleCommandMask.HandleScaledUniform) && Receive(command as IMotionCommand);
        }

        public override bool Receive(IRotate3DCommand command)
        {
            return FlagsHelper.IsSet(HandleCommandMask, IDraggerTransformCallback.HandleCommandMask.HandleRotate3D) && Receive(command as IMotionCommand);
        }
        
    }
}