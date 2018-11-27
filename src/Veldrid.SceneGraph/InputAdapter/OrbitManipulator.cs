//
// Copyright 2018 Sean Spicer 
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

namespace Veldrid.SceneGraph.InputAdapter
{
    public class OrbitManipulator : CameraManipulator
    {
        protected bool VerticalAxisFixed { get; set; } = true;
        protected float MinimumDistance { get; set; } = 0.05f;
        protected float WheelZoomFactor { get; set; } = 0.01f;

        private Quaternion _rotation = Quaternion.Identity;
        private Vector3 _center = Vector3.Zero;
        private float _distance = 5.0f;
        private float _trackballSize = 0.8f;

        public static ICameraManipulator Create()
        {
            return new OrbitManipulator();
        } 
       
        protected OrbitManipulator()
        {
            
        }

        protected override Matrix4x4 InverseMatrix => Matrix4x4.CreateTranslation(-_center) *
                                                      Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(_rotation)) *
                                                      Matrix4x4.CreateTranslation(new Vector3(0.0f, 0.0f, -_distance));

        
        
        protected override bool PerformMovementLeftMouseButton(float dx, float dy)
        {
            // rotate camera
            if (VerticalAxisFixed)

                // TODO: Implement Orbit Calculation
                throw new NotImplementedException();
            else

            {
                var xNorm = 2.0f*(InputStateTracker.MousePosition.Value.X / InputStateTracker.FrameSnapshot.WindowWidth)-1.0f;
                var yNorm = -2.0f*(InputStateTracker.MousePosition.Value.Y / InputStateTracker.FrameSnapshot.WindowHeight)+1.0f;
                
                var xNormLast = 2.0f*(InputStateTracker.LastMousePosition.Value.X / InputStateTracker.FrameSnapshot.WindowWidth)-1.0f;
                var yNormLast = -2.0f*(InputStateTracker.LastMousePosition.Value.Y / InputStateTracker.FrameSnapshot.WindowHeight)+1.0f;

                RotateTrackball(xNorm, yNorm, xNormLast, yNormLast, 1.0f);
            }
                
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
            angle = (float) Math.Asin(t);
        }

        private float ProjectToSphere(float r, float x, float y)
        {
            double z;

            var d = Math.Sqrt(x*x + y*y);
            
            /* Inside sphere */
            if (d < r * 0.70710678118654752440)
            {
                z = Math.Sqrt(r*r - d*d);
            } 
            
            /* On hyperbola */
            else
            {
                var t = r / 1.41421356237309504880;
                z = t*t / d;
            }
            return (float)z;
        }
        
        protected override void HandleWheelDelta()
        {
            ZoomModel(WheelZoomFactor *InputStateTracker.FrameSnapshot.WheelDelta, true);
            RequestRedraw();
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
        
        public override void HandleInput(IInputStateSnapshot snapshot)
        {
            base.HandleInput(snapshot);
            
            foreach (var keyEvent in snapshot.KeyEvents)
            {
                if (keyEvent.Down)
                {
                    switch (keyEvent.Key)
                    {
                        case Key.V:
                            ViewAll();
                            break;
                    }
                    
                }
            }
        }

        public override void ViewAll(float slack = 1)
        {
            // Find the bounding sphere of the scene
            var sceneView = _camera.View as SceneGraph.Viewer.IView;  // TODO: fixme this is just bad.
            var bSphere = sceneView.SceneData.GetBound();
            var radius = bSphere.Radius;
            var center = bSphere.Center;

            // Compute an aspect-radius to ensure that the 
            // scene will be inside the viewing volume
            var aspect = _camera.AspectRatio;
            if (aspect >= 1.0) {
                aspect = 1.0f;
            }
            var aspectRadius = radius / aspect;

            // Compute the direction of motion for the camera
            // between it's current position and the scene center
            var direction = _camera.Position - center; 
            var normDirection = Vector3.Normalize(direction);

            // Compute the length to move the camera by examining
            // the tangent to the bounding sphere
            var moveLen = radius + aspectRadius / Math.Tan(_camera.Fov / 2.0);

            // Compute the new camera position
            var moveDirection = normDirection * (float)moveLen;
            var cameraPos = center + moveDirection;

            // Compute the near and far plane locations
            const double epsilon = 0.001;
            var distToMid = (cameraPos - center).Length();
            var zNear = (float) Math.Max(distToMid * epsilon, distToMid - radius * slack);
            var zFar = distToMid + radius * slack;

            // Set the camera view and projection matrices.
            //Camera.SetViewMatrixToLookAt(cameraPos, center, new Vector3(0, 1, 0));
            _center = center;
            _distance = distToMid;
            _camera.SetProjectionMatrixAsPerspective(_camera.Fov, _camera.AspectRatio, zNear, zFar);
            
            RequestRedraw();
        }
    }
}