//
// Copyright 2018-2019 Sean Spicer 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Numerics;
using Microsoft.Extensions.Logging;
using Veldrid.SceneGraph.Logging;
using Veldrid.SceneGraph.Util;
using Math = System.Math;

namespace Veldrid.SceneGraph.InputAdapter
{
    public class OrbitManipulator : StandardManipulator
    {
        protected float MinimumDistance { get; set; } = 0.05f;
        protected float WheelZoomFactor { get; set; } = 0.01f;

        private Quaternion _rotation = Quaternion.Identity;
        private Vector3 _center = Vector3.Zero;
        private float _distance = 5.0f;
        private float _trackballSize = 0.8f;

        public float Distance => _distance;
        
        public static ICameraManipulator Create()
        {
            return new OrbitManipulator();
        } 
       
        protected OrbitManipulator()
        {
            
        }
        
        private Matrix4x4 _viewMatrix = Matrix4x4.Identity;

        public override Matrix4x4 InverseMatrix =>
            Matrix4x4.CreateTranslation(-_center) *
            Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(_rotation)) *
            Matrix4x4.CreateTranslation(new Vector3(0.0f, 0.0f, -_distance));

        private float _zoomScale = 1;
        protected override float ZoomScale => _zoomScale;
        
        public override void SetTransformation(Vector3 eye, Vector3 center, Vector3 up, bool excludeRotation=false)
        {
            _viewMatrix = Matrix4x4.CreateLookAt(eye, center, up);
            
            var lv = center - eye;

            var f = Vector3.Normalize((new Vector3(lv.X, lv.Y, lv.Z)));

            var s = Vector3.Normalize(Vector3.Cross(f, up));

            var u = Vector3.Normalize(Vector3.Cross(s, f));

            var rotationMatrix = new Matrix4x4(
                s.X,  u.X, -f.X,  0.0f,
                s.Y,  u.Y, -f.Y,  0.0f,
                s.Z,  u.Z, -f.Z,  0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);

            _center = center;
            _distance = lv.Length();

            if (false == excludeRotation)
            {
                _rotation = Quaternion.Inverse(Quaternion.CreateFromRotationMatrix(rotationMatrix));
            }
            

            if (VerticalAxisFixed)
            {
                throw new NotImplementedException();
            }

        }

        protected override (Vector3, Vector3, Vector3) GetTransformation()
        {
            var eye = _center + _rotation.RotateVector(_distance * Vector3.UnitZ);
            var up = _rotation.RotateVector(Vector3.UnitY);

            return (eye, _center, up);
        }
        
        protected override bool PerformMovementLeftMouseButton(double eventTimeDelta, float dx, float dy)
        {
            // rotate camera
            if (VerticalAxisFixed)
            {
                // TODO: Implement Orbit Calculation
                throw new NotImplementedException();
            }
            else
            {
                RotateTrackball(
                    EventAdapterT0.XNormalized, 
                    EventAdapterT0.YNormalized, 
                    EventAdapterT1.XNormalized, 
                    EventAdapterT1.YNormalized, 
                    GetThrowScale(eventTimeDelta));
            }
                
            return true;
        }

        protected override bool PerformMovementRightMouseButton(double eventTimeDelta, float dx, float dy)
        {
            float scale = -0.3f * _distance * GetThrowScale( eventTimeDelta );
            PanModel( dx*scale, dy*scale );
            return true;
        }

        protected virtual void RotateTrackball(float px0, float py0, float px1, float py1, float scale)
        {
            Trackball( out var axis, out var angle, px0 + (px1-px0)*scale, py0 + (py1-py0)*scale, px0, py0 );
            
            _rotation = Quaternion.Multiply(Quaternion.CreateFromAxisAngle(axis, angle), _rotation);
        }
        
        private void Trackball( out Vector3 axis, out float angle, float p1x, float p1y, float p2x, float p2y )
        {
            var uv = Vector3.Transform(Vector3.UnitY, _rotation);
            var sv = Vector3.Transform(Vector3.UnitX, _rotation);
            var lv = Vector3.Transform(-Vector3.UnitZ, _rotation);
            
            var p1 = sv * p1x + uv * p1y - lv * ProjectToSphere(_trackballSize, p1x, p1y);
            var p2 = sv * p2x + uv * p2y - lv * ProjectToSphere(_trackballSize, p2x, p2y);
            
            // Compute normalized cross-product.
            axis = Vector3.Normalize(Vector3.Cross(p2,p1));
            
            var t = (p2 - p1).Length() / (2.0 * _trackballSize);

            // Avoid problems with out-of-control values.
            if (t > 1.0) t = 1.0;
            if (t < -1.0) t = -1.0;
            angle = (float)  System.Math.Asin(t);
        }

        private float ProjectToSphere(float r, float x, float y)
        {
            double z;

            var d =  System.Math.Sqrt(x*x + y*y);
            
            /* Inside sphere */
            if (d < r * 0.70710678118654752440)
            {
                z =  System.Math.Sqrt(r*r - d*d);
            } 
            
            /* On hyperbola */
            else
            {
                var t = r / 1.41421356237309504880;
                z = t*t / d;
            }
            return (float)z;
        }
        
        protected override bool HandleWheelDelta(IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter)
        {
            var sm = eventAdapter.ScrollingMotion;

            if ((_flags & UserInteractionFlags.SetCenterOnForwardWheelMovement) != 0)
            {
                if (((sm == IUiEventAdapter.ScrollingMotionType.ScrollDown && WheelZoomFactor > 0)) ||
                    ((sm == IUiEventAdapter.ScrollingMotionType.ScrollUp && WheelZoomFactor < 0)))
                {
                    if (GetAnimationTime() <= 0)
                    {
                        SetCenterByMousePointerIntersection(eventAdapter, actionAdapter);
                    }
                    else
                    {
                        if (!IsAnimating())
                        {
                            StartAnimationByMousePointerIntersection(eventAdapter, actionAdapter);
                        }
                    }
                }
            }

            switch (sm)
            {
                case IUiEventAdapter.ScrollingMotionType.ScrollUp:
                {
                    ZoomModel(WheelZoomFactor, true);
                    actionAdapter.RequestRedraw();
                    actionAdapter.RequestContinuousUpdate(IsAnimating() || _thrown);
                    return true;
                }
                case IUiEventAdapter.ScrollingMotionType.ScrollDown:
                {
                    ZoomModel(-WheelZoomFactor, true);
                    actionAdapter.RequestRedraw();
                    actionAdapter.RequestContinuousUpdate(IsAnimating() || _thrown);
                    return true;
                }
                
                default:
                    return false;
            }
            
            // TODO Implement me.
            //ZoomModel(WheelZoomFactor *InputStateTracker.FrameSnapshot.WheelDelta, true);
            //actionAdapter.RequestRedraw();
            //return false;
        }
        
        void PanModel(float p1x, float p1y, float p2x, float p2y)
        {
            // //throw new NotImplementedException();
            // var startNear = new Vector3(p1x, p1y, 0);
            //
            // var startFar = new Vector3(p1x, p1y, 1);
            // var endFar = new Vector3(p2x, p2y, 1);
            //
            // var worldStartNear = NormalizedScreenToWorld(startNear, snapshot.ProjectionMatrix, snapshot.ViewMatrix);
            // var worldStartFar = NormalizedScreenToWorld(startFar, snapshot.ProjectionMatrix, snapshot.ViewMatrix);
            //
            // var worldEndFar = NormalizedScreenToWorld(endFar, snapshot.ProjectionMatrix, snapshot.ViewMatrix);
            //
            // var lenFar = (worldEndFar - worldStartFar).Length();
            //
            // var motionDir = Vector3.Normalize(worldEndFar - worldStartFar);
            // var d = (worldStartFar - worldStartNear).Length();
            //
            // var motionLen = lenFar * _distance / d;
            //
            // _center += motionLen*motionDir;
        }

        void PanModel(float dx, float dy, float dz = 0.0f)
        {
            var rotationMatrix = Matrix4x4.CreateFromQuaternion(_rotation);
            var dv = new Vector3(dx, dy, dz);
            _center += rotationMatrix.PreMultiply(dv);
        }
        
        public Vector3 NormalizedScreenToWorld(Vector3 screenCoords, Matrix4x4 projectionMatrix, Matrix4x4 viewMatrix)
        {
            var viewProjectionMatrix = projectionMatrix.PreMultiply(viewMatrix);

            Matrix4x4 vpi;
            
            if (Matrix4x4.Invert(viewProjectionMatrix, out vpi))
            {
                var nc = new Vector3(screenCoords.X, screenCoords.Y, screenCoords.Z);
                var pc = vpi.PreMultiply(nc);

                return pc;
                
            }
            else
            {
                throw new Exception("Cannot invert view-projection matrix");
            }
            
        }
        
        void ZoomModel(float dy, bool pushForwardIfNeeded )
        {
            // scale
            var scale = 1.0f + dy;

            // minimum distance
            float minDist = MinimumDistance;
            
            // TODO - Implement below
            //if( getRelativeFlag( _minimumDistanceFlagIndex ) )
            //    minDist *= _modelSize;

            if( _distance*scale > minDist )
            {
                // regular zoom
                _distance *= scale;
                _zoomScale *= scale;
            }
            else
            {
                if( pushForwardIfNeeded )
                {
                    // push the camera forward
                    float yscale = -_distance;
                    var dv = Vector3.Transform(new Vector3( 0.0f, 0.0f, -1.0f ), _rotation) * (dy * yscale);
                    _center += dv;
                }
                else
                {
                    // set distance on its minimum value
                    _distance = minDist;
                }
            }
        }
        
        protected override bool HandleKeyDown(IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter)
        {

            if (eventAdapter.Key == IUiEventAdapter.KeySymbol.KeyV)
            {
                ViewAll(actionAdapter);
                actionAdapter.RequestRedraw();
                return true;
            }

            return base.HandleKeyDown(eventAdapter, actionAdapter);
        }

        public override void ViewAll(IUiActionAdapter aa, float slack = 20)
        {
            if (aa is Viewer.IView view)
            {

                var cbv = ComputeBoundsVisitor.Create();
                GetNode().Accept(cbv);
                
                var bSphere = BoundingSphere.Create();
                bSphere.ExpandBy(cbv.GetBoundingBox());
                if (bSphere.Radius < 0) return;

                var radius = bSphere.Radius;
                var center = bSphere.Center;
                
                switch (view.Camera)
                {
                    case IPerspectiveCamera perspectiveCamera:
                    {
                        // Compute an aspect-radius to ensure that the 
                        // scene will be inside the viewing volume
                        var aspect = view.Camera.Viewport.AspectRatio;
                        if (aspect >= 1.0)
                        {
                            aspect = 1.0f;
                        }

                        var aspectRadius = radius / aspect;

                        Vector3 camEye;
                        Vector3 camCenter;
                        Vector3 camUp;

                        perspectiveCamera.ProjectionMatrix.GetLookAt(out camEye, out camCenter, out camUp, 1);

                        // Compute the direction of motion for the camera
                        // between it's current position and the scene center
                        var direction = camEye - camCenter;
                        var normDirection = Vector3.Normalize(direction);

                        // Compute the length to move the camera by examining
                        // the tangent to the bounding sphere
                        var moveLen = radius + aspectRadius / ( System.Math.Tan(perspectiveCamera.VerticalFov / 2.0));

                        // Compute the new camera position
                        var moveDirection = normDirection * (float) moveLen;
                        var cameraPos = center + moveDirection;

                        // Compute the near and far plane locations
                        const double epsilon = 0.001;
                        var distToMid = (cameraPos - center).Length();
                        var zNear = (float)  System.Math.Max(distToMid * epsilon, distToMid - radius * slack);
                        var zFar = distToMid + radius * slack;

                        _center = center;
                        _distance = distToMid;

                        perspectiveCamera.SetProjectionMatrixAsPerspective(perspectiveCamera.VerticalFov,
                            perspectiveCamera.Viewport.AspectRatio, zNear, zFar);
                        break;
                    }
                    case IOrthographicCamera orthoCamera:
                    {
                        _zoomScale = 1.0f;
                        const float winScale = 2.0f;
                        var width = radius * winScale  * view.Camera.Viewport.AspectRatio * ZoomScale;
                        var height = radius * winScale * ZoomScale;
                        var zNear = winScale * radius;
                        var zFar = -winScale * radius;
                        
                        var vertical2 =  System.Math.Abs(width) / zNear / 2f;
                        var horizontal2 =  System.Math.Abs(height) / zNear / 2f;
                        var dim = horizontal2 < vertical2 ? horizontal2 : vertical2;
                        var viewAngle =  System.Math.Atan2(dim,1f);
                        var dist = (float) (radius /  System.Math.Sin(viewAngle));
                        
                        orthoCamera.SetProjectionMatrixAsOrthographic(width, height, zNear, zFar);

                        _center = center;
                        _distance = dist;
                        
                        break;
                    }
                    
                }
            }
            // Fallback - should really not be called.
            else
            {
                LogManager.LoggerFactory.CreateLogger<OrbitManipulator>().LogWarning("In ViewAll() fallback -- this should not be called.");
                ComputeHomePosition(null, (_flags & UserInteractionFlags.ComputeHomeUsingBoundingBox) != 0);
                SetTransformation( _homeEye, _homeCenter, _homeUp, excludeRotation: true );
            }
        }
    }
}