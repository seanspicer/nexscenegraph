
using System;
using System.Numerics;
using Veldrid.SceneGraph.RenderGraph;
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
        public float MaximumScale { get; set; } = float.MaxValue;

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

        protected Matrix4x4 ComputeMatrix(INodeVisitor visitor)
        {
            Quaternion rotation = Rotation;
            Vector3 scale = Scale;
            
            
            
            // Do a bunch of stuff...
            if(visitor is ICullVisitor cullVisitor)
            {
                var eyePoint = cullVisitor.GetEyeLocal();
                var localUp = cullVisitor.GetUpLocal();

                if (AutoScaleToScreen)
                {
                    var size = 10.0f / cullVisitor.PixelSize(Position, 0.48f);
                    if (AutoScaleTransitionWidthRatio > 0)
                    {
                        if (MinimumScale > 0.0)
                        {
                            var j = MinimumScale;
                            var i = (MaximumScale<float.MaxValue) ?
                                MinimumScale+(MaximumScale-MinimumScale)*AutoScaleTransitionWidthRatio :
                                MinimumScale*(1.0+AutoScaleTransitionWidthRatio);
                            var c = 1.0/(4.0*(i-j));
                            var b = 1.0 - 2.0*c*i;
                            var a = j + b*b / (4.0*c);
                            var k = -b / (2.0*c);

                            if (size < k)
                            {
                                size = MinimumScale;
                            }
                            else if (size < i)
                            {
                                size = (float)(a + b*size + c*(size*size));
                            }
                        }
                        if (MaximumScale<float.MaxValue)
                        {
                            var n = MaximumScale;
                            var m = (MinimumScale>0.0) ?
                                MaximumScale+(MinimumScale-MaximumScale)*AutoScaleTransitionWidthRatio :
                                MaximumScale*(1.0-AutoScaleTransitionWidthRatio);
                            var c = 1.0 / (4.0*(m-n));
                            var b = 1.0 - 2.0*c*m;
                            var a = n + b*b/(4.0*c);
                            var p = -b / (2.0*c);

                            if (size > p)
                            {
                                size = MaximumScale;
                            }
                            else if (size > m)
                            {
                                size = (float)(a + b*size + c*(size*size));
                            }
                        }
                    }
                    else
                    {
                        if (MinimumScale>0.0 && size<MinimumScale)
                        {
                            size = MinimumScale;
                        }

                        if (MaximumScale<float.MaxValue && size>MaximumScale)
                        {
                            size = MaximumScale;
                        }
                    }

                    scale = new Vector3(size, size, size);
                }

                if (AutoRotateMode == IAutoTransform.AutoRotateModeType.RotateToScreen)
                {
                    if (Matrix4x4.Decompose(cullVisitor.GetModelViewMatrix(), 
                        out var dScale, 
                        out var dRotation,
                        out var dTranslation))
                    {
                        rotation = Quaternion.Inverse(dRotation);
                    }
                }

                else if (AutoRotateMode == IAutoTransform.AutoRotateModeType.RotateToCamera)
                {
                    Vector3 posToEye = Position - eyePoint;
                    var lookAt = Matrix4x4.CreateLookAt(Vector3.Zero, posToEye, localUp);
                    if (Matrix4x4.Invert(lookAt, out var lookAtInverse))
                    {
                        rotation = Quaternion.CreateFromRotationMatrix(lookAtInverse);
                    }
                }
                
                else if (AutoRotateMode == IAutoTransform.AutoRotateModeType.RotateToAxis)
                {
                    var rMatrix = Matrix4x4.Identity;
                    var eyeVector = eyePoint - Position;
                    switch (CachedMode)
                    {
                        case (int) AxisAligned.AxialRotZAxis:
                            {
                                eyeVector.Z = 0.0f;
                                var evLength = eyeVector.Length();
                                if (evLength > 0.0f)
                                {
                                    var inv = 1.0f/evLength;
                                    var s = eyeVector.X*inv;
                                    var c = -eyeVector.Y*inv;
                                    rMatrix.M11 = c;
                                    rMatrix.M21 = -s;
                                    rMatrix.M12 = s;
                                    rMatrix.M22 = c;
                                }
                            } 
                            break;
                        case (int) AxisAligned.AxialRotYAxis:
                            {
                                eyeVector.Y = 0.0f;
                                var evLength = eyeVector.Length();
                                if (evLength > 0.0f)
                                {
                                    var inv = 1.0f/evLength;
                                    var s = eyeVector.Z*inv;
                                    var c = -eyeVector.X*inv;
                                    rMatrix.M11 = c;
                                    rMatrix.M31 = s;
                                    rMatrix.M13 = -s;
                                    rMatrix.M33 = c;
                                }
                            } 
                            break;
                        case (int) AxisAligned.AxialRotXAxis:
                            {
                                eyeVector.X = 0.0f;
                                var evLength = eyeVector.Length();
                                if (evLength > 0.0f)
                                {
                                    var inv = 1.0f/evLength;
                                    var s = eyeVector.Z*inv;
                                    var c = -eyeVector.Y*inv;
                                    rMatrix.M22 = c;
                                    rMatrix.M32 = -s;
                                    rMatrix.M23 = s;
                                    rMatrix.M33 = c;
                                }
                            } break;
                        case (int) IAutoTransform.AutoRotateModeType.RotateToAxis:
                            {
                                var evSide = Vector3.Dot(eyeVector, Side);
                                var evNormal = Vector3.Dot(eyeVector, Normal);
                                var angle = (float) System.Math.Atan2(evSide, evNormal);
                                rMatrix = Matrix4x4.CreateFromAxisAngle(Axis, angle);
                            }
                            break;
                        
                    }

                    rotation = Quaternion.CreateFromRotationMatrix(rMatrix);
                }
            }
            
            Rotation = rotation;
            Scale = scale;

            Matrix4x4 matrix = Matrix4x4.CreateFromQuaternion(rotation);
            matrix = matrix.PostMultiplyTranslate(Position);
            matrix = matrix.PreMultiplyScale(Scale);
            matrix = matrix.PreMultiplyTranslate(-1 * PivotPoint);

            return matrix;
            
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