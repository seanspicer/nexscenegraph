
using System.Numerics;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph
{
    public interface IAutoTransform : ITransform
    {
        enum AutoRotateModeType
        {
            NoRotation     = 1,
            RotateToScreen = 2,
            RotateToCamera = 3,
            RotateToAxis   = 4
        }
        
        AutoRotateModeType AutoRotateMode { get; set; }
        Vector3 Position { get; set; }
        Quaternion Rotation { get; set; }
        Vector3 Scale { get; set; }
        Vector3 PivotPoint { get; set; }
        Vector3 Axis { get; set; }
        Vector3 Normal { get; set; }
        
        bool AutoScaleToScreen { get; set; }
        float AutoScaleTransitionWidthRatio { get; set; }
        
        float MinimumScale { get; set; }
        float MaximumScale { get; set; }
        
        float AutoUpdateEyeMovementTolerance { get; set; }
        
    }
    
    public class AutoTransform : Transform, IAutoTransform
    {
        protected enum AxisAligned
        {
            AxialRotXAxis = IAutoTransform.AutoRotateModeType.RotateToAxis+1,
            AxialRotYAxis = IAutoTransform.AutoRotateModeType.RotateToAxis+2,
            AxialRotZAxis = IAutoTransform.AutoRotateModeType.RotateToAxis+3,
            CacheDirty = IAutoTransform.AutoRotateModeType.RotateToAxis+4
        }

        private IAutoTransform.AutoRotateModeType _autoRotateMode = IAutoTransform.AutoRotateModeType.NoRotation;

        public IAutoTransform.AutoRotateModeType AutoRotateMode
        {
            get => _autoRotateMode;
            set
            {
                _autoRotateMode = value;
                CachedMode = (int) AxisAligned.CacheDirty;
                UpdateCache();
            }
        }
        
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        private Vector3 _scale = Vector3.One;
        public Vector3 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                if (_scale.X<MinimumScale) _scale.X = MinimumScale;
                if (_scale.Y<MinimumScale) _scale.Y = MinimumScale;
                if (_scale.Z<MinimumScale) _scale.Z = MinimumScale;

                if (_scale.X>MaximumScale) _scale.X = MaximumScale;
                if (_scale.Y>MaximumScale) _scale.Y = MaximumScale;
                if (_scale.Z>MaximumScale) _scale.Z = MaximumScale;
                DirtyBound();
            }
        }
        public Vector3 PivotPoint { get; set; }
        private Vector3 _axis = Vector3.UnitZ;

        public Vector3 Axis
        {
            get => _axis;
            set
            {
                _axis = Vector3.Normalize(value);
                UpdateCache();
            }
        }
        
        public Vector3 Normal { get; set; } = -1.0f * Vector3.UnitY;

        private bool _autoScaleToScreen = false;
        public bool AutoScaleToScreen
        {
            get => _autoScaleToScreen;
            set
            {
                _autoScaleToScreen = value;
                if (_autoScaleToScreen)
                {
                    CullingActive = false;
                }
            }
        }

        public float AutoScaleTransitionWidthRatio { get; set; } = 0.25f;

        public float MinimumScale { get; set; } = 0.0f;
        public float MaximumScale { get; set; } = float.MinValue;

        public float AutoUpdateEyeMovementTolerance { get; set; } = 0.0f;

        protected int CachedMode { get; set; } = (int) IAutoTransform.AutoRotateModeType.NoRotation;
        
        protected Vector3 Side { get; set; } = Vector3.UnitX;

        public new static IAutoTransform Create()
        {
            return new AutoTransform();
        }

        protected AutoTransform()
        {
            
        }
        
        public override bool ComputeLocalToWorldMatrix(ref Matrix4x4 matrix, NodeVisitor visitor)
        {
            if (ReferenceFrame == ReferenceFrameType.Relative)
            {
                matrix = matrix.PreMultiply(ComputeMatrix(visitor));
            }
            else
            {
                matrix = ComputeMatrix(visitor);
            }

            return true;
        }

        public override bool ComputeWorldToLocalMatrix(ref Matrix4x4 matrix, NodeVisitor visitor)
        {
            if (Matrix4x4.Invert(ComputeMatrix(visitor), out var inverse))
            {
                if (ReferenceFrame == ReferenceFrameType.Relative)
                {
                    matrix = matrix.PostMultiply(inverse);
                }
                else
                {
                    matrix = inverse;
                }

                return true;
            }

            return false;

        }

        protected Matrix4x4 ComputeMatrix(NodeVisitor visitor)
        {
            return Matrix4x4.Identity;
        }
        
        protected void UpdateCache()
        {
            if (_autoRotateMode == IAutoTransform.AutoRotateModeType.RotateToAxis)
            {
                if (Axis == Vector3.UnitX && Normal == (-1 * Vector3.UnitY))
                {
                    CachedMode = (int) AxisAligned.AxialRotXAxis;
                }
                else if (Axis == Vector3.UnitY && Normal == (Vector3.UnitX))
                {
                    CachedMode = (int) AxisAligned.AxialRotYAxis;
                }
                else if (Axis == Vector3.UnitZ && Normal == (-1 * Vector3.UnitY))
                {
                    CachedMode = (int) AxisAligned.AxialRotZAxis;
                }
                else
                {
                    CachedMode = (int) IAutoTransform.AutoRotateModeType.RotateToAxis;
                }
            }
            else
            {
                CachedMode = (int) AutoRotateMode;
            }

            Side = Vector3.Normalize(Vector3.Cross(Axis, Normal));
        }        
    }
}