//
// Copyright (c) 2018 Sean Spicer
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
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
        
        public OrbitManipulator()
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
                var xNorm = 2.0f*(_inputStateTracker.MousePosition.Value.X / _inputStateTracker.FrameSnapshot.WindowWidth)-1.0f;
                var yNorm = -2.0f*(_inputStateTracker.MousePosition.Value.Y / _inputStateTracker.FrameSnapshot.WindowHeight)+1.0f;
                
                var xNormLast = 2.0f*(_inputStateTracker.LastMousePosition.Value.X / _inputStateTracker.FrameSnapshot.WindowWidth)-1.0f;
                var yNormLast = -2.0f*(_inputStateTracker.LastMousePosition.Value.Y / _inputStateTracker.FrameSnapshot.WindowHeight)+1.0f;

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
    }
}