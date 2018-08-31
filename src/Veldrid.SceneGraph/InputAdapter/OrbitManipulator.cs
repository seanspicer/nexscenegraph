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

        private float _distance = 1.0f;
        private float _trackballSize = 0.8f;
        
        public OrbitManipulator()
        {
            
        }

        protected virtual void RotateTrackball(float px0, float py0, float px1, float py1, float scale)
        {
            throw new NotImplementedException();
        }
        
        private void Trackball( Vector3 axis, float angle, float p1x, float p1y, float p2x, float p2y )
        {
            throw new NotImplementedException();
        }
    }
}